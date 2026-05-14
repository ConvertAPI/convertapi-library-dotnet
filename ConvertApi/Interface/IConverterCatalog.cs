using System.Collections.Generic;
using System.Threading.Tasks;
using ConvertApiDotNet.Model;

namespace ConvertApiDotNet.Interface
{
    /// <summary>
    /// Provides access to ConvertAPI converters and tags by parsing the OpenAPI specification.
    /// </summary>
    public interface IConverterCatalog
    {
        /// <summary>
        /// Gets the complete list of available converters.
        /// </summary>
        List<ConverterDto> GetAllConverters();

        /// <summary>
        /// Gets a single converter definition by source and destination formats.
        /// </summary>
        ConverterDto GetConverter(string sourceFormat, string destinationFormat);

        /// <summary>
        /// Gets converters filtered by the specified tags.
        /// If tags is null or empty, returns all converters.
        /// </summary>
        List<ConverterDto> GetConvertersByTags(List<string> tags = null);

        /// <summary>
        /// Searches converters by hitting the server's <c>/info/openapi?q=...</c> endpoint.
        /// Terms are joined with spaces. Results are returned in server-computed relevance
        /// order (highest first). Empty or null input returns an empty list without a
        /// network call.
        /// </summary>
        /// <remarks>
        /// Requires a ConvertAPI server build that supports server-side search filtering.
        /// </remarks>
        List<ConverterDto> SearchConverters(string[] terms);

        /// <summary>
        /// Searches converters by hitting the server's <c>/info/openapi?q=...</c> endpoint.
        /// Results are returned in server-computed relevance order (highest first). Empty
        /// or whitespace input returns an empty list without a network call.
        /// </summary>
        List<ConverterDto> SearchConverters(string query);

        /// <summary>
        /// Async-canonical form of <see cref="SearchConverters(string)"/>. The synchronous
        /// overloads block on this.
        /// </summary>
        Task<List<ConverterDto>> SearchConvertersAsync(string query);

        /// <summary>
        /// Gets the list of available tags.
        /// </summary>
        List<TagDto> GetTags();

        /// <summary>
        /// Reloads converter and tag information from the service and updates the cache.
        /// </summary>
        Task Reload();
    }
}
