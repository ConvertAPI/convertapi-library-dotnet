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
        internal Task<ConvertApiUpload> Tasks;
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
        /// <summary>
        /// Convert remote file.
        /// </summary>
        /// <param name="url">Remote file url</param>
        public ConvertApiFileParam(Uri url) : base("File")
        {
            Upload(url);
        }

        /// <summary>
        /// Convert local file or pass File ID.
        /// </summary>
        /// <param name="path">Full path to local file or File ID.</param>
        public ConvertApiFileParam(string path) : base("File")
        {
            //If file then load as stream if not then assume that it is file id
            if (File.Exists(path))
                Upload(File.OpenRead(path), Path.GetFileName(path));
            else
                Value = new[] {path};
        }

        /// <summary>
        /// Convert local file.
        /// </summary>
        /// <param name="file">Full path to local file</param>
        public ConvertApiFileParam(FileInfo file) : base("File")
        {
            Upload(file.OpenRead(), file.Name);
        }

        /// <summary>
        /// Convert file from stream
        /// </summary>
        /// <param name="fileStream">File stream</param>
        /// <param name="fileName">Set source file name.</param>
        public ConvertApiFileParam(Stream fileStream, string fileName) : base("File")
        {
            Upload(fileStream, fileName);
        }
        
        public ConvertApiFileParam(ProcessedFile processedFile) : base("File", processedFile.Url) { }

        public ConvertApiFileParam(ConvertApiResponse response) : base("File", response) { }

        private void Upload(Stream fileStream, string fileName)
        {
            var client = new ConvertApiBase(ConvertApiConstants.UploadTimeoutInSeconds).HttpClient;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = Path.GetFileName(fileName)
            };

            var task = client.PostAsync(new Uri($"{ConvertApi.ApiBaseUri}/upload"), content)
                .ContinueWith(uploadTask =>
                {
                    var responseMessage = uploadTask.Result;
                    var result = uploadTask.Result.Content.ReadAsStringAsync().Result;
                    if (responseMessage.StatusCode != HttpStatusCode.OK)
                    {
                       throw new ConvertApiException(responseMessage.StatusCode, $"Unable to upload file. {responseMessage.ReasonPhrase}", result);
                    }
                    return JsonConvert.DeserializeObject<ConvertApiUpload>(result);
                });
            Tasks = task;
        }

        private void Upload(Uri remoteFileUrl)
        {
            var client = new ConvertApiBase(ConvertApiConstants.UploadTimeoutInSeconds).HttpClient;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var task = client.PostAsync(new Uri($"{ConvertApi.ApiBaseUri}/upload?url={remoteFileUrl}"), null)
                .ContinueWith(uploadTask =>
                {
                    var responseMessage = uploadTask.Result;
                    var result = uploadTask.Result.Content.ReadAsStringAsync().Result;
                    if (responseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        throw new ConvertApiException(responseMessage.StatusCode, $"Unable to upload file. {responseMessage.ReasonPhrase}", result);
                    }
                    return JsonConvert.DeserializeObject<ConvertApiUpload>(result);
                });
            Tasks = task;
        }

        public ConvertApiUpload GetValue()
        {
            return Tasks?.Result;
        }
    }
}
