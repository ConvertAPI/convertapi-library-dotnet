using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace MergeWordDocsToPdf
{
    class Program
    {
        /// <summary>
        /// The example shows how to merge Word and Powerpoint documents to one PDF by converting them to PDFs and then merging.
        /// https://www.convertapi.com/docx-to-pdf
        /// https://www.convertapi.com/pdf-to-merge
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("The example shows how to merge Word and Powerpoint documents to one PDF by converting them to PDFs and then merging. Please wait...");
                var sw = new Stopwatch();
                sw.Start();
                //Get your api token at https://www.convertapi.com/a
                var convertApi = new ConvertApi("api_token");

                var destinationFileName = Path.Combine(Path.GetTempPath(), $"test-merged-{Guid.NewGuid()}.pdf");

                var convertApiFileParam = new ConvertApiFileParam(@"..\..\..\..\TestFiles\test.docx");

                var doc1 = convertApi.ConvertAsync("docx", "pdf", convertApiFileParam);
                Console.WriteLine("Word sent for conversion...");
                var doc2 = convertApi.ConvertAsync("pptx", "pdf", new ConvertApiFileParam(@"..\..\..\..\TestFiles\test.pptx"));
                Console.WriteLine("PowerPoint sent for conversion...");

                var mergeTask = await convertApi.ConvertAsync("pdf", "merge",
                    new ConvertApiFileParam(await doc1),
                    new ConvertApiFileParam(await doc2));

                var saveFiles = await mergeTask.Files.First().SaveFileAsync(destinationFileName);

                Console.WriteLine("The PDF saved to " + saveFiles);
                sw.Stop();
                Console.WriteLine("Elapsed " + sw.Elapsed);
            }
            //Catch exceptions from asynchronous methods
            catch (ConvertApiException e)
            {
                Console.WriteLine("Status Code: " + e.StatusCode);
                Console.WriteLine("Response: " + e.Response);
            }

            Console.ReadLine();
        }
    }
}
