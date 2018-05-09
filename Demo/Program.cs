using System;
using System.IO;
using System.Net;
using ConvertApi;
using ConvertApi.Exceptions;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                const string secret = "<Your secret here>";

                var tempDir = Path.Combine(Path.GetTempPath(), "ConvertAPI");
                Directory.CreateDirectory(tempDir);
                var filesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files");
                Console.WriteLine($"Using directory for result files: {tempDir}");
                
                var convertApiClient = new ConvertApiClient(secret, 180);
                ConvertUrlToPdf(convertApiClient, tempDir);

                convertApiClient = new ConvertApiClient(secret);
                ConvertPdfToJpgAndZip(convertApiClient, filesDir, tempDir);

                convertApiClient = new ConvertApiClient(secret);
                ConvertDocxToPdf(convertApiClient, filesDir, tempDir);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException?.Message);
                var httpStatusCode = (e.InnerException as ConvertApiException)?.StatusCode;
                Console.WriteLine("Status Code: " + httpStatusCode);
                Console.WriteLine("Response: " + (e.InnerException as ConvertApiException)?.Response);

                if (httpStatusCode == HttpStatusCode.Unauthorized)
                    Console.WriteLine("Secret is not provided or no additional seconds left in account to proceed conversion. More information https://www.convertapi.com/a");
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Example of converting Word to PDF file synchronously
        /// https://www.convertapi.com/docx-to-pdf
        /// </summary>
        private static void ConvertDocxToPdf(ConvertApiClient convertApiClient, string filesDir, string tempDir)
        {
            Console.WriteLine("Converting Word DOCX to PDF");
            var sourceFile = Path.Combine(filesDir, "test.docx");
            var destinationFile = Path.Combine(tempDir, "test.pdf");
            convertApiClient.Convert(sourceFile, destinationFile);
            Console.WriteLine("Conversions done.");
        }


        /// <summary>
        /// Example of converting Web Page URL to PDF file
        /// https://www.convertapi.com/web-to-pdf
        /// </summary>
        private static void ConvertUrlToPdf(ConvertApiClient convertApiClient, string tempDir)
        {

            Console.WriteLine("Converting Web Page to PDF");
            convertApiClient.ConvertAsync("web", "pdf", new[]
            {
                new ConvertApiParam("Url", "https://en.wikipedia.org/wiki/Data_conversion"),
                new ConvertApiParam("FileName", "web-example")
            }).Result.SaveFiles(tempDir);
        }

        /// <summary>
        /// Short example of conversions chaining, the PDF pages extracted and saved as separated JPGs and then ZIP'ed
        /// https://www.convertapi.com/doc/chaining
        /// </summary>
        private static void ConvertPdfToJpgAndZip(ConvertApiClient convertApiClient, string filesDir, string tempDir)
        {
            Console.WriteLine("Converting PDF to JPG and compressing result files with ZIP");
            var fileName = Path.Combine(filesDir, "test.pdf");

            var firstTask = convertApiClient.ConvertAsync("pdf", "jpg", new[] { new ConvertApiParam("File", File.OpenRead(fileName)) });
            Console.WriteLine($"Conversions done. Cost: {firstTask.Result.ConversionCost}. Total files created: {firstTask.Result.FileCount()}");

            var secondsTask = convertApiClient.ConvertAsync("jpg", "zip", new[] { new ConvertApiParam("Files", firstTask.Result) });
            secondsTask.Result.SaveFiles(tempDir);

            Console.WriteLine($"Conversions done. Cost: {secondsTask.Result.ConversionCost}. Total files created: {secondsTask.Result.FileCount()}");
        }
    }
}
