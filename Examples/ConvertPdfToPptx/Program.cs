using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace ConvertPdfToPptx
{
    class Program
    {
        /// <summary>
        /// Example of converting a PDF file to MS Office PowerPoint PPTX slideshow using .NET C#
        /// https://www.convertapi.com/pdf-to-pptx
        /// </summary>
        static async Task Main(string[] args)
        {            
            try
            {
                //Get your api token at https://www.convertapi.com/a
                var convertApi = new ConvertApi("api_token");

                const string sourceFile = @"..\..\..\..\TestFiles\test.pdf";

                var destinationFileName = Path.Combine(Path.GetTempPath(), $"result-{Guid.NewGuid()}.pptx");

                var convertTask = await convertApi.ConvertAsync("pdf", "pptx", 
                    new ConvertApiFileParam(sourceFile));

                var saveFiles = await convertTask.Files.First().SaveFileAsync(destinationFileName);

                Console.WriteLine("The converted PPTX saved to " + saveFiles);
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