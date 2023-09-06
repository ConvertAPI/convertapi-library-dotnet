/// <summary>
/// The example shows how to handle a ConvertAPI exception and write details about it 
/// </summary>

using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

try
{
    var convertApi = new ConvertApi("your api secret");
    const string sourceFile = @"..\..\..\TestFiles\test.docx";

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
