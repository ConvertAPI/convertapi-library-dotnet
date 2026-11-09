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
        /// https://www.convertapi.com/html-to-pdf
        /// </summary>
        static async Task Main(string[] args)
        {            
            try
            {
                //Get your API Token at https://www.convertapi.com/a/authentication
                var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

                Console.WriteLine("Converting web page https://cdn.convertapi.com/public/files/demo to PDF...");

                var response = await convertApi.ConvertAsync("html", "pdf",
                    new ConvertApiParam("Url", "https://cdn.convertapi.com/public/files/demo"), 
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
