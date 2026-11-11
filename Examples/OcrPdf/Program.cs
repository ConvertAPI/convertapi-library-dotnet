// Makes a scanned PDF searchable: OCR adds an invisible text layer, so the text can be found,
// selected and copied. Pages that already have text are skipped.
// https://www.convertapi.com/pdf-to-ocr
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

try
{
    // Get your API token at https://www.convertapi.com/a/authentication
    var convertApi = new ConvertApi(Environment.GetEnvironmentVariable("CONVERTAPI_TOKEN"));

    // Sample scan from Examples/TestFiles, copied next to the program when it builds
    var scannedPdf = Path.Combine(AppContext.BaseDirectory, "TestFiles", "scanned.pdf");

    Console.WriteLine("Running OCR on the scanned PDF...");
    var result = await convertApi.ConvertAsync("pdf", "ocr",
        new ConvertApiFileParam(scannedPdf),
        // The language of the text, such as "de" or "es". If text is missed, for example in tables or
        // low-quality scans, also pass new ConvertApiParam("OcrQuality", "enhanced"): slower, but it reads more.
        new ConvertApiParam("OcrLanguage", "en"));

    var searchable = await result.Files[0].SaveFileAsync(Path.Combine(Path.GetTempPath(), "searchable.pdf"));
    Console.WriteLine("The searchable PDF saved to " + searchable.FullName);
}
catch (ConvertApiException e)
{
    Console.WriteLine("Status Code: " + e.StatusCode);
    Console.WriteLine("Response: " + e.Response);
}

Console.ReadLine();
