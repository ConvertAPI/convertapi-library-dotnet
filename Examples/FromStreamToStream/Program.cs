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
            const string htmlString = "<!DOCTYPE html><html><body><h1>My First Heading</h1><p>My first paragraph.</p></body></html>";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlString));

            var convertToPdf = convertApi.ConvertAsync("html", "pdf",
                new ConvertApiFileParam(stream, "test.html")
                );

            var outputStream = convertToPdf.Result.FileStream();

            Console.Write(new StreamReader(outputStream).ReadToEnd());
            Console.WriteLine("End of file stream.");
            Console.ReadLine();
        }
    }
}
