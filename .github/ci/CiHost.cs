// CI only. This file is never part of an example or of the code in README.md.
//
// The Examples workflow (.github/workflows/examples.yml) compiles it into each example and README program it
// builds, through ci-host.targets. The programs call the API exactly as people run them. This file only:
//   - prints the API address the program uses, a line the workflow requires, so it knows this file is in;
//   - records the files the program uploads and the files the API returns, and deletes them from the server
//     when the program exits, because the API would keep them for up to 3 hours;
//   - prints what the API answered when the program crashes on a ConvertApiException, which the crash
//     message leaves out.
// To run the programs against another API address, set CONVERTAPI_BASE_URI. The workflow does not set it.
namespace ConvertApiCi
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class CiHost
    {
        private static int _deleted;

        [System.Runtime.CompilerServices.ModuleInitializer]
        internal static void Start()
        {
            var baseUri = Environment.GetEnvironmentVariable("CONVERTAPI_BASE_URI");
            if (!string.IsNullOrEmpty(baseUri))
                global::ConvertApiDotNet.ConvertApi.ApiBaseUri = baseUri;
            Console.Error.WriteLine("[ci] ConvertApi.ApiBaseUri = " + global::ConvertApiDotNet.ConvertApi.ApiBaseUri);

            // Installs the recording client. Passing the current ApiToken (none yet) leaves the token to the program.
            _ = new global::ConvertApiDotNet.ConvertApi(global::ConvertApiDotNet.ConvertApi.ApiToken, new RecordingClient());

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is global::ConvertApiDotNet.Exceptions.ConvertApiException error)
                    Console.Error.WriteLine($"[ci] The API answered {(int)error.StatusCode}: {error.Response}");
                DeleteFiles();
            };
            AppDomain.CurrentDomain.ProcessExit += (_, _) => DeleteFiles();
        }

        private static void DeleteFiles()
        {
            if (Interlocked.Exchange(ref _deleted, 1) == 1)
                return;
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            int deleted = 0, total = 0;
            foreach (var url in Recorder.FileUrls.Keys)
            {
                total++;
                try
                {
                    // As the SDK's DeleteFilesAsync does. 404: the program already deleted the file itself.
                    var response = http.DeleteAsync(url).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode || (int)response.StatusCode == 404)
                        deleted++;
                    else
                        Console.Error.WriteLine($"[ci] Deleting a file from the server returned {(int)response.StatusCode}");
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("[ci] Deleting a file from the server failed: " + e.Message);
                }
            }
            Console.Error.WriteLine($"[ci] Deleted {deleted} of {total} files from the server");
        }
    }

    // The SDK's default client is an HttpClient with ConvertApiConstants.HttpClientTimeOut, made on first use.
    // This one is the same, with a handler that notes the URL of every file the API returns.
    internal sealed class RecordingClient : global::ConvertApiDotNet.Interface.IConvertApiHttpClient
    {
        private readonly Lazy<HttpClient> _client = new Lazy<HttpClient>(() =>
            new HttpClient(new Recorder(new HttpClientHandler()))
            {
                Timeout = global::ConvertApiDotNet.Constants.ConvertApiConstants.HttpClientTimeOut
            });

        public HttpClient Client => _client.Value;
    }

    internal sealed class Recorder : DelegatingHandler
    {
        internal static readonly ConcurrentDictionary<string, byte> FileUrls = new ConcurrentDictionary<string, byte>();

        public Recorder(HttpMessageHandler inner) : base(inner) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            var path = request.RequestUri?.AbsolutePath ?? "";
            // An upload answers with the uploaded file, a conversion with its result files: both carry a Url.
            if (request.Method == HttpMethod.Post && response.IsSuccessStatusCode &&
                (path.StartsWith("/upload", StringComparison.Ordinal) || path.StartsWith("/convert/", StringComparison.Ordinal)))
            {
                await response.Content.LoadIntoBufferAsync();
                try
                {
                    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                    Collect(json.RootElement);
                }
                catch (JsonException)
                {
                    // Not JSON: nothing to record. The program reads the same buffered content.
                }
            }
            return response;
        }

        private static void Collect(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (property.NameEquals("Url") && property.Value.ValueKind == JsonValueKind.String)
                        FileUrls.TryAdd(property.Value.GetString()!, 0);
                    else
                        Collect(property.Value);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                    Collect(item);
            }
        }
    }
}
