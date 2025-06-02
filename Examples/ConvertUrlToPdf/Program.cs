using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace ConvertUrlToPdf
{
    class Program
    {
        /// <summary>
        /// Example of converting Web Page URL to PDF file
        /// https://www.convertapi.com/web-to-pdf
        /// </summary>
        static async Task Main(string[] args)
        {            
            try
            {
                //Get your API Token at https://www.convertapi.com/a/authentication
                var convertApi = new ConvertApi("api_token");

                Console.WriteLine("Converting web page https://en.wikipedia.org/wiki/Data_conversion to PDF...");

                var response = await convertApi.ConvertAsync("web", "pdf", 
                    new ConvertApiParam("Url", "https://en.wikipedia.org/wiki/Data_conversion"), 
                    new ConvertApiParam("FileName", "web-example"));

                var fileSaved = await response.Files.SaveFilesAsync(Path.GetTempPath());

                Console.WriteLine("The web page PDF saved to " + fileSaved.First());
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
