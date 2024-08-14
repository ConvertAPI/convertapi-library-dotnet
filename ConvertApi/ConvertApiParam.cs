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
        private Task<ConvertApiFiles> Tasks { get; set; }

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
        /// Convert local file or pass File ID.
        /// </summary>
        /// <param name="path">Full path to local file or File ID.</param>
        public ConvertApiFileParam(string path) : this("file", path)
        {
        }

        public ConvertApiFileParam(string name, string path) : base(name)
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

        public ConvertApiFileParam(ConvertApiFiles processedFile) : this("File", processedFile)
        {
        }

        public ConvertApiFileParam(string name, ConvertApiFiles processedFile) : base(name, processedFile.Url)
        {
        }

        public ConvertApiFileParam(ConvertApiResponse response) : this("File", response)
        {
        }

        public ConvertApiFileParam(string name, ConvertApiResponse response) : base(name)
        {
            Value = response.Files.Select(s => s.Url.ToString()).ToArray();
        }

        private static async Task<ConvertApiFiles> Upload(FileInfo file)
        {
            using (var fileStream = file.OpenRead())
            {
                return await Upload(fileStream, fileStream.Name);
            }
        }

        private static async Task<ConvertApiFiles> Upload(Stream fileStream, string fileName)
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

                responseMessage = await ConvertApi.GetClient().PostAsync(url.Uri, ConvertApiConstants.UploadTimeout, content, ConvertApi.AuthCredentials);
            }

            var result = await responseMessage.Content.ReadAsStringAsync();
            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new ConvertApiException(responseMessage.StatusCode, $"Unable to upload file. {responseMessage.ReasonPhrase}", result);
            }

            return JsonConvert.DeserializeObject<ConvertApiFiles>(result);
        }

        private static async Task<ConvertApiFiles> Upload(Uri remoteFileUrl)
        {
            var url = new UriBuilder(ConvertApi.ApiBaseUri)
            {
                Path = "/upload",
                Query = $"url={WebUtility.UrlEncode(remoteFileUrl.ToString())}"
            };

            var responseMessage = await ConvertApi.GetClient().PostAsync(url.Uri, ConvertApiConstants.UploadTimeout, null, ConvertApi.AuthCredentials);
            var result = await responseMessage.Content.ReadAsStringAsync();
            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new ConvertApiException(responseMessage.StatusCode, $"Unable to upload file. {responseMessage.ReasonPhrase}", result);
            }

            return JsonConvert.DeserializeObject<ConvertApiFiles>(result);
        }

        public async Task<ConvertApiFiles> GetValueAsync()
        {
            return Tasks == null ? null : await Tasks;
        }
    }
}