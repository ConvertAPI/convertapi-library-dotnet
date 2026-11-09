/// <summary>
/// The example shows how to handle a ConvertAPI exception and write details about it 
/// </summary>

using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

try
{
    // Get your API Token at https://www.convertapi.com/a/authentication
    var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

    // Sample file from Examples/TestFiles, copied next to the program when it builds
    var sourceFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "test.docx");

    var convert = await convertApi.ConvertAsync("pdf", "split",
        new ConvertApiFileParam(sourceFile));
}
//Catch exceptions and write details
catch (ConvertApiException e)
{
    Console.WriteLine("Status Code: " + e.StatusCode);
    Console.WriteLine("Response: " + e.Response);
}

Console.ReadLine();
