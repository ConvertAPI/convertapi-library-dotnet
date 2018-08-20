using System;
using System.IO;
using System.Linq;
using ConvertApiDotNet;

namespace CreatePdfThumbnail
{
    class Program
    {
        /// <summary>
        /// Create PDF Thumbnail
        /// Example of extracting first page from PDF and then chaining conversion PDF page to JPG.
        /// https://www.convertapi.com/pdf-to-extract
        /// https://www.convertapi.com/pdf-to-jpg
        /// </summary>
        static void Main(string[] args)
        {
            //Get your secret at https://www.convertapi.com/a
            var convertApi = new ConvertApi("wBcm6PoqjxZ7vtDQ");
            var pdfFile = @"..\..\..\TestFiles\test.pdf";

            var extractFirstPage = convertApi.ConvertAsync("pdf", "extract",
                new ConvertApiFileParam(pdfFile),
                new ConvertApiParam("PageRange", "1")
            );

            var thumbnail = convertApi.ConvertAsync("pdf", "jpg",

                new ConvertApiFileParam(extractFirstPage.Result),
                new ConvertApiParam("ScaleImage", "true"),
                new ConvertApiParam("ScaleProportions", "true"),
                new ConvertApiParam("ImageHeight", "300"),
                new ConvertApiParam("ImageWidth", "300")
            );

            var saveFiles = thumbnail.Result.SaveFiles(Path.GetTempPath());
            Console.WriteLine("The thumbnail saved to " + saveFiles.First());
            Console.ReadLine();
        }
    }
}
