using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
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
                // FileMode.Create truncates an existing file. FileInfo.OpenWrite (FileMode.OpenOrCreate) did not,
                // so a result saved over a longer file kept that file's trailing bytes and came out corrupt.
                using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
                    await readFile.CopyToAsync(fileStream);
            }

            return fileInfo;
        }

        #region File Task Methods
        /// <summary>
        /// Save the first result file as <paramref name="fileName"/>, replacing the file if it exists.
        /// </summary>
        public static async Task<FileInfo> SaveFileAsync(this ConvertApiResponse response, string fileName)
        {
            return await response.Files[0].SaveFileAsync(fileName);
        }

        /// <summary>
        /// Save every result file into <paramref name="outputDirectory"/> under its own name, replacing files
        /// that exist. The folder is created if it does not exist.
        /// </summary>
        public static async Task<List<FileInfo>> SaveFilesAsync(this ConvertApiResponse response, string outputDirectory)
        {
            return await response.Files.SaveFilesAsync(outputDirectory);
        }

        public static async Task<Stream> FileStreamAsync(this ConvertApiFile processedFile)
        {
            return await AsStreamAsync(processedFile.Url);
        }

        /// <summary>
        /// Save the file as <paramref name="fileName"/>, replacing the file if it exists.
        /// </summary>
        public static async Task<FileInfo> SaveFileAsync(this ConvertApiFile processedFile, string fileName)
        {
            return await SaveFileAsync(processedFile.Url, fileName);
        }

        /// <summary>
        /// Save the files into <paramref name="outputDirectory"/>, each under its own name, replacing files that
        /// exist. The folder is created if it does not exist.
        /// </summary>
        public static async Task<List<FileInfo>> SaveFilesAsync(this IEnumerable<ConvertApiFile> processedFile, string outputDirectory)
        {
            // An empty path still means the current directory, as Path.Combine("", name) below always did.
            if (!string.IsNullOrEmpty(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

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
        public static async Task<int> DeleteFilesAsync(this IEnumerable<ConvertApiFile> processedFile)
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

        /// <summary>
        /// Delete all files related to a conversion: output files from the response and any uploaded input files
        /// used by the provided parameters. If not deleted explicitly, files are automatically removed after 3 hours.
        /// </summary>
        /// <param name="response">Conversion response containing destination files.</param>
        /// <param name="parameters">Parameters used for the conversion (to detect uploaded input files).</param>
        /// <returns>Total count of successfully deleted files.</returns>
        public static async Task<int> DeleteFilesAsync(this ConvertApiResponse response, IEnumerable<ConvertApiBaseParam> parameters)
        {
            var allFiles = new List<ConvertApiFile>();

            if (response?.Files != null && response.Files.Length > 0)
                allFiles.AddRange(response.Files);

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (p is ConvertApiFileParam fileParam)
                    {
                        // Only parameters that initiated an upload have an uploaded file to delete
                        var uploaded = await fileParam.GetUploadedFileAsync();
                        if (uploaded != null)
                            allFiles.Add(uploaded);
                    }
                }
            }

            // Deduplicate by FileId when available, otherwise by absolute URL
            var unique = allFiles
                .Where(f => f != null && (f.Url != null || !string.IsNullOrWhiteSpace(f.FileId)))
                .GroupBy(f => !string.IsNullOrWhiteSpace(f.FileId) ? f.FileId : f.Url.ToString(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            return await DeleteFilesAsync((IEnumerable<ConvertApiFile>)unique);
        }

        /// <summary>
        /// Delete all files related to a conversion: output files from the response and any uploaded input files
        /// used by the provided parameters. If not deleted explicitly, files are automatically removed after 3 hours.
        /// </summary>
        /// <param name="response">Conversion response containing destination files.</param>
        /// <param name="parameters">Parameters used for the conversion (to detect uploaded input files).</param>
        /// <returns>Total count of successfully deleted files.</returns>
        public static Task<int> DeleteFilesAsync(this ConvertApiResponse response, params ConvertApiBaseParam[] parameters)
        {
            return DeleteFilesAsync(response, parameters?.AsEnumerable());
        }

        /// <summary>
        /// Delete all files related to a conversion without needing to pass parameters.
        /// This removes destination files from the response and any input files uploaded
        /// during the same conversion call (tracked on the response instance).
        /// If not deleted explicitly, files are automatically removed after 3 hours.
        /// </summary>
        /// <param name="response">Conversion response that holds destination files and tracked uploaded inputs.</param>
        /// <returns>Total count of successfully deleted files.</returns>
        public static async Task<int> DeleteFilesAsync(this ConvertApiResponse response)
        {
            var allFiles = new List<ConvertApiFile>();

            if (response?.Files != null && response.Files.Length > 0)
                allFiles.AddRange(response.Files);

            if (response?.UploadedInputFiles != null && response.UploadedInputFiles.Count > 0)
                allFiles.AddRange(response.UploadedInputFiles);

            var unique = allFiles
                .Where(f => f != null && (f.Url != null || !string.IsNullOrWhiteSpace(f.FileId)))
                .GroupBy(f => !string.IsNullOrWhiteSpace(f.FileId) ? f.FileId : f.Url.ToString(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            return await DeleteFilesAsync((IEnumerable<ConvertApiFile>)unique);
        }

        #endregion
    }
}