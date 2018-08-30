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
        public ConvertApiBaseParam(string name, string[] toArray)
        {
            Name = name;
            _value = toArray;
        }

        public ConvertApiBaseParam(string name, Uri url)
        {
            Name = name;
            _value = new[] { url.ToString()};         
        }

        protected ConvertApiBaseParam(string name, ConvertApiResponse convertApiResponse)
        {
            Name = name;
            _value = convertApiResponse.Files.Select(s => s.Url.ToString()).ToArray();
        }

        public ConvertApiBaseParam(string name)
        {
            Name = name;
        }

        public string Name { get; }
        internal Task<ConvertApiUpload> _tasks;
        internal string[] _value;

        public string[] GetValues()
        {
            return _value;
        }
    }

    public class ConvertApiParam : ConvertApiBaseParam
    {

        public ConvertApiParam(string name, string value) : base(name)
        {
            _value = new[] { value };            
        }

        public ConvertApiParam(string name, int value) : this(name, value.ToString()) { }

        public ConvertApiParam(string name, decimal value) : this(name, value.ToString(CultureInfo.InvariantCulture)) { }

        public ConvertApiParam(string name, ConvertApiResponse response) : base(name, response) { }

    }

    public class ConvertApiFileParam : ConvertApiBaseParam
    {
        public ConvertApiFileParam(string name, Stream fileStream, string fileName) : base(name)
        {
            var client = new ConvertApiBase(ConvertApiConstants.UploadTimeoutInSeconds).HttpClient;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Headers.Add("Content-Disposition", $"attachment; filename=\"{Path.GetFileName(fileName)}\"");

            var task = client.PostAsync(new Uri($"{ConvertApi.ApiBaseUri}/upload"), content)
                .ContinueWith(uploadTask =>
                {
                    var responseMessage = uploadTask.Result;
                    if (responseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        throw new ConvertApiException("Unable to upload file.", responseMessage);
                    }
                    return JsonConvert.DeserializeObject<ConvertApiUpload>(uploadTask.Result.Content.ReadAsStringAsync().Result);
                });
            _tasks = task;
        }

        public ConvertApiFileParam(string name, Uri remoteFileUrl) : base(name)
        {
            var client = new ConvertApiBase(ConvertApiConstants.UploadTimeoutInSeconds).HttpClient;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var task = client.PostAsync(new Uri($"{ConvertApi.ApiBaseUri}/upload-from-url?url={remoteFileUrl}"), null)
                .ContinueWith(uploadTask =>
                {
                    var responseMessage = uploadTask.Result;
                    if (responseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        throw new ConvertApiException("Unable to upload file.", responseMessage);
                    }
                    return JsonConvert.DeserializeObject<ConvertApiUpload>(uploadTask.Result.Content.ReadAsStringAsync().Result);
                });
            _tasks = task;
        }

        public ConvertApiUpload GetValue()
        {
            return _tasks?.Result;
        }

        public ConvertApiFileParam(Uri url) : this("File", url) { }

        public ConvertApiFileParam(string path) : this("File", File.OpenRead(path), Path.GetFileName(path)) { }

        public ConvertApiFileParam(FileInfo file) : this("File", file.OpenRead(), file.Name) { }

        public ConvertApiFileParam(Stream fileStream, string fileName) : this("File", fileStream, fileName) { }

        public ConvertApiFileParam(ProcessedFile processedFile) : this(processedFile.Url) { }

        public ConvertApiFileParam(ConvertApiResponse response) : base("File", response) { }
    }
}
