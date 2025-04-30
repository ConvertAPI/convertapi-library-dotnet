using System;
using System.IO;
using System.Threading.Tasks;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace ConvertRemoteFile
{
    class Program
    {
        /// <summary>
        /// The example converting remotely stored file
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                //Get your api token at https://www.convertapi.com/a
                var convertApi = new ConvertApi("api_token");

                var sourceFile = new Uri("https://cdn.convertapi.com/public/files/demo.pptx");

                Console.WriteLine($"Converting online PowerPoint file {sourceFile} to PDF...");

                var convertToPdf = await convertApi.ConvertAsync("pptx", "pdf", new ConvertApiFileParam(sourceFile));

                var outputFileName = convertToPdf.Files[0];
                var fileInfo = await outputFileName.SaveFileAsync(Path.Combine(Path.GetTempPath(), outputFileName.FileName));

                Console.WriteLine("The PDF saved to " + fileInfo);
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