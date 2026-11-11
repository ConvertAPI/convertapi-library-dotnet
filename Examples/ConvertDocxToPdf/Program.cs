// Converts a Word document (DOCX) to PDF. For a DOC file, keep "docx" and pass the DOC file.
// https://www.convertapi.com/docx-to-pdf
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

try
{
    // Get your API token at https://www.convertapi.com/a/authentication
    var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

    // Sample file from Examples/TestFiles, copied next to the program when it builds
    var wordFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "test.docx");

    Console.WriteLine("Converting the Word document to PDF...");
    var result = await convertApi.ConvertAsync("docx", "pdf",
        new ConvertApiFileParam(wordFile));

    var pdf = await result.Files[0].SaveFileAsync(Path.Combine(Path.GetTempPath(), "word-to-pdf.pdf"));
    Console.WriteLine("The PDF saved to " + pdf.FullName);
}
catch (ConvertApiException e)
{
    Console.WriteLine("Status Code: " + e.StatusCode);
    Console.WriteLine("Response: " + e.Response);
}

Console.ReadLine();
