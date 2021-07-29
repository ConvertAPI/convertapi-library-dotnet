using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet;

namespace AlternativeConverter
{
    class Program
    {
        /// <summary>
        /// The example shows how to use alternative converters to convert Word document to PDF.
        /// Build in MS Word converter https://www.convertapi.com/docx-to-pdf
        /// Custom MS Word printer converter https://www.convertapi.com/docx-to-pdf/printer
        /// OpenOffice converter https://www.convertapi.com/docx-to-pdf/openoffice
        /// </summary>
        static async Task Main(string[] args)
        {
            var convertApi = new ConvertApi("your api secret");

            var pdf1 = Path.Combine(Path.GetTempPath(), $"test-Office-{Guid.NewGuid()}.pdf");
            var doc1 = await convertApi.ConvertAsync("docx", "pdf",
                new ConvertApiFileParam(@"..\..\..\..\TestFiles\test.docx"));
            await doc1.Files.First().SaveFileAsync(pdf1);
            Console.WriteLine($"The file converted using Office {pdf1}");

            var pdf2 = Path.Combine(Path.GetTempPath(), $"test-Office-Printer-{Guid.NewGuid()}.pdf");
            var doc2 = await convertApi.ConvertAsync("docx", "pdf",
                new ConvertApiFileParam(@"..\..\..\..\TestFiles\test.docx"),
                new ConvertApiFileParam("Converter", "Printer"));
            await doc2.Files.First().SaveFileAsync(pdf2);
            Console.WriteLine($"The file converted using Office {pdf2}");
            
            var pdf3 = Path.Combine(Path.GetTempPath(), $"test-Office-OpenOffice-{Guid.NewGuid()}.pdf");
            var doc3 = await convertApi.ConvertAsync("docx", "pdf",
                new ConvertApiFileParam(@"..\..\..\..\TestFiles\test.docx"),
                new ConvertApiFileParam("Converter", "OpenOffice"));
            await doc3.Files.First().SaveFileAsync(pdf3);
            Console.WriteLine($"The file converted using Office {pdf3}");
        }
    }
}