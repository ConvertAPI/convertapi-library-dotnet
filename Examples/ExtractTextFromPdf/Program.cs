// Extracts the text of a PDF into a string. Scanned pages are read with OCR when needed.
// https://www.convertapi.com/pdf-to-txt
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

try
{
    // Get your API token at https://www.convertapi.com/a/authentication
    var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

    // Sample file from Examples/TestFiles, copied next to the program when it builds
    var pdfFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "test.pdf");

    Console.WriteLine("Extracting the text of the PDF...");
    var result = await convertApi.ConvertAsync("pdf", "txt",
        new ConvertApiFileParam(pdfFile));

    // Read the text file from the server as a stream, without saving it to disk
    using var reader = new StreamReader(await result.Files[0].FileStreamAsync());
    var text = await reader.ReadToEndAsync();

    Console.WriteLine($"The PDF holds {text.Length} characters of text:");
    Console.WriteLine(text);
}
catch (ConvertApiException e)
{
    Console.WriteLine("Status Code: " + e.StatusCode);
    Console.WriteLine("Response: " + e.Response);
}

Console.ReadLine();
