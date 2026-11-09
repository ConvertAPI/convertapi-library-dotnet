using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace UploadOnceReuseTwice
{
    class Program
    {
        /// <summary>
        /// Demonstrates uploading a DOCX file once and reusing it for two conversions: DOCX->PDF and DOCX->PNG.
        /// Reuse is achieved by explicitly uploading once with GetValueAsync and passing the FileId to ConvertAsync.
        /// https://www.convertapi.com/docx-to-pdf
        /// https://www.convertapi.com/docx-to-png
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                // Get your API Token at https://www.convertapi.com/a/authentication
                var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

                // Sample file from Examples/TestFiles, copied next to the program when it builds
                var sourceFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "test.docx");

                if (!File.Exists(sourceFile))
                {
                    throw new FileNotFoundException(
                        $"Sample file not found at '{sourceFile}'. " +
                        "Ensure 'Examples/TestFiles/test.docx' exists or update the path accordingly.");
                }

                // Upload once and get back the uploaded file info (with FileId)
                var uploaded = await (new ConvertApiFileParam(sourceFile)).GetUploadedFileAsync();

                // 1) Convert DOCX to PDF reusing the same uploaded FileId
                var toPdf = await convertApi.ConvertAsync("docx", "pdf",
                    new ConvertApiParam("File", uploaded.FileId));
                var pdfFile = toPdf.Files.First();
                var pdfSaved = await pdfFile.SaveFileAsync(Path.Combine(Path.GetTempPath(), pdfFile.FileName));
                Console.WriteLine("The PDF saved to " + pdfSaved);

                // 2) Convert DOCX to PNG reusing the same uploaded FileId (no second upload)
                var toPng = await convertApi.ConvertAsync("docx", "png",
                    new ConvertApiParam("File", uploaded.FileId));
                foreach (var processed in toPng.Files)
                {
                    var saved = await processed.SaveFileAsync(Path.Combine(Path.GetTempPath(), processed.FileName));
                    Console.WriteLine("The PNG saved to " + saved);
                }
            }
            catch (ConvertApiException e)
            {
                Console.WriteLine("Status Code: " + e.StatusCode);
                Console.WriteLine("Response: " + e.Response);
            }

            Console.WriteLine("Done. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
