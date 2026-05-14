using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ConvertApiDotNet.Constants;
using ConvertApiDotNet.Helpers;
using ConvertApiDotNet.Interface;
using ConvertApiDotNet.Model;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace ConvertApiDotNet.Services
{
    /// <summary>
    /// Default implementation that downloads and parses ConvertAPI OpenAPI spec and exposes a catalog of converters and tags.
    /// </summary>
    public class ConverterCatalog : IConverterCatalog
    {
        private readonly Uri _baseUri;
        private readonly IConvertApiHttpClient _http;

        private readonly SemaphoreSlim _loadSync = new SemaphoreSlim(1, 1);
        private volatile bool _loaded;
        private List<ConverterDto> _converters = new List<ConverterDto>();
        private List<TagDto> _tags = new List<TagDto>();

        public ConverterCatalog()
            : this(new Uri(ConvertApi.ApiBaseUri), ConvertApi.GetClient())
        {
        }

        public ConverterCatalog(Uri baseUri, IConvertApiHttpClient http)
        {
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public List<ConverterDto> GetAllConverters()
        {
            EnsureLoaded();
            return _converters;
        }

        public ConverterDto GetConverter(string sourceFormat, string destinationFormat)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(sourceFormat) || string.IsNullOrWhiteSpace(destinationFormat))
                return null;
            var src = TrimDot(sourceFormat);
            var dst = TrimDot(destinationFormat);
            return _converters.FirstOrDefault(c =>
                string.Equals(c.SourceFormat, src, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.DestinationFormat, dst, StringComparison.OrdinalIgnoreCase));
        }

        public List<ConverterDto> GetConvertersByTags(List<string> tags = null)
        {
            EnsureLoaded();
            if (tags == null || tags.Count == 0)
                return _converters;
            var wanted = tags.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToList();
            if (wanted.Count == 0) return _converters;
            return _converters.Where(c => ContainsAllTags(c.Tags, wanted)).ToList();
        }

        public List<ConverterDto> SearchConverters(string[] terms)
        {
            if (terms == null || terms.Length == 0)
                return new List<ConverterDto>();
            return SearchConverters(string.Join(" ", terms));
        }

        public List<ConverterDto> SearchConverters(string query)
        {
            return SearchConvertersAsync(query).GetAwaiter().GetResult();
        }

        public async Task<List<ConverterDto>> SearchConvertersAsync(string query)
        {
            // Short-circuit empty input — preserves the old SDK contract and saves a
            // round-trip. The server treats no-query as "return everything", which
            // would be surprising for a method named SearchConverters.
            if (string.IsNullOrWhiteSpace(query))
                return new List<ConverterDto>();

            var doc = await TryFetchOpenApiAsync("info/openapi", query).ConfigureAwait(false)
                      ?? await TryFetchOpenApiAsync("info/openApi", query).ConfigureAwait(false);
            if (doc == null)
                throw new InvalidOperationException(
                    "OpenAPI search response could not be retrieved. The server may not support " +
                    "the ?q= search filter — upgrade ConvertAPI server to a build that includes it.");

            // The server orders paths by relevance (x-ca-search-rank ascending) when
            // filtered, so parser document order is already the order we want.
            return ParseConverters(doc);
        }

        public List<TagDto> GetTags()
        {
            EnsureLoaded();
            return _tags;
        }

        public async Task Reload()
        {
            await LoadInternal(force: true).ConfigureAwait(false);
        }

        private void EnsureLoaded()
        {
            if (_loaded) return;
            // Avoid deadlocks by waiting on background thread
            LoadInternal(force: false).GetAwaiter().GetResult();
        }

        private async Task LoadInternal(bool force)
        {
            if (_loaded && !force) return;
            await _loadSync.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_loaded && !force) return;
                var doc = await TryFetchOpenApiAsync("info/openapi").ConfigureAwait(false)
                          ?? await TryFetchOpenApiAsync("info/openApi").ConfigureAwait(false);
                if (doc == null)
                    throw new InvalidOperationException("OpenAPI document could not be retrieved.");

                var parsedConverters = ParseConverters(doc);
                var parsedTags = ParseTags(doc);

                // Apply
                _converters = parsedConverters
                    .OrderBy(c => c.SourceFormat, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(c => c.DestinationFormat, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                _tags = parsedTags
                    .OrderBy(t => t.FriendlyName ?? t.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                _loaded = true;
            }
            finally
            {
                _loadSync.Release();
            }
        }

        private async Task<OpenApiDocument> TryFetchOpenApiAsync(string path, string searchQuery = null)
        {
            var ub = new UriBuilder(_baseUri) { Path = path };
            if (!string.IsNullOrEmpty(searchQuery))
                ub.Query = "q=" + Uri.EscapeDataString(searchQuery);
            try
            {
                var response = await _http.GetAsync(ub.Uri, ConvertApiConstants.DownloadTimeout).ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
                {
                    var reader = new OpenApiStreamReader();
                    return reader.Read(ms, out var _);
                }
            }
            catch
            {
                return null;
            }
        }

        private static List<ConverterDto> ParseConverters(OpenApiDocument doc)
        {
            var list = new List<ConverterDto>();

            foreach (var pathKvp in doc.Paths)
            {
                var path = pathKvp.Key; // e.g. /convert/pdf/to/docx
                if (!path.StartsWith("/convert/", StringComparison.OrdinalIgnoreCase))
                    continue;

                var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                // expected: ["convert", "{src}", "to", "{dst}"]
                if (parts.Length < 4 || !string.Equals(parts[2], "to", StringComparison.OrdinalIgnoreCase))
                    continue;
                var src = TrimDot(parts[1]);
                var dst = TrimDot(parts[3]);

                var item = pathKvp.Value;
                if (item?.Operations == null) continue;
                if (!item.Operations.TryGetValue(OperationType.Post, out var op))
                    continue;

                var dto = new ConverterDto
                {
                    SourceFormat = src,
                    DestinationFormat = dst,
                    Summary = item.Summary ?? op.Summary,
                    Description = item.Description ?? op.Description,
                    Overview = GetExtensionString(op.Extensions, "x-ca-overview") ?? GetExtensionString(item.Extensions, "x-ca-overview"),
                    MetaTitle = GetExtensionString(op.Extensions, "x-ca-meta-title") ?? GetExtensionString(item.Extensions, "x-ca-meta-title"),
                    MetaDescription = GetExtensionString(op.Extensions, "x-ca-meta-description") ?? GetExtensionString(item.Extensions, "x-ca-meta-description"),
                    Tags = CollectTags(doc, op, item),
                    SourceExtensions = ParseExtensionsFrom("x-ca-source-formats", op.Extensions, item.Extensions, defaultTo: new[] { src }),
                    DestinationExtensions = ParseExtensionsFrom("x-ca-destination-formats", op.Extensions, item.Extensions, defaultTo: new[] { dst }),
                    ConverterParameterGroups = ParseParameterGroups(op)
                };

                list.Add(dto);
            }

            return list;
        }

        private static List<string> CollectTags(OpenApiDocument doc, OpenApiOperation op, OpenApiPathItem item)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // From custom extension array
            foreach (var extDict in new[] { op?.Extensions, item?.Extensions })
            {
                var ext = GetExtensionArray(extDict, "x-ca-tags");
                if (ext != null)
                {
                    foreach (var v in ext)
                    {
                        if (!string.IsNullOrWhiteSpace(v)) set.Add(v);
                    }
                }
            }
            return set.ToList();
        }

        private static IEnumerable<ConverterParameterGroupDto> ParseParameterGroups(OpenApiOperation op)
        {
            // For simplicity, we treat operation request body schema properties as a single group "General"
            var groups = new List<ConverterParameterGroupDto>();
            var parameters = new List<ConverterParameterDto>();

            var content = op?.RequestBody?.Content;
            if (content != null)
            {
                foreach (var kv in content)
                {
                    var schema = kv.Value?.Schema;
                    if (schema == null) continue;

                    var requiredList = schema.Required != null ? schema.Required.ToList() : new List<string>();
                    var required = new HashSet<string>(requiredList, StringComparer.OrdinalIgnoreCase);
                    if (schema.Properties != null)
                    {
                        foreach (var p in schema.Properties)
                        {
                            var name = p.Key;
                            var s = p.Value;

                            var cp = new ConverterParameterDto
                            {
                                Name = name,
                                Label = GetExtensionString(s.Extensions, "x-ca-label") ?? name,
                                Description = s.Description,
                                GroupName = GetExtensionString(s.Extensions, "x-ca-group"),
                                Type = s.Type,
                                XType = GetExtensionString(s.Extensions, "x-ca-type"),
                                Representation = GetExtensionString(s.Extensions, "x-ca-representation"),
                                Default = s.Default is IOpenApiPrimitive prim ? GetPrimitiveValue(prim) : null,
                                Values = ToValuesDictionary(s.Enum, s.Extensions),
                                Range = GetRange(s),
                                Required = required.Contains(name),
                                Featured = GetExtensionBool(s.Extensions, "x-ca-featured") ?? false,
                                Array = string.Equals(s.Type, "array", StringComparison.OrdinalIgnoreCase),
                                AllowedExtensions = GetAllowedExtensions(s)
                            };

                            parameters.Add(cp);
                        }
                    }
                }
            }

            // Group by GroupName to ConverterParameterGroupDto
            foreach (var grp in parameters.GroupBy(p => p.GroupName ?? "General"))
            {
                groups.Add(new ConverterParameterGroupDto
                {
                    Name = grp.Key,
                    ConverterParameters = grp.ToList()
                });
            }
            return groups;
        }

        private static ConverterParameterRangeDto GetRange(OpenApiSchema s)
        {
            string from = null;
            string to = null;
            if (s.Minimum.HasValue) from = s.Minimum.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (s.Maximum.HasValue) to = s.Maximum.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (from == null && to == null) return null;
            return new ConverterParameterRangeDto { From = from, To = to };
        }

        private static string[] GetAllowedExtensions(OpenApiSchema s)
        {
            // Use x-ca-allowed-extensions or x-ca-source-formats on the property
            var formats = GetExtensionString(s.Extensions, "x-ca-allowed-extensions")
                          ?? GetExtensionString(s.Extensions, "x-ca-source-formats");
            if (string.IsNullOrWhiteSpace(formats)) return null;
            return SplitExtensions(formats).ToArray();
        }

        private static Dictionary<string, string> ToValuesDictionary(IList<IOpenApiAny> @enum, IDictionary<string, IOpenApiExtension> extensions)
        {
            // Try x-ca-values first for human-readable display values
            if (extensions != null && extensions.TryGetValue("x-ca-values", out var ext) && ext is OpenApiObject obj && obj.Count > 0)
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in obj)
                {
                    if (entry.Value is OpenApiString valStr)
                        dict[entry.Key] = valStr.Value;
                }
                if (dict.Count > 0) return dict;
            }

            // Fallback to enum key=key
            return ToEnumDictionary(@enum);
        }

        private static Dictionary<string, string> ToEnumDictionary(IList<IOpenApiAny> @enum)
        {
            if (@enum == null || @enum.Count == 0) return null;
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in @enum)
            {
                if (e is IOpenApiPrimitive p)
                {
                    var v = GetPrimitiveValue(p)?.ToString();
                    if (!string.IsNullOrWhiteSpace(v) && !dict.ContainsKey(v))
                        dict[v] = v;
                }
            }
            return dict.Count > 0 ? dict : null;
        }

        private static object GetPrimitiveValue(IOpenApiPrimitive prim)
        {
            switch (prim)
            {
                case OpenApiString s: return s.Value;
                case OpenApiBoolean b: return b.Value;
                case OpenApiInteger i: return i.Value;
                case OpenApiLong l: return l.Value;
                case OpenApiFloat f: return f.Value;
                case OpenApiDouble d: return d.Value;
                default: return prim?.ToString();
            }
        }

        private static List<TagDto> ParseTags(OpenApiDocument doc)
        {
            var list = new List<TagDto>();
            if (doc.Info.Extensions.TryGetValue("x-ca-converter-tags", out var ext))
            {
                IEnumerable<IOpenApiAny> items = null;
                if (ext is OpenApiArray arr)
                    items = arr;
                else if (ext is OpenApiObject obj)
                    items = obj.Values;

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item is OpenApiObject obj)
                        {
                            var tag = new TagDto
                            {
                                Name = GetStringProperty(obj, "name"),
                                Summary = GetStringProperty(obj, "summary"),
                                Description = GetStringProperty(obj, "description"),
                                PageTitle = GetStringProperty(obj, "pageTitle"),
                                FriendlyName = GetStringProperty(obj, "friendlyName"),
                                MetaTitle = GetStringProperty(obj, "metaTitle"),
                                MetaDescription = GetStringProperty(obj, "metaDescription"),
                                Category = GetStringProperty(obj, "category")
                            };
                            list.Add(tag);
                        }
                    }
                }
            }
            return list;
        }

        private static string GetStringProperty(OpenApiObject obj, string key)
        {
            if (obj.TryGetValue(key, out var val) && val is OpenApiString s)
                return s.Value;
            return null;
        }

        private static List<string> ParseExtensionsFrom(string key, IDictionary<string, IOpenApiExtension> opExt, IDictionary<string, IOpenApiExtension> pathExt, IEnumerable<string> defaultTo = null)
        {
            var str = GetExtensionString(opExt, key) ?? GetExtensionString(pathExt, key);
            if (!string.IsNullOrWhiteSpace(str))
            {
                return SplitExtensions(str).ToList();
            }
            return defaultTo?.ToList() ?? new List<string>();
        }

        private static IEnumerable<string> SplitExtensions(string csv)
        {
            return (csv ?? string.Empty)
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static string GetExtensionString(IDictionary<string, IOpenApiExtension> extensions, string key)
        {
            if (extensions == null) return null;
            if (!extensions.TryGetValue(key, out var ext) || ext == null) return null;
            if (ext is OpenApiString s) return s.Value;
            if (ext is OpenApiArray arr)
            {
                try
                {
                    var joined = string.Join(",", arr.Select(a => (a as OpenApiString)?.Value).Where(v => !string.IsNullOrWhiteSpace(v)));
                    return joined;
                }
                catch { return null; }
            }
            return null;
        }

        private static IEnumerable<string> GetExtensionArray(IDictionary<string, IOpenApiExtension> extensions, string key)
        {
            if (extensions == null) return null;
            if (!extensions.TryGetValue(key, out var ext) || ext == null) return null;
            if (ext is OpenApiArray arr)
            {
                return arr.Select(a => (a as OpenApiString)?.Value)
                          .Where(v => !string.IsNullOrWhiteSpace(v))
                          .ToArray();
            }
            if (ext is OpenApiString s)
            {
                return s.Value?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(v => v.Trim())
                         .Where(v => !string.IsNullOrWhiteSpace(v))
                         .ToArray();
            }
            return null;
        }

        private static bool? GetExtensionBool(IDictionary<string, IOpenApiExtension> extensions, string key)
        {
            if (extensions == null) return null;
            if (!extensions.TryGetValue(key, out var ext) || ext == null) return null;
            if (ext is OpenApiBoolean b) return b.Value;
            if (ext is OpenApiString s && bool.TryParse(s.Value, out var v)) return v;
            return null;
        }

        private static bool ContainsAllTags(List<string> converterTags, List<string> wanted)
        {
            if (wanted == null || wanted.Count == 0) return true;
            if (converterTags == null || converterTags.Count == 0) return false;
            var set = new HashSet<string>(converterTags, StringComparer.OrdinalIgnoreCase);
            foreach (var t in wanted)
            {
                if (!set.Contains(t)) return false;
            }
            return true;
        }

        private static string TrimDot(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            return s.Trim().TrimStart('.');
        }
    }
}
