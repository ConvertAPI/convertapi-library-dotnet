using System;
using System.IO;
using System.Linq;
using ConvertApi;

namespace CreatePdfThumbnail
{
    class Program
    {
        /// <summary>
        /// Example of extracting first page from PDF and then chaining conversion PDF page to JPG.
        /// https://www.convertapi.com/pdf-to-extract
        /// https://www.convertapi.com/pdf-to-jpg
        /// </summary>
        static void Main(string[] args)
        {
            //Get your secret at https://www.convertapi.com/a
            var convertApiClient = new ConvertApiClient("your api secret");
            var pdfFile = @"..\..\..\TestFiles\test.pdf";

            var extractFirstPage = convertApiClient.ConvertAsync("pdf", "extract", new[]
            {
                new ConvertApiParam("File", File.OpenRead(pdfFile)),
                new ConvertApiParam("PageRange","1")
            });

            var thumbnail = convertApiClient.ConvertAsync("pdf", "jpg", new[]
            {
                new ConvertApiParam("File", extractFirstPage.Result),
                new ConvertApiParam("ScaleImage","true"),
                new ConvertApiParam("ScaleProportions","true"),
                new ConvertApiParam("ImageHeight","300"),
                new ConvertApiParam("ImageWidth","300")
            });

            var saveFiles = thumbnail.Result.SaveFiles(Path.GetTempPath());
            Console.WriteLine("The thumbnail saved to "+ saveFiles.First());
            Console.ReadLine();
        }
    }
}
