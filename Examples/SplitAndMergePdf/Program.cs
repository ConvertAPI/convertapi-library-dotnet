using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace SplitAndMergePdf
{
    class Program
    {
        /// <summary>
        /// Example of extracting first and last pages from PDF and then merging them back to new PDF.
        /// https://www.convertapi.com/pdf-to-split
        /// https://www.convertapi.com/pdf-to-merge
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                //Get your api token at https://www.convertapi.com/a
                var convertApi = new ConvertApi("api_token");
                const string sourceFile = @"..\..\..\TestFiles\test.pdf";

                var destinationFileName = Path.Combine(Path.GetTempPath(), $"test-merged-{Guid.NewGuid()}.pdf");

                var splitTask = await convertApi.ConvertAsync("pdf", "split", 
                    new ConvertApiFileParam(sourceFile));

                var mergeTask = await convertApi.ConvertAsync("pdf", "merge", 
                    new ConvertApiFileParam(splitTask.Files.First()), 
                    new ConvertApiFileParam(splitTask.Files.Last()));

                var saveFiles = await mergeTask.Files.First().SaveFileAsync(destinationFileName);

                Console.WriteLine("The PDF saved to " + saveFiles);
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
