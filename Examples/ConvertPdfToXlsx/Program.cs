using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace ConvertPdfToXlsx
{
    class Program
    {
        /// <summary>
        /// Example of converting a PDF file to MS Office Excel XLSX spreadsheets using .NET C#
        /// https://www.convertapi.com/pdf-to-xlsx
        /// </summary>
        static async Task Main(string[] args)
        {            
            try
            {
                //Get your secret at https://www.convertapi.com/a
                var convertApi = new ConvertApi("your api secret");

                const string sourceFile = @"..\..\..\..\TestFiles\test.pdf";

                var destinationFileName = Path.Combine(Path.GetTempPath(), $"result-{Guid.NewGuid()}.xlsx");

                var convertTask = await convertApi.ConvertAsync("pdf", "xlsx", 
                    new ConvertApiFileParam(sourceFile));

                var saveFiles = await convertTask.Files.First().SaveFileAsync(destinationFileName);

                Console.WriteLine("The converted XLSX saved to " + saveFiles);
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