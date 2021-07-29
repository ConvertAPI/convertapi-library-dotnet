using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace TokenAuthentication
{
    class Program
    {
        /// <summary>
        /// Token authentication example
        /// More information https://www.convertapi.com/doc/auth
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                //Get your token and apikey at https://www.convertapi.com/a/auth
                var convertApi = new ConvertApi("token", 0);
                var sourceFile = @"..\..\..\TestFiles\test.docx";

                var result = await convertApi.ConvertAsync("docx", "pdf",
                    new ConvertApiFileParam(sourceFile)
                );

                var saveFiles = await result.SaveFilesAsync(Path.GetTempPath());
                Console.WriteLine("The pdf saved to " + saveFiles.First());
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