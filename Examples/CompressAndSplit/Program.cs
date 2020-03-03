using ConvertApiDotNet;
using System;
using System.IO;

namespace CompressAndSplit
{
    /// <summary>
    /// Compressing PDF and if the result is larger than 500Kb splitting result pages to the separate files.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                var convertApi = new ConvertApi(args[0]);

                convertApi.ConvertAsync("pdf", "compress", new[] { new ConvertApiFileParam(args[1])})
                    .ContinueWith(t =>
                            t.Result.Files[0].FileSize > 500 * 1024
                            ? convertApi.ConvertAsync("pdf", "split", new[] { new ConvertApiParam("file", t.Result)}).Result
                            : t.Result
                        )
                    .Result.SaveFilesAsync(Directory.GetCurrentDirectory()).Wait();
            } else
            {
                Console.WriteLine("Usage: compressandsplit.exe <CONVERTAPI_SECRET> <PDF_FILE>");
            }
        }
    }
}