using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using ConvertApiDotNet.Constants;
using ConvertApiDotNet.Exceptions;
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
            // Anything but 200 is an error page, not the file: throw instead of handing it over (or saving it) as the
            // result. Earlier versions returned the error body, so SaveFileAsync wrote it into the output file.
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                using (httpResponseMessage)
                {
                    var body = await httpResponseMessage.Content.ReadAsStringAsync();
                    throw new ConvertApiException(httpResponseMessage.StatusCode, ConvertApiException.MessageFor(
                        httpResponseMessage.StatusCode, $"Unable to download file. {httpResponseMessage.ReasonPhrase}"), body);
                }
            }
            return await httpResponseMessage.Content.ReadAsStreamAsync();
        }

        private static async Task<FileInfo> SaveFileAsync(Uri url, string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            using (var readFile = await AsStreamAsync(url))
            {
                // OpenWrite (FileMode.OpenOrCreate) does not truncate, so after the copy the file is cut to what was
                // written: otherwise a result saved over a longer file keeps that file's trailing bytes. FileMode.Create
                // would truncate, but on Windows it fails on an existing hidden or system file, which OpenWrite writes.
                // A stream that cannot seek (a device such as /dev/null) has no length to set.
                using (var fileStream = fileInfo.OpenWrite())
                {
                    await readFile.CopyToAsync(fileStream);
                    if (fileStream.CanSeek && fileStream.Length > fileStream.Position)
                        fileStream.SetLength(fileStream.Position);
                }
            }

            return fileInfo;
        }

        #region File Task Methods
        /// <summary>
        /// Save the first result file as <paramref name="fileName"/>, replacing the file if it exists.
        /// </summary>
        /// <exception cref="ConvertApiException">The download answered with anything but 200 OK.</exception>
        public static async Task<FileInfo> SaveFileAsync(this ConvertApiResponse response, string fileName)
        {
            return await response.Files[0].SaveFileAsync(fileName);
        }

        /// <summary>
        /// Save every result file into <paramref name="outputDirectory"/> under its own name, replacing files
        /// that exist. The folder is created if it does not exist.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="outputDirectory"/> is blank, is an existing file, or
        /// looks like a result file name. To save one result as a file, call <c>SaveFileAsync</c>.</exception>
        /// <exception cref="ConvertApiException">A download answered with anything but 200 OK.</exception>
        public static async Task<List<FileInfo>> SaveFilesAsync(this ConvertApiResponse response, string outputDirectory)
        {
            return await response.Files.SaveFilesAsync(outputDirectory);
        }

        /// <summary>
        /// Download the file as a stream, for example to send it on without saving it. Dispose the stream when done.
        /// </summary>
        /// <exception cref="ConvertApiException">The download answered with anything but 200 OK.</exception>
        public static async Task<Stream> FileStreamAsync(this ConvertApiFile processedFile)
        {
            return await AsStreamAsync(processedFile.Url);
        }

        /// <summary>
        /// Save the file as <paramref name="fileName"/>, replacing the file if it exists.
        /// </summary>
        /// <exception cref="ConvertApiException">The download answered with anything but 200 OK; no file is written.</exception>
        public static async Task<FileInfo> SaveFileAsync(this ConvertApiFile processedFile, string fileName)
        {
            return await SaveFileAsync(processedFile.Url, fileName);
        }

        /// <summary>
        /// Save the files into <paramref name="outputDirectory"/>, each under its own name, replacing files that
        /// exist. The folder is created if it does not exist. An empty string means the current folder.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="outputDirectory"/> is blank, is an existing file, or
        /// looks like a result file name (for example "result.pdf" for PDF results). To save one result as a file,
        /// call <c>SaveFileAsync</c>. To use a new folder whose name has an extension, end the path with a slash.</exception>
        /// <exception cref="ConvertApiException">A download answered with anything but 200 OK.</exception>
        public static async Task<List<FileInfo>> SaveFilesAsync(this IEnumerable<ConvertApiFile> processedFile, string outputDirectory)
        {
            var files = processedFile.ToList();
            var list = new List<FileInfo>();
            // Nothing to save: no folder is created, as in versions before the folder was created.
            if (files.Count == 0)
                return list;

            // An empty path still means the current directory, as Path.Combine("", name) below always did.
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                CheckIsAFolder(outputDirectory, files);
                Directory.CreateDirectory(outputDirectory);
            }

            foreach (var file in files)
            {
                list.Add(await file.SaveFileAsync(Path.Combine(outputDirectory, file.FileName)));
            }

            return list;
        }

        // SaveFilesAsync("result.pdf") would otherwise create a folder named result.pdf, which then blocks
        // SaveFileAsync("result.pdf"). Checked before anything is downloaded.
        private static void CheckIsAFolder(string outputDirectory, List<ConvertApiFile> files)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentException("The output folder is blank. Pass \"\" or \".\" for the current folder.", nameof(outputDirectory));

            var endsInSeparator = outputDirectory.EndsWith(Path.DirectorySeparatorChar.ToString())
                                  || outputDirectory.EndsWith(Path.AltDirectorySeparatorChar.ToString());
            var extension = Path.GetExtension(outputDirectory);
            var looksLikeAResultFile = !endsInSeparator
                                       && extension.Length > 1
                                       && !Directory.Exists(outputDirectory)
                                       && files.Any(f => string.Equals(Path.GetExtension(f.FileName ?? ""), extension, StringComparison.OrdinalIgnoreCase));

            if (File.Exists(outputDirectory) || looksLikeAResultFile)
                throw new ArgumentException(
                    $"SaveFilesAsync saves into a folder, and '{outputDirectory}' is a file name. To save the result as '{outputDirectory}', " +
                    "call SaveFileAsync. To use a new folder with this name, end the path with a slash.", nameof(outputDirectory));
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