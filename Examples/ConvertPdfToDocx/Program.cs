using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace ConvertPdfToDocx
{
    class Program
    {
        /// <summary>
        /// Example of converting a PDF file to an editable MS Office Word DOCX document using .NET C#
        /// https://www.convertapi.com/pdf-to-docx
        /// </summary>
        static async Task Main(string[] args)
        {            
            try
            {
                //Get your secret at https://www.convertapi.com/a
                var convertApi = new ConvertApi("your api secret");

                const string sourceFile = @"..\..\..\..\TestFiles\test.pdf";

                var destinationFileName = Path.Combine(Path.GetTempPath(), $"result-{Guid.NewGuid()}.docx");

                var convertTask = await convertApi.ConvertAsync("pdf", "docx", 
                    new ConvertApiFileParam(sourceFile));

                var saveFiles = await convertTask.Files.First().SaveFileAsync(destinationFileName);

                Console.WriteLine("The converted DOCX saved to " + saveFiles);
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