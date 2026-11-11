// CI only. This file is never part of an example or of the code in README.md.
//
// The Examples workflow (.github/workflows/examples.yml) compiles it into each example and README program
// it builds, through staging-host.targets. The module initializer runs before Main, so ConvertApi.ApiBaseUri
// already points at the staging API when the program creates its client, and the code people copy
// stays exactly as it is in the repository.
namespace ConvertApiCi
{
    internal static class StagingHost
    {
        [System.Runtime.CompilerServices.ModuleInitializer]
        internal static void UseBaseUriFromEnvironment()
        {
            var baseUri = System.Environment.GetEnvironmentVariable("CONVERTAPI_BASE_URI");
            if (string.IsNullOrEmpty(baseUri))
                return;

            ConvertApiDotNet.ConvertApi.ApiBaseUri = baseUri;

            // The workflow fails a run that does not print this line, so a program built without this
            // file cannot pass while it calls the production API.
            System.Console.Error.WriteLine("[ci] ConvertApi.ApiBaseUri = " + baseUri);

            // A README program does not catch errors, and the crash message leaves out what the API said,
            // such as which parameter it rejected. Print that too.
            System.AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is ConvertApiDotNet.Exceptions.ConvertApiException error)
                    System.Console.Error.WriteLine($"[ci] The API answered {(int)error.StatusCode}: {error.Response}");
            };
        }
    }
}
