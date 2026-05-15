using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ConvertApiDotNet.Constants;
using ConvertApiDotNet.Exceptions;
using ConvertApiDotNet.Model;
using Newtonsoft.Json;

namespace ConvertApiDotNet
{
    public class ConvertApiBaseParam
    {
        public ConvertApiBaseParam(string name, string value)
        {
            Name = name;
            Value = new[] { value };
        }


        public ConvertApiBaseParam(string name, string[] values)
        {
            Name = name;
            Value = values;
        }

        public ConvertApiBaseParam(string name, Uri url)
        {
            Name = name;
            Value = new[] { url.ToString() };
        }

        /*protected ConvertApiBaseParam(string name, ConvertApiResponse convertApiResponse)
        {
            Name = name;
            Value = convertApiResponse.Files.Select(s => s.Url.ToString()).ToArray();
        }*/

        protected ConvertApiBaseParam(string name)
        {
            Name = name;
        }

        public string Name { get; }
        internal string[] Value;

        public IEnumerable<string> GetValues()
        {
            return Value;
        }
    }

    public class ConvertApiParam : ConvertApiBaseParam
    {
        public ConvertApiParam(string name, string value) : base(name, value)
        {
        }

        public ConvertApiParam(string name, int value) : this(name, value.ToString())
        {
        }

        public ConvertApiParam(string name, decimal value) : this(name, value.ToString(CultureInfo.InvariantCulture))
        {
        }
    }

    public class ConvertApiFileParam : ConvertApiBaseParam
    {
        private Task<ConvertApiFile> Tasks { get; set; }

        /// <summary>
        /// Convert remote file.
        /// </summary>
        /// <param name="url">Remote file url</param>
        public ConvertApiFileParam(Uri url) : this("file", url)
        {
        }

        public ConvertApiFileParam(string name, Uri url) : base(name)
        {
            Tasks = Upload(url);
        }

        /// <summary>
        /// Convert a local file or reference an already uploaded file by its FileId.
        /// </summary>
        /// <param name="path">Path to a local file or a 32-character lowercase FileId.</param>
        public ConvertApiFileParam(string path) : this("file", path)
        {
        }

        public ConvertApiFileParam(string name, string path) : base(name)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(
                    "Value must be a non-empty local file path or a 32-character lowercase FileId.",
                    nameof(path));

            // Prefer an existing local file path
            if (File.Exists(path))
            {
                Tasks = Upload(new FileInfo(path));
                return;
            }

            // If it looks like a FileId, pass it as-is (no upload)
            if (LooksLikeFileId(path))
            {
                Value = new[] { path };
                return;
            }

            // Neither a file on disk nor a valid FileId
            throw new FileNotFoundException(
                $"Value '{path}' is neither an existing local file nor a valid FileId (32 lowercase alphanumeric characters).",
                path);
        }

        /// <summary>
        /// Convert local file.
        /// </summary>
        /// <param name="file">Full path to local file</param>
        public ConvertApiFileParam(FileInfo file) : this("File", file)
        {
        }

        public ConvertApiFileParam(string name, FileInfo file) : base(name)
        {
            Tasks = Upload(file);
        }

        /// <summary>
        /// Convert file from stream
        /// </summary>
        /// <param name="fileStream">File stream</param>
        /// <param name="fileName">Set source file name.</param>
        public ConvertApiFileParam(Stream fileStream, string fileName) : this("File", fileStream, fileName)
        {
        }

        public ConvertApiFileParam(string name, Stream fileStream, string fileName) : base(name)
        {
            Tasks = Upload(fileStream, fileName);
        }

        public ConvertApiFileParam(ConvertApiFile processedFile) : this("File", processedFile)
        {
        }

        public ConvertApiFileParam(string name, ConvertApiFile processedFile) : base(name, processedFile.Url)
        {
        }

        public ConvertApiFileParam(ConvertApiResponse response) : this("File", response)
        {
        }

        public ConvertApiFileParam(string name, ConvertApiResponse response) : base(name)
        {
            Value = response.Files.Select(s => s.Url.ToString()).ToArray();
        }

        private static async Task<ConvertApiFile> Upload(FileInfo file)
        {
            using (var fileStream = file.OpenRead())
            {
                return await Upload(fileStream, fileStream.Name);
            }
        }

        private static async Task<ConvertApiFile> Upload(Stream fileStream, string fileName)
        {
            HttpResponseMessage responseMessage;
            using (var content = new StreamContent(fileStream))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileNameStar = Path.GetFileName(fileName)
                };

                var url = new UriBuilder(ConvertApi.ApiBaseUri)
                {
                    Path = "/upload",
                };

                responseMessage = await ConvertApi.GetClient().PostAsync(url.Uri, ConvertApiConstants.UploadTimeout, content, ConvertApi.ApiToken);
            }

            var result = await responseMessage.Content.ReadAsStringAsync();
            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new ConvertApiException(responseMessage.StatusCode, $"Unable to upload file. {responseMessage.ReasonPhrase}", result);
            }

            return JsonConvert.DeserializeObject<ConvertApiFile>(result);
        }

        private static async Task<ConvertApiFile> Upload(Uri remoteFileUrl)
        {
            var url = new UriBuilder(ConvertApi.ApiBaseUri)
            {
                Path = "/upload",
                Query = $"url={WebUtility.UrlEncode(remoteFileUrl.ToString())}"
            };

            var responseMessage = await ConvertApi.GetClient().PostAsync(url.Uri, ConvertApiConstants.UploadTimeout, null, ConvertApi.ApiToken);
            var result = await responseMessage.Content.ReadAsStringAsync();
            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new ConvertApiException(responseMessage.StatusCode, $"Unable to upload file. {responseMessage.ReasonPhrase}", result);
            }

            return JsonConvert.DeserializeObject<ConvertApiFile>(result);
        }

        /// <summary>
        /// Gets the uploaded file information if this instance initiated an upload.
        /// Returns null when this parameter was constructed from existing values (e.g., URL or response).
        /// </summary>
        public async Task<ConvertApiFile> GetUploadedFileAsync()
        {
            return Tasks == null ? null : await Tasks;
        }

        [Obsolete("Use GetUploadedFileAsync() instead.")]
        public async Task<ConvertApiFile> GetValueAsync()
        {
            return await GetUploadedFileAsync();
        }

        private static bool LooksLikeFileId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 32)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                var isDigit = ch >= '0' && ch <= '9';
                var isLower = ch >= 'a' && ch <= 'z';
                if (!isDigit && !isLower)
                    return false;
            }

            return true;
        }
    }
}