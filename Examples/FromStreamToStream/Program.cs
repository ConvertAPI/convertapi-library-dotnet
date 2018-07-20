using System.IO;
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

            var data = File.ReadAllBytes(sourceFile);
            var stream = new MemoryStream(data);

            var fileParam = new ConvertApiParam("File", stream, "test.docx");

            var convertToPdf = convertApi.ConvertAsync("docx", "pdf", new[]
            {
                fileParam
            });

            var outputStream = convertToPdf.Result.FileStream();
        }
    }
}
