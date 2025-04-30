using System;
using System.IO;
using System.Threading.Tasks;
using ConvertApiDotNet;

namespace ConvertWordToPdfAndPng
{
    class Program
    {
        /// <summary>
        /// Example of saving the same Word docx to PDF and to PNG without uploading the same Word file two times.
        /// https://www.convertapi.com/docx-to-pdf
        /// https://www.convertapi.com/docx-to-png
        /// </summary>
        static async Task Main(string[] args)
        {
            //Get your api token at https://www.convertapi.com/a
            var convertApi = new ConvertApi("api_token");
            const string sourceFile = @"..\..\..\TestFiles\test.docx";

            var fileParam = new ConvertApiFileParam(sourceFile);

            var convertToPdf = await convertApi.ConvertAsync("docx", "pdf", fileParam);

            var outputFileName = convertToPdf.Files[0];
            var fileInfo = await outputFileName.SaveFileAsync(Path.Combine(Path.GetTempPath(), outputFileName.FileName));

            Console.WriteLine("The PDF saved to " + fileInfo);            

            var convertToPng = await convertApi.ConvertAsync("docx", "png", fileParam);

            foreach (var processedFile in convertToPng.Files)
            {
                fileInfo = await processedFile.SaveFileAsync(Path.Combine(Path.GetTempPath(), processedFile.FileName));
                Console.WriteLine("The PNG saved to " + fileInfo);
            }

            Console.ReadLine();
        }
    }
}
