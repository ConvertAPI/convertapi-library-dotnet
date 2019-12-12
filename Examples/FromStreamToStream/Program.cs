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
        /// Example of converting HTML document from stream and getting back PDF stream
        /// https://www.convertapi.com/docx-to-pdf        
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
