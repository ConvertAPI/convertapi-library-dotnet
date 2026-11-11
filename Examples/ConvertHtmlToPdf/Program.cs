// Converts an HTML file, and an HTML string such as an invoice your app builds, to PDF files.
// To convert a web page by its URL, see the ConvertUrlToPdf example.
// https://www.convertapi.com/html-to-pdf
using System.Text;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

try
{
    // Get your API token at https://www.convertapi.com/a/authentication
    var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

    // Sample file from Examples/TestFiles, copied next to the program when it builds. Only the HTML file
    // is uploaded, so link images and stylesheets by absolute URL or put them inline.
    var htmlFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "test.html");

    Console.WriteLine("Converting the HTML file to PDF...");
    var fromFile = await convertApi.ConvertAsync("html", "pdf",
        new ConvertApiFileParam(htmlFile),
        new ConvertApiParam("PageSize", "a4"));

    var filePdf = await fromFile.Files[0].SaveFileAsync(Path.Combine(Path.GetTempPath(), "html-file.pdf"));
    Console.WriteLine("The PDF of the HTML file saved to " + filePdf.FullName);

    // An HTML string is uploaded from memory, under a file name that ends in .html
    var html = "<h1>Invoice 1001</h1><p>Total: 99.00 USD</p>";

    Console.WriteLine("Converting the HTML string to PDF...");
    var fromString = await convertApi.ConvertAsync("html", "pdf",
        new ConvertApiFileParam(new MemoryStream(Encoding.UTF8.GetBytes(html)), "invoice.html"));

    var stringPdf = await fromString.Files[0].SaveFileAsync(Path.Combine(Path.GetTempPath(), "html-string.pdf"));
    Console.WriteLine("The PDF of the HTML string saved to " + stringPdf.FullName);
}
catch (ConvertApiException e)
{
    Console.WriteLine("Status Code: " + e.StatusCode);
    Console.WriteLine("Response: " + e.Response);
}

Console.ReadLine();
