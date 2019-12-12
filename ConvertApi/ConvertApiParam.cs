using System;
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

        protected ConvertApiBaseParam(string name, ConvertApiResponse convertApiResponse)
        {
            Name = name;
            Value = convertApiResponse.Files.Select(s => s.Url.ToString()).ToArray();
        }

        public ConvertApiBaseParam(string name)
        {
            Name = name;
        }

        public string Name { get; }
        internal string[] Value;

        public string[] GetValues()
        {
            return Value;
        }
    }

    public class ConvertApiParam : ConvertApiBaseParam
    {
        public ConvertApiParam(string name, string value) : base(name, value) { }

        public ConvertApiParam(string name, int value) : this(name, value.ToString()) { }

        public ConvertApiParam(string name, decimal value) : this(name, value.ToString(CultureInfo.InvariantCulture)) { }

        public ConvertApiParam(string name, ConvertApiResponse response) : base(name, response) { }

    }

    public class ConvertApiFileParam : ConvertApiBaseParam
    {
        internal Task<ConvertApiUpload> Tasks { get; set; }

        /// <summary>
        /// Convert remote file.
        /// </summary>
        /// <param name="url">Remote file url</param>
        public ConvertApiFileParam(Uri url) : base("File")
        {
            Tasks = Upload(url);
        }

        /// <summary>
        /// Convert local file or pass File ID.
        /// </summary>
        /// <param name="path">Full path to local file or File ID.</param>
        public ConvertApiFileParam(string path) : base("File")
        {
            //If file then load as stream if not then assume that it is file id
            if (File.Exists(path))
            {
                Tasks = Upload(new FileInfo(path));
            }
            else
                Value = new[] { path };
        }

        /// <summary>
        /// Convert local file.
        /// </summary>
        /// <param name="file">Full path to local file</param>
        public ConvertApiFileParam(FileInfo file) : base("File")
        {
            Tasks = Upload(file);
        }

        /// <summary>
        /// Convert file from stream
        /// </summary>
        /// <param name="fileStream">File stream</param>
        /// <param name="fileName">Set source file name.</param>
        public ConvertApiFileParam(Stream fileStream, string fileName) : base("File")
        {
            Tasks = Upload(fileStream, fileName);
        }

        public ConvertApiFileParam(ProcessedFile processedFile) : base("File", processedFile.Url) { }

        public ConvertApiFileParam(ConvertApiResponse response) : base("File", response) { }

        private static async Task<ConvertApiUpload> Upload(FileInfo file)
        {
            using (var fileStream = file.OpenRead())
            {
                return await Upload(fileStream, fileStream.Name);
            }
        }

        private static async Task<ConvertApiUpload> Upload(Stream fileStream, string fileName)
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

                responseMessage = await ConvertApi.GetClient().PostAsync(url.Uri, ConvertApiConstants.UploadTimeoutInSeconds, content);
            }

            var result = await responseMessage.Content.ReadAsStringAsync();
            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new ConvertApiException(responseMessage.StatusCode, $"Unable to upload file. {responseMessage.ReasonPhrase}", result);
            }
            return JsonConvert.DeserializeObject<ConvertApiUpload>(result);
        }

        private static async Task<ConvertApiUpload> Upload(Uri remoteFileUrl)
        {
            var url = new UriBuilder(ConvertApi.ApiBaseUri)
            {
                Path = "/upload",
                Query = $"url={remoteFileUrl}"
            };

            var responseMessage = await ConvertApi.GetClient().PostAsync(url.Uri, ConvertApiConstants.UploadTimeoutInSeconds,null);
            var result = await responseMessage.Content.ReadAsStringAsync();
            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new ConvertApiException(responseMessage.StatusCode, $"Unable to upload file. {responseMessage.ReasonPhrase}", result);
            }
            return JsonConvert.DeserializeObject<ConvertApiUpload>(result);
        }

        public async Task<ConvertApiUpload> GetValueAsync()
        {
            return Tasks == null ? null : await Tasks;
        }
    }
}
