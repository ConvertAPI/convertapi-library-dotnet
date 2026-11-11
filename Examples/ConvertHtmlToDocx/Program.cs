// Converts an HTML file to an editable Word document (DOCX).
// https://www.convertapi.com/html-to-docx
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

try
{
    // Get your API token at https://www.convertapi.com/a/authentication
    var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

    // Sample file from Examples/TestFiles, copied next to the program when it builds
    var htmlFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "test.html");

    Console.WriteLine("Converting the HTML file to a Word document...");
    var result = await convertApi.ConvertAsync("html", "docx",
        new ConvertApiFileParam(htmlFile));

    var docx = await result.Files[0].SaveFileAsync(Path.Combine(Path.GetTempPath(), "html-to-word.docx"));
    Console.WriteLine("The Word document saved to " + docx.FullName);
}
catch (ConvertApiException e)
{
    Console.WriteLine("Status Code: " + e.StatusCode);
    Console.WriteLine("Response: " + e.Response);
}

Console.ReadLine();
