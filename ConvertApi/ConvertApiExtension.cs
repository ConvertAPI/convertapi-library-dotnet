using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ConvertApiDotNet.Constants;
using ConvertApiDotNet.Model;

namespace ConvertApiDotNet
{

    public static class ConvertApiExtension
    {
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
            var httpResponseMessage = await ConvertApi.GetClient().GetAsync(url, ConvertApiConstants.DownloadTimeout);
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