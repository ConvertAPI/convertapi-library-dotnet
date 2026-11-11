// Compresses a PDF and prints its size before and after.
// The Preset parameter accepts lossless, text, archive, web, ebook and printer.
// https://www.convertapi.com/pdf-to-compress
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

try
{
    // Get your API token at https://www.convertapi.com/a/authentication
    var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

    // Sample file from Examples/TestFiles, copied next to the program when it builds
    var pdfFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, "TestFiles", "test.pdf"));

    Console.WriteLine("Compressing the PDF...");
    var result = await convertApi.ConvertAsync("pdf", "compress",
        new ConvertApiFileParam(pdfFile),
        new ConvertApiParam("Preset", "web"));

    var compressed = await result.Files[0].SaveFileAsync(Path.Combine(Path.GetTempPath(), "compressed.pdf"));

    Console.WriteLine($"Before: {pdfFile.Length:N0} bytes");
    Console.WriteLine($"After:  {compressed.Length:N0} bytes, {100.0 * compressed.Length / pdfFile.Length:F0}% of the original");
    Console.WriteLine("The compressed PDF saved to " + compressed.FullName);
}
catch (ConvertApiException e)
{
    Console.WriteLine("Status Code: " + e.StatusCode);
    Console.WriteLine("Response: " + e.Response);
}

Console.ReadLine();
