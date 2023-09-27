using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertApiDotNet;

namespace FromStreamToStream
{
    class Program
    {
        /// <summary>
        /// A demo demonstrate the process of converting an HTML document from a stream(string HTML) and obtaining a PDF stream in return.
        /// https://www.convertapi.com/html-to-pdf        
        /// </summary>
        static async Task Main(string[] args)
        {
            //Get your secret at https://www.convertapi.com/a
            var convertApi = new ConvertApi("your api secret");
            const string htmlString = "<!DOCTYPE html><html><body><h1>My First Heading</h1><p>My first paragraph.</p></body></html>";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlString));

            var convertToPdf = await convertApi.ConvertAsync("html", "pdf",
                new ConvertApiFileParam(stream, "test.html")
                );

            var outputStream = await convertToPdf.Files.First().FileStreamAsync();

            Console.Write(new StreamReader(outputStream).ReadToEnd());
            Console.WriteLine("End of file stream.");
            Console.ReadLine();
        }
    }
}
