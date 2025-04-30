// See https://aka.ms/new-console-template for more information

using System.Net;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace HandleRetry;

class Program
{
    /// <summary>
    /// Example of handling 503 (service not available) error using .NET C#
    /// </summary>
    static async Task Main()
    {
        //Get your api token at https://www.convertapi.com/a
        var convertApi = new ConvertApi("api_token");
        const string sourceFile = @"..\..\..\..\TestFiles\test.pdf";

        // Define two retry delays.
        var retryDelays = new[] { TimeSpan.FromSeconds(2.5), TimeSpan.FromSeconds(7) };
        var maxAttempts = retryDelays.Length + 1; // 1 initial try + 2 retries

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var destination = Path.Combine(Path.GetTempPath(), $"result-{Guid.NewGuid()}.pptx");

                // Do the conversion.
                var convertTask = await convertApi.ConvertAsync("pdf","pptx", new ConvertApiFileParam(sourceFile));

                // Save and report success.
                var saved = await convertTask.Files.First().SaveFileAsync(destination);
                Console.WriteLine("Saved to " + saved);
                break;
            }
            catch (ConvertApiException e) when (e.StatusCode == (HttpStatusCode)503 && attempt < maxAttempts)
            {
                // On 503, wait the corresponding delay and loop.
                var delay = retryDelays[attempt - 1];
                Console.WriteLine($"Attempt {attempt} got 503. Waiting {delay.TotalSeconds}s before retry...");
                await Task.Delay(delay);
            }
            catch (ConvertApiException e)
            {
                // Any non-503 or exhausted retries.
                Console.WriteLine("Conversion failed.");
                Console.WriteLine("Status Code: " + e.StatusCode);
                Console.WriteLine("Response: " + e.Response);
                break;
            }
        }
    }
}