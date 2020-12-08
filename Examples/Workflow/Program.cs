using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace Workflow
{
    class Program
    {
        /// <summary>
        /// Short example of conversions workflow, the PDF pages extracted and saved as separated JPGs and then ZIP'ed
        /// https://www.convertapi.com/doc/chaining
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                //Get your secret at https://www.convertapi.com/a            
                var convertApi = new ConvertApi("your api secret");

                Console.WriteLine("Converting PDF to JPG and compressing result files with ZIP");
                var fileName = Path.Combine(Path.GetTempPath(), "test.pdf");

                var firstTask = await convertApi.ConvertAsync("pdf", "jpg", new ConvertApiFileParam(fileName));
                Console.WriteLine($"Conversions done. Cost: {firstTask.ConversionCost}. Total files created: {firstTask.FileCount()}");

                var secondsTask = await convertApi.ConvertAsync("jpg", "zip", new ConvertApiFileParam(firstTask));
                var saveFiles = await secondsTask.Files.SaveFilesAsync(Path.GetTempPath());

                Console.WriteLine($"Conversions done. Cost: {secondsTask.ConversionCost}. Total files created: {secondsTask.FileCount()}");
                Console.WriteLine($"File saved to {saveFiles.First().FullName}");
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