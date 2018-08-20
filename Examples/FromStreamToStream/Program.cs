using System;
using System.IO;
using System.Text;
using ConvertApiDotNet;

namespace FromStreamToStream
{
    class Program
    {
        /// <summary>
        /// Example of converting Word document from stream and getting back PDF stream
        /// https://www.convertapi.com/docx-to-pdf        
        /// </summary>
        static void Main(string[] args)
        {
            //Get your secret at https://www.convertapi.com/a
            var convertApi = new ConvertApi("your api secret");
            const string sourceFile = @"..\..\..\TestFiles\test.docx";

            var stream = new MemoryStream(File.ReadAllBytes(sourceFile));

            var convertToPdf = convertApi.ConvertAsync("docx", "pdf", 
                new ConvertApiFileParam(stream, "test.docx")
                );

            var outputStream = convertToPdf.Result.FileStream();
            
            Console.Write(new StreamReader(outputStream).ReadToEnd());
            Console.WriteLine("End of file stream.");
            Console.ReadLine();
        }
    }
}
