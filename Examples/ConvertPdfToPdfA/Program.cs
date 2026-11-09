using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

namespace ConvertPdfToPdfA
{
    class Program
    {
        /// <summary>
        /// Example of converting a PDF file to PDF/A with embedded invoice XML using .NET C#
        /// https://www.convertapi.com/pdf-to-pdfa/csharp
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                // Get your API Token at https://www.convertapi.com/a/authentication
                var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

                // Sample files from Examples/TestFiles, copied next to the program when it builds
                var sourceFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "invoice.pdf");
                var invoiceXml = Path.Combine(AppContext.BaseDirectory, "TestFiles", "invoice.xml");

                var destinationFileName = Path.Combine(Path.GetTempPath(), $"result-pdfa-{Guid.NewGuid()}.pdf");

                // Convert PDF to PDF/A
                var convertTask = await convertApi.ConvertAsync("pdf", "pdfa",
                    new ConvertApiFileParam(sourceFile),
                    new ConvertApiParam("InvoiceFormat", "zugferd1"),
                    new ConvertApiFileParam("InvoiceFile", invoiceXml));

                var savedFile = await convertTask.Files.First().SaveFileAsync(destinationFileName);

                Console.WriteLine("The converted PDF/A saved to " + savedFile);
            }
            // Catch exceptions from asynchronous methods
            catch (ConvertApiException e)
            {
                Console.WriteLine("Status Code: " + e.StatusCode);
                Console.WriteLine("Response: " + e.Response);
            }
            Console.ReadLine();
        }
    }
}
