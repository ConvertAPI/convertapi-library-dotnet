using System;
using System.IO;
using System.Linq;
using ConvertApi;
using ConvertApi.Exceptions;

namespace ConvertUrlToPdf
{
    class Program
    {
        /// <summary>
        /// Example of converting Web Page URL to PDF file
        /// https://www.convertapi.com/web-to-pdf
        /// </summary>
        static void Main(string[] args)
        {            
            try
            {
                //Get your secret at https://www.convertapi.com/a
                var convertApiClient = new ConvertApiClient("your api secret", 180);
                var saveFiles = convertApiClient.ConvertAsync("web", "pdf", new[]
                {
                new ConvertApiParam("Url", "https://en.wikipedia.org/wiki/Data_conversion"),
                new ConvertApiParam("FileName", "web-example")
                }).Result.SaveFiles(Path.GetTempPath());
                Console.WriteLine("The web page PDF saved to " + saveFiles.First());
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
