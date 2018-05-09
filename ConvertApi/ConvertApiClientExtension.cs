using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ConvertApi.Model;

namespace ConvertApi
{
    public static class ConvertApiClientExtension
    {
        #region Convert method extensions

        private static Task<ConvertApiResponse> BindConvertApiClient(ConvertApiClient convertApiClient, string fromFile, string outputExtension)
        {
            var fromExt = Path.GetExtension(fromFile).Replace(".", "");            
            var task = convertApiClient.ConvertAsync(fromExt, outputExtension, new[] { new ConvertApiParam("File", File.OpenRead(fromFile)) });
            return task;
        }

        public static void Convert(this ConvertApiClient convertApiClient, string fromFile, string toFile)
        {
            var toExt = Path.GetExtension(toFile).Replace(".", "");
            var task = BindConvertApiClient(convertApiClient, fromFile, toExt);
            task.Result.AsFileAsync(0, new FileInfo(toFile)).Wait();
        }

        public static void Convert(this ConvertApiClient convertApiClient, string fromFile, string outputExtension, string outputDirectory)
        {            
            var task = BindConvertApiClient(convertApiClient, fromFile, outputExtension);
            task.Result.SaveFiles(outputDirectory);
        }

        #endregion

        #region Result extensions

        /// <summary>
        /// Return the count of converted files
        /// </summary>        
        /// <returns>Files converted</returns>
        public static int FileCount(this ConvertApiResponse response)
        {
            return response.Files.Length;
        }

        public static Task<Stream> AsStreamAsync(Uri url) => new HttpClient().GetStreamAsync(url);

        private static IEnumerable<Task<Stream>> AsFilesStreamAsync(this ConvertApiResponse response)
        {
            return response.Files.Select(s => AsStreamAsync(s.Url));
        }

        private static Stream AsFilesStream(this ConvertApiResponse response, int fileIndex)
        {
            return AsStreamAsync(response.Files[fileIndex].Url).Result;
        }

        private static Task<FileInfo> AsFileAsync(this ConvertApiResponse response, int fileIndex, FileInfo fileInfo)
        {
            return AsFileAsync(response.Files[fileIndex].Url, fileInfo);
        }

        private static Task<FileInfo> AsFileAsync(Uri url, FileInfo fileInfo)
        {
            return AsStreamAsync(url).ContinueWith(task =>
            {
                using (var fileStream = fileInfo.OpenWrite()) task.Result.CopyTo(fileStream);
                return fileInfo;
            });
        }

        public static FileInfo[] SaveFiles(this ConvertApiResponse response, string directory)
        {
            return response.Files.Select(file =>
            {
                var fileInfo = new FileInfo(Path.Combine(directory, Path.GetFileName(file.FileName)));
                return AsFileAsync(file.Url, fileInfo);
            }).Select(task => task.Result).ToArray();
        }

        #endregion
    }
}