using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApi.Constants;
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

        private static ConvertApiResponse TaskResult(Task<ConvertApiResponse> task)
        {
            try
            {
                return task.Result;
            }
            catch (AggregateException e)
            {
                //Move actual exception from task which is written to InnerException and re-throw it
                var innerException = e.Flatten().InnerException;
                if (innerException != null)
                    throw innerException;

                throw;
            }
        }

        public static void Convert(this ConvertApiClient convertApiClient, string fromFile, string toFile)
        {
            var toExt = Path.GetExtension(toFile).Replace(".", "");
            var task = BindConvertApiClient(convertApiClient, fromFile, toExt);
            TaskResult(task).AsFileAsync(0, toFile).Wait();
        }

        public static void Convert(this ConvertApiClient convertApiClient, string fromFile, string outputExtension, string outputDirectory)
        {
            var task = BindConvertApiClient(convertApiClient, fromFile, outputExtension);
            TaskResult(task).SaveFiles(outputDirectory);
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

        public static Task<Stream> AsStreamAsync(Uri url) => new ConvertApiClientBase(ConvertApiConstants.DownloadTimeoutInSeconds).HttpClient.GetStreamAsync(url);

        private static IEnumerable<Task<Stream>> AsFilesStreamAsync(this ConvertApiResponse response)
        {
            return response.Files.Select(s => AsStreamAsync(s.Url));
        }

        private static Stream AsFilesStream(this ConvertApiResponse response, int fileIndex)
        {
            return AsStreamAsync(response.Files[fileIndex].Url).Result;
        }

        public static Task<FileInfo> AsFileAsync(this ConvertApiResponse response, int fileIndex, string fileName)
        {
            return AsFileAsync(response.Files[fileIndex].Url, fileName);
        }

        public static Task<FileInfo> AsFileAsync(this ProcessedFile processedFile, string fileName)
        {
            return AsFileAsync(processedFile.Url, fileName);
        }

        private static Task<FileInfo> AsFileAsync(Uri url, string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            return AsStreamAsync(url).ContinueWith(task =>
            {
                using (var fileStream = fileInfo.OpenWrite())
                    task.Result.CopyTo(fileStream);
                return fileInfo;
            });
        }

        public static FileInfo[] SaveFiles(this ConvertApiResponse response, string directory)
        {
            return response.Files.Select(file => AsFileAsync(file.Url, Path.Combine(directory, Path.GetFileName(file.FileName)))).Select(task => task.Result).ToArray();
        }

        public static FileInfo SaveFile(this ConvertApiResponse response, string fileName)
        {
            return response.Files[0].AsFileAsync(fileName).Result;
        }

        #endregion
    }
}