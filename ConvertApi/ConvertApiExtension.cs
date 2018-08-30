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

        private static IEnumerable<ConvertApiBaseParam> JoinParameters(ConvertApiBaseParam convertApiFileParam,IEnumerable<ConvertApiBaseParam> parameters)
        {
            var paramsList = new List<ConvertApiBaseParam> {convertApiFileParam};
            paramsList.AddRange(parameters);
            return paramsList;
        }

        private static string GetPlainExtension(string fromFile)
        {
            return Path.GetExtension(fromFile).Replace(".", "");
        }

        private static Task<ConvertApiResponse> BindFile(ConvertApi convertApi, string fromFile, string outputExtension, IEnumerable<ConvertApiBaseParam> parameters)
        {
            return convertApi.ConvertAsync(GetPlainExtension(fromFile), outputExtension, JoinParameters(new ConvertApiFileParam(fromFile), parameters));
        }

        private static Task<ConvertApiResponse> BindFile(ConvertApi convertApi, Uri fileUrl, string outputExtension, IEnumerable<ConvertApiBaseParam> parameters)
        {            
            return convertApi.ConvertAsync("*", outputExtension, JoinParameters(new ConvertApiFileParam(fileUrl), parameters));
        }

        private static Task<ConvertApiResponse> BindUrl(ConvertApi convertApi, string url, string outputExtension, IEnumerable<ConvertApiBaseParam> parameters)
        {            
            return convertApi.ConvertAsync("web", outputExtension, JoinParameters(new ConvertApiParam("url", url), parameters));
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

        public static FileInfo ConvertFile(this ConvertApi convertApi, string fromFile, string toFile, params ConvertApiBaseParam[] parameters)
        {
            var task = BindFile(convertApi, fromFile, GetPlainExtension(toFile), parameters);
            return TaskResult(task).SaveFile(toFile);
        }

        public static IEnumerable<FileInfo> ConvertFile(this ConvertApi convertApi, string fromFile, string outputExtension, string outputDirectory, params ConvertApiBaseParam[] parameters)
        {
            var task = BindFile(convertApi, fromFile, outputExtension, parameters);
            return TaskResult(task).SaveFiles(outputDirectory);
        }

        public static FileInfo ConvertRemoteFile(this ConvertApi convertApi, string fileUrl, string toFile, params ConvertApiBaseParam[] parameters)
        {
            var task = BindFile(convertApi, new Uri(fileUrl), GetPlainExtension(toFile), parameters);
            return TaskResult(task).SaveFile(toFile);
        }

        public static IEnumerable<FileInfo> ConvertRemoteFile(this ConvertApi convertApi, string fileUrl, string outputExtension, string outputDirectory, params ConvertApiBaseParam[] parameters)
        {
            var task = BindFile(convertApi, new Uri(fileUrl), outputExtension, parameters);
            return TaskResult(task).SaveFiles(outputDirectory);
        }

        public static FileInfo ConvertUrl(this ConvertApi convertApi, string url, string toFile, params ConvertApiBaseParam[] parameters)
        {
            var task = BindUrl(convertApi, url, GetPlainExtension(toFile), parameters);
            return TaskResult(task).SaveFile(toFile);
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
            return response.AsFilesStreamAsync().Select(s => s.Result);
        }

        public static IEnumerable<FileInfo> SaveFiles(this ConvertApiResponse response, string directory)
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