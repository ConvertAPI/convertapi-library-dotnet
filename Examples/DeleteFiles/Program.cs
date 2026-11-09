using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace DeleteFiles
{
    class Program
    {
        /// <summary>
        /// Example showing how to delete both result and uploaded input files using DeleteFilesAsync()
        /// https://www.convertapi.com/docx-to-pdf
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                // Get your API Token at https://www.convertapi.com/a/authentication
                var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

                // Sample file from Examples/TestFiles, copied next to the program when it builds
                var sourceFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "test.docx");

                Console.WriteLine("Converting DOCX to PDF...");

                // Convert local file DOCX -> PDF
                var response = await convertApi.ConvertAsync("docx", "pdf",
                    new ConvertApiFileParam(sourceFile),
                    new ConvertApiParam("FileName", "delete-files-demo"));

                // Save the resulting file(s) to a temp folder
                var saved = await response.Files.SaveFilesAsync(Path.GetTempPath());
                Console.WriteLine("Saved to: " + string.Join(", ", saved.Select(f => f.FullName)));

                // Delete both result files and any uploaded input files with no extra parameters
                var deletedCount = await response.DeleteFilesAsync();
                Console.WriteLine($"Deleted {deletedCount} file(s) from server.");
            }
            catch (ConvertApiException e)
            {
                Console.WriteLine("Status Code: " + e.StatusCode);
                Console.WriteLine("Response: " + e.Response);
            }

            Console.ReadLine();
        }
    }
}
