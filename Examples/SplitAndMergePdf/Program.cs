using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        static void Main(string[] args)
        {
            try
            {
                //Get your secret at https://www.convertapi.com/a
                var convertApi = new ConvertApi("your secret");
                const string sourceFile = @"..\..\..\TestFiles\test.pdf";

                var destinationFileName = Path.Combine(Path.GetTempPath(), "test-merged.pdf");

                var splitTask = convertApi.ConvertAsync("pdf", "split", new[]
                {
                new ConvertApiParam("File", File.OpenRead(sourceFile))
                });

                var processedFiles = splitTask.Result;

                var mergeTask = convertApi.ConvertAsync("pdf", "merge", new[]
                {
                new ConvertApiParam("Files[0]", processedFiles.Files.First().Url.ToString()),
                new ConvertApiParam("Files[1]", processedFiles.Files.Last().Url.ToString())
            });

                var saveFiles = mergeTask.Result.SaveFile(destinationFileName);

                Console.WriteLine("The PDF saved to " + saveFiles);
            }
            //Catch exceptions from asynchronous methods
            catch (AggregateException e)
            {
                Console.WriteLine($"{e.GetType()}: {e.Message}");
                Console.WriteLine(e.InnerException?.Message);
                var httpStatusCode = (e.InnerException as ConvertApiException)?.StatusCode;
                Console.WriteLine("Status Code: " + httpStatusCode);
                Console.WriteLine("Response: " + (e.InnerException as ConvertApiException)?.Response);
            }

            Console.ReadLine();
        }
    }
}
