using ConvertApiDotNet;
using System;
using System.IO;
using ConvertApiDotNet.Exceptions;

namespace Simple
{
    class Program
    {
        static void Main(string[] args)
        {
            //Get your secret at https://www.convertapi.com/a            
            var convertApi = new ConvertApi("your api secret");

            try
            {
                Console.WriteLine("Converting Powerpoint to PDF...");
                var pdfFile = convertApi.ConvertFile(@"..\..\..\TestFiles\test.pptx", Path.Combine(Path.GetTempPath(), "presentation.pdf"));
                Console.WriteLine("PDF created at " + pdfFile.FullName);

                Console.WriteLine("Extracting PDF pages and saving them to PNG...");
                var pngFiles = convertApi.ConvertFile(@"..\..\..\TestFiles\test.pdf", "png", Path.GetTempPath());
                foreach (var fileInfo in pngFiles)
                {
                    Console.WriteLine("PDF page as image " + fileInfo.FullName);
                }

                Console.WriteLine("Google web site to PDF");
                var googlePdf = convertApi.ConvertUrl("https://www.google.com", Path.Combine(Path.GetTempPath(), "google.pdf"));
                Console.WriteLine("PDF created at " + googlePdf.FullName);

                Console.WriteLine("Converting Word to PDF...");
                pdfFile = convertApi.ConvertRemoteFile("https://cdn.convertapi.com/cara/testfiles/document.docx", Path.Combine(Path.GetTempPath(), "document.pdf"));
                Console.WriteLine("PDF created at " + pdfFile.FullName);

            }
            catch (ConvertApiException e)
            {
                Console.WriteLine("Status Code: " + e.StatusCode);
                Console.WriteLine("Response: " + e.Response);
            }

            Console.ReadLine();
        }
    }
}
