using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet;

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
            //Get your secret at https://www.convertapi.com/a
            var convertApi = new ConvertApi("your api secret");
            var pdfFile = @"..\..\..\TestFiles\test.pdf";

            var extractFirstPage = await convertApi.ConvertAsync("pdf", "extract",
                new ConvertApiFileParam(pdfFile),
                new ConvertApiParam("PageRange", "1")
            );

            var thumbnail = await convertApi.ConvertAsync("pdf", "jpg",
                new ConvertApiFileParam(extractFirstPage),
                new ConvertApiParam("ScaleImage", "true"),
                new ConvertApiParam("ScaleProportions", "true"),
                new ConvertApiParam("ImageHeight", "300"),
                new ConvertApiParam("ImageWidth", "300")
            );

            var saveFiles = await thumbnail.SaveFilesAsync(Path.GetTempPath());
            Console.WriteLine("The thumbnail saved to " + saveFiles.First());
            var deletedCount = await thumbnail.Files.DeleteFilesAsync();
            Console.ReadLine();
        }
    }
}