using System;
using System.IO;
using System.Linq;
using ConvertApiDotNet;

namespace ConversionsChaining
{
    class Program
    {
        /// <summary>
        /// Short example of conversions chaining, the PDF pages extracted and saved as separated JPGs and then ZIP'ed
        /// https://www.convertapi.com/doc/chaining
        /// </summary>
        static void Main(string[] args)
        {
            //Get your secret at https://www.convertapi.com/a
            var convertApi = new ConvertApi("your api secret");
            Console.WriteLine("Converting PDF to JPG and compressing result files with ZIP");
            var fileName = Path.Combine(Path.GetTempPath(), "test.pdf");

            var firstTask = convertApi.ConvertAsync("pdf", "jpg", new[] { new ConvertApiParam("File", File.OpenRead(fileName)) });
            Console.WriteLine($"Conversions done. Cost: {firstTask.Result.ConversionCost}. Total files created: {firstTask.Result.FileCount()}");            

            var secondsTask = convertApi.ConvertAsync("jpg", "zip", new[] { new ConvertApiParam("Files", firstTask.Result) });
            var saveFiles = secondsTask.Result.SaveFiles(Path.GetTempPath());

            Console.WriteLine($"Conversions done. Cost: {secondsTask.Result.ConversionCost}. Total files created: {secondsTask.Result.FileCount()}");
            Console.WriteLine($"File saved to {saveFiles.First().FullName}");

            Console.ReadLine();
        }
    }
}
