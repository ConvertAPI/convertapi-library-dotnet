using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvertApiDotNet.Constants;
using ConvertApiDotNet.Model;

namespace ConvertApiDotNet
{
    public static class ConvertApiExtension
    {
        #region Convert method extensions

        private static Task<ConvertApiResponse> BindConvertApi(ConvertApi convertApi, string fromFile, string outputExtension)
        {
            var fromExt = Path.GetExtension(fromFile).Replace(".", "");
            var task = convertApi.ConvertAsync(fromExt, outputExtension, new[] { new ConvertApiParam("File", File.OpenRead(fromFile)) });
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

        public static void Convert(this ConvertApi convertApi, string fromFile, string toFile)
        {
            var toExt = Path.GetExtension(toFile).Replace(".", "");
            var task = BindConvertApi(convertApi, fromFile, toExt);
            TaskResult(task).AsFileAsync(0, toFile).Wait();
        }

        public static void Convert(this ConvertApi convertApi, string fromFile, string outputExtension, string outputDirectory)
        {
            var task = BindConvertApi(convertApi, fromFile, outputExtension);
            TaskResult(task).SaveFiles(outputDirectory);
        }

        #endregion



        /// <summary>
        /// Return the count of converted files
        /// </summary>        
        /// <returns>Files converted</returns>
        public static int FileCount(this ConvertApiResponse response)
        {
            return response.Files.Length;
        }


        private static Task<Stream> AsStreamAsync(Uri url) => new ConvertApiBase(ConvertApiConstants.DownloadTimeoutInSeconds).HttpClient.GetStreamAsync(url);

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


        #region Files Task Methods

        public static IEnumerable<Task<Stream>> AsFilesStreamAsync(this ConvertApiResponse response)
        {
            return response.Files.Select(s => AsStreamAsync(s.Url));
        }

        public static IEnumerable<Task<FileInfo>> AsFilesAsync(this ConvertApiResponse response)
        {
            return response.Files.Select(s => AsFileAsync(s.Url, s.FileName));
        }

        #endregion


        #region File Task Methods

        public static Task<Stream> AsFileStreamAsync(this ConvertApiResponse response, int fileIndex)
        {
            return AsStreamAsync(response.Files[fileIndex].Url);
        }

        public static Task<FileInfo> AsFileAsync(this ConvertApiResponse response, int fileIndex, string fileName)
        {
            return AsFileAsync(response.Files[fileIndex].Url, fileName);
        }

        public static Task<Stream> AsFileStreamAsync(this ProcessedFile processedFile)
        {
            return AsStreamAsync(processedFile.Url);
        }

        public static Task<FileInfo> AsFileAsync(this ProcessedFile processedFile, string fileName)
        {
            return AsFileAsync(processedFile.Url, fileName);
        }

        #endregion

        #region Files Methods

        public static IEnumerable<Stream> FilesStream(this ConvertApiResponse response)
        {
            return response.AsFilesStreamAsync().Select(s=>s.Result);            
        }

        public static IEnumerable<FileInfo>SaveFiles(this ConvertApiResponse response, string directory)
        {
            return response.Files.Select(file => AsFileAsync(file.Url, Path.Combine(directory, Path.GetFileName(file.FileName)))).Select(task => task.Result).ToArray();
        }

        #endregion


        #region File Methods

        public static Stream FileStream(this ConvertApiResponse response)
        {
            return response.Files[0].FileStream();
        }

        public static FileInfo SaveFile(this ConvertApiResponse response, string fileName)
        {
            return response.Files[0].SaveFile(fileName);
        }

        public static Stream FileStream(this ProcessedFile processedFile)
        {
            return processedFile.AsFileStreamAsync().Result;
        }

        public static FileInfo SaveFile(this ProcessedFile processedFile, string fileName)
        {
            return processedFile.AsFileAsync(fileName).Result;
        }

        #endregion
    }
}