// Converts an Excel workbook (XLSX or XLS) to PDF.
// https://www.convertapi.com/xlsx-to-pdf
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

try
{
    // Get your API token at https://www.convertapi.com/a/authentication
    var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

    // Sample file from Examples/TestFiles, copied next to the program when it builds
    var workbook = Path.Combine(AppContext.BaseDirectory, "TestFiles", "test.xlsx");

    Console.WriteLine("Converting the Excel workbook to PDF...");
    var result = await convertApi.ConvertAsync("xlsx", "pdf",
        new ConvertApiFileParam(workbook));

    var pdf = await result.Files[0].SaveFileAsync(Path.Combine(Path.GetTempPath(), "excel-to-pdf.pdf"));
    Console.WriteLine("The PDF saved to " + pdf.FullName);
}
catch (ConvertApiException e)
{
    Console.WriteLine("Status Code: " + e.StatusCode);
    Console.WriteLine("Response: " + e.Response);
}

Console.ReadLine();
