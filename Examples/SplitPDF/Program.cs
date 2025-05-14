using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

var randomFolderName = Path.GetRandomFileName();
var outputDirectory = Path.Combine(Path.GetTempPath(), randomFolderName);
Directory.CreateDirectory(outputDirectory);
var demoPdf = Path.Combine(Environment.CurrentDirectory, "Files", "text-pattern-demo.pdf");


try
{
    var convertApi = new ConvertApi("api_token");
    var convert = await convertApi.ConvertAsync("pdf", "split",
        new ConvertApiFileParam("File", demoPdf),
        new ConvertApiParam("SplitByTextPattern", @"Chapter\s+\d+")
    );
    await convert.SaveFilesAsync(outputDirectory);
}
catch (ConvertApiException e)
{
    Console.WriteLine("Status Code: " + e.StatusCode);
    Console.WriteLine("Response: " + e.Response);
}

var files = Directory.GetFiles(outputDirectory);
Directory.Delete(outputDirectory, true);
foreach (var file in files)
{
    Console.WriteLine(file);
}