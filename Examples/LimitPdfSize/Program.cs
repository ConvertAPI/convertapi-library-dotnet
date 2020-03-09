using ConvertApiDotNet;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace CompressAndSplit
{
    /// <summary>
    /// Compressing PDF and if the result is larger than SIZE_LIMIT_BYTES removing last pages to make file less than SIZE_LIMIT_BYTES.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 3)
            {
                var convertApi = new ConvertApi(args[0]);
                var maxSize = Int64.Parse(args[1]);
                var result = await convertApi.ConvertAsync("pdf", "compress", new[] { new ConvertApiFileParam(args[2]) });
                if (result.Files[0].FileSize > Int64.Parse(args[1]))
                {
                    var splitRes = await convertApi.ConvertAsync("pdf", "split", new[] { new ConvertApiParam("file", result) });
                    var size = 0;
                    var fileParams = splitRes.Files.ToList().Where(f =>
                    {
                        size += f.FileSize;
                        return size <= maxSize;
                    })
                    .Select(f => new ConvertApiFileParam(f));

                    var mergeRes = await convertApi.ConvertAsync("pdf", "merge", fileParams);
                    result = await convertApi.ConvertAsync("pdf", "compress", new[] { new ConvertApiParam("file", mergeRes) });
                }
                result.SaveFilesAsync(Directory.GetCurrentDirectory()).Wait();
            } else
            {
                Console.WriteLine("Usage: limitpdfsize.exe <CONVERTAPI_SECRET> <SIZE_LIMIT_BYTES> <PDF_FILE>");
            }
        }
    }
}