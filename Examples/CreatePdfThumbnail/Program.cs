using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace CreatePdfThumbnail
{
    class Program
    {
        /// <summary>
        /// Create PDF Thumbnail
        /// Example of extracting first page from PDF and then converting it to JPG using chaining.
        /// https://www.convertapi.com/pdf-to-extract
        /// https://www.convertapi.com/pdf-to-jpg
        /// </summary>
        static async Task Main(string[] args)
        {
            //Get your api token at https://www.convertapi.com/a
            var convertApi = new ConvertApi("api_token");
            var pdfFile = @"..\..\..\TestFiles\test.pdf";           

            try
            {
                var thumbnail = await convertApi.ConvertAsync("pdf", "jpg",
                    new ConvertApiFileParam(pdfFile),
                    new ConvertApiParam("ScaleImage", "true"),
                    new ConvertApiParam("ScaleProportions", "true"),
                    new ConvertApiParam("ImageHeight", "300"),
                    new ConvertApiParam("ImageWidth", "300"),
                    new ConvertApiParam("PageRange", "1")
                );


                var saveFiles = await thumbnail.SaveFilesAsync(Path.GetTempPath());
                Console.WriteLine("The thumbnail saved to " + saveFiles.First());
                await thumbnail.Files.DeleteFilesAsync();
            }
            //Catch exceptions and write details
            catch (ConvertApiException e)
            {
                Console.WriteLine("Status Code: " + e.StatusCode);
                Console.WriteLine("Response: " + e.Response);
            }

            Console.ReadLine();
        }
    }
}