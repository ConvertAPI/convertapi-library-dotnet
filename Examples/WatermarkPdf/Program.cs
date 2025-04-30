/// <summary>
/// The example shows how to watermark PDF with text and put overlay PDF.
/// https://www.convertapi.com/pdf-to-watermark
/// https://www.convertapi.com/pdf-to-watermark-overlay
/// </summary>

using System.Diagnostics;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;

try
{
    Console.WriteLine("The example shows how to watermark PDF with text and put overlay PDF");
    var sw = new Stopwatch();
    sw.Start();
    //Get your api token at https://www.convertapi.com/a
    var convertApi = new ConvertApi("api_token");

    var destinationFileName = Path.Combine(Path.GetTempPath(), $"watermarked-{Guid.NewGuid()}.pdf");

    Console.WriteLine("PDF sent for text watermarking...");

    var pdfWatermark = await convertApi.ConvertAsync("pdf", "watermark",
        new ConvertApiFileParam("File", @"..\..\..\TestFiles\test-text.pdf"),
        new ConvertApiParam("VerticalAlignment", "bottom"),
        new ConvertApiParam("Text", "This is ConvertAPI watermark %DATETIME%"),
        new ConvertApiParam("FontSize", 15));

    Console.WriteLine("PDF sent for overlay watermarking...");

    var pdfWatermarkOverlay = await convertApi.ConvertAsync("pdf", "watermark-overlay",
        new ConvertApiFileParam("File", pdfWatermark),
        new ConvertApiFileParam("OverlayFile", new FileInfo(@"..\..\..\TestFiles\pdf-overlay.pdf")),
        new ConvertApiParam("Style", "stamp"),
        new ConvertApiParam("Opacity", 50));


    var saveFiles = await pdfWatermarkOverlay.Files.First().SaveFileAsync(destinationFileName);

    Console.WriteLine("The PDF saved to " + saveFiles);
    sw.Stop();

    Console.WriteLine("Elapsed " + sw.Elapsed);

    Process.Start(saveFiles.ToString());
}
//Catch exceptions from asynchronous methods
catch (ConvertApiException e)
{
    Console.WriteLine("Status Code: " + e.StatusCode);
    Console.WriteLine("Response: " + e.Response);
}

Console.ReadLine();