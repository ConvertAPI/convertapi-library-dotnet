using System;
using System.IO;
using ConvertApiDotNet;

namespace ConvertRemoteFile
{
    class Program
    {
        /// <summary>
        /// The example converting remotely stored file
        /// </summary>
        static void Main(string[] args)
        {
            //Get your secret at https://www.convertapi.com/a
            var convertApi = new ConvertApi("your api secret");

            var sourceFile = new Uri("https://cdn.convertapi.com/cara/testfiles/presentation.pptx");

            Console.WriteLine($"Converting online PowerPoint file {sourceFile} to PDF...");

            var convertToPdf = convertApi.ConvertAsync("pptx", "pdf", new ConvertApiFileParam(sourceFile));

            var outputFileName = convertToPdf.Result.Files[0];
            var fileInfo = outputFileName.AsFileAsync(Path.Combine(Path.GetTempPath(), outputFileName.FileName)).Result;

            Console.WriteLine("The PDF saved to " + fileInfo);
            Console.ReadLine();
        }
    }
}
