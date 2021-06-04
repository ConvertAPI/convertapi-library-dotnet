using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ConvertApiDotNet.Constants;
using ConvertApiDotNet.Model;

namespace ConvertApiDotNet
{

    public static class ConvertApiExtension
    {
        #region Convert method extensions

        /// <summary>
        /// Waits for the task to complete, unwrapping any exceptions.
        /// </summary>
        /// <param name="task">The task. May not be <c>null</c>.</param>
        private static void WaitAndUnwrapException(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Waits for the task to complete, unwrapping any exceptions.
        /// </summary>
        /// <param name="task">The task. May not be <c>null</c>.</param>
        private static T WaitAndUnwrapException<T>(this Task<T> task)
        {
            return task.GetAwaiter().GetResult();
        }

        private static IEnumerable<ConvertApiBaseParam> JoinParameters(ConvertApiBaseParam convertApiFileParam, IEnumerable<ConvertApiBaseParam> parameters)
        {
            var paramsList = new List<ConvertApiBaseParam> { convertApiFileParam };
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

        public static FileInfo ConvertFile(this ConvertApi convertApi, string fromFile, string toFile, params ConvertApiBaseParam[] parameters)
        {
            var task = BindFile(convertApi, fromFile, GetPlainExtension(toFile), parameters);
            return task.WaitAndUnwrapException().Files[0].SaveFileAsync(toFile).WaitAndUnwrapException();
        }

        public static IEnumerable<FileInfo> ConvertFile(this ConvertApi convertApi, string fromFile, string outputExtension, string outputDirectory, params ConvertApiBaseParam[] parameters)
        {
            var task = BindFile(convertApi, fromFile, outputExtension, parameters);
            var parallelQuery = task.WaitAndUnwrapException().Files.AsParallel().WithDegreeOfParallelism(6)
                .Select(s => s.SaveFileAsync(Path.Combine(outputDirectory, s.FileName)).WaitAndUnwrapException());

            var l = new List<FileInfo>();
            l.AddRange(parallelQuery);

            return l;
        }

        public static FileInfo ConvertRemoteFile(this ConvertApi convertApi, string fileUrl, string toFile, params ConvertApiBaseParam[] parameters)
        {
            var task = BindFile(convertApi, new Uri(fileUrl), GetPlainExtension(toFile), parameters);
            return task.WaitAndUnwrapException().Files[0].SaveFileAsync(toFile).WaitAndUnwrapException();
        }

        public static FileInfo ConvertUrl(this ConvertApi convertApi, string url, string toFile, params ConvertApiBaseParam[] parameters)
        {
            var outputExtension = GetPlainExtension(toFile);
            var task = convertApi.ConvertAsync("web", outputExtension, JoinParameters(new ConvertApiParam("url", url), parameters));
            return task.WaitAndUnwrapException().Files[0].SaveFileAsync(toFile).WaitAndUnwrapException();
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


        private static async Task<Stream> AsStreamAsync(Uri url)
        {
            var httpResponseMessage = await ConvertApi.GetClient().GetAsync(url, ConvertApiConstants.DownloadTimeoutInSeconds);
            return await httpResponseMessage.Content.ReadAsStreamAsync();
        }

        private static async Task<FileInfo> SaveFileAsync(Uri url, string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            using (var readFile = await AsStreamAsync(url))
            {
                using (var fileStream = fileInfo.OpenWrite())
                    readFile.CopyTo(fileStream);
            }

            return fileInfo;
        }

        #region File Task Methods
        public static async Task<FileInfo> SaveFileAsync(this ConvertApiResponse response, string fileName)
        {
            return await response.Files[0].SaveFileAsync(fileName);
        }

        public static async Task<List<FileInfo>> SaveFilesAsync(this ConvertApiResponse response, string outputDirectory)
        {
            return await response.Files.SaveFilesAsync(outputDirectory);
        }

        public static async Task<Stream> FileStreamAsync(this ConvertApiFiles processedFile)
        {
            return await AsStreamAsync(processedFile.Url);
        }

        public static async Task<FileInfo> SaveFileAsync(this ConvertApiFiles processedFile, string fileName)
        {
            return await SaveFileAsync(processedFile.Url, fileName);
        }

        public static async Task<List<FileInfo>> SaveFilesAsync(this IEnumerable<ConvertApiFiles> processedFile, string outputDirectory)
        {
            var list = new List<FileInfo>();
            foreach (var file in processedFile)
            {
                list.Add(await file.SaveFileAsync(Path.Combine(outputDirectory, file.FileName)));
            }

            return list;
        }
        
        /// <summary>
        /// Delete files from the ConvertAPI server, and if left, they automatically will be deleted after 3 hours. 
        /// </summary>
        /// <param name="processedFile">Files to delete.</param>
        /// <returns>Returns deleted files count.</returns>
        public static async Task<int> DeleteFilesAsync(this IEnumerable<ConvertApiFiles> processedFile)
        {
            var httpClient = ConvertApi.GetClient().Client;
            var count = 0;
            foreach (var file in processedFile)
            {
                var httpResponseMessage = await httpClient.DeleteAsync(file.Url);
                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    count += 1;
            }

            return count;
        }

        #endregion
    }
}