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
    public class ConvertApiParam
    {
        public string Name { get; }
        private readonly string[] _values;
        private readonly List<Task<string>> _tasks = new List<Task<string>>();

        public ConvertApiParam(string name, string[] values)
        {
            Name = name;
            _values = values;
        }

        public ConvertApiParam(string name)
        {
            Name = name;
            _values = new string[0];
        }

        public ConvertApiParam(string name, string value) : this(name, new[] { value }) { }

        public ConvertApiParam(string name, int value) : this(name, value.ToString()) { }

        public ConvertApiParam(string name, decimal value) : this(name, value.ToString(CultureInfo.InvariantCulture)) { }

        public ConvertApiParam(string name, Uri remoteFileUrl) : this(name)
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
                    return uploadTask.Result.Content.ReadAsStringAsync().Result;
                });
            _tasks.Add(task);
        }

        public ConvertApiParam(string name, Stream fileStream, string fileName) : this(name)
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
                   return JsonConvert.DeserializeObject<ConvertApiUpload>(uploadTask.Result.Content.ReadAsStringAsync().Result).FileId;
               });
            _tasks.Add(task);
        }

        public ConvertApiParam(string name, FileStream value) : this(name, value, value.Name) { }

        public ConvertApiParam(string name, ConvertApiResponse response) : this(name, response.Files.Select(s => s.Url.ToString()).ToArray()) { }

        public string[] GetValues()
        {
            return _tasks.Count == 0 ? _values : _tasks.Select(t => t.Result).ToArray();
        }
    }

    public class ConvertApiFileParam : ConvertApiParam
    {
        public ConvertApiFileParam(Uri url) : base("File", url)
        {

        }

        public ConvertApiFileParam(string path) : base("File", File.OpenRead(path))
        {
        }

        public ConvertApiFileParam(FileInfo file) : base("File", file.OpenRead())
        {
        }

        public ConvertApiFileParam(Stream fileStream, string fileName) : base("File", fileStream, fileName)
        {
        }

        public ConvertApiFileParam(ConvertApiResponse response) : this(response.Files.First())
        {
        }

        public ConvertApiFileParam(ProcessedFile processedFile) : this(processedFile.Url)
        {
        }
    }

    public class ConvertApiFilesParam : ConvertApiParam
    {
        public ConvertApiFilesParam(Uri url) : base("Files", url.ToString())
        {

        }

        public ConvertApiFilesParam(string path) : base("Files", File.OpenRead(path))
        {
        }

        public ConvertApiFilesParam(FileInfo file) : base("Files", file.OpenRead())
        {
        }

        public ConvertApiFilesParam(Stream fileStream, string fileName) : base("Files", fileStream, fileName)
        {
        }

        public ConvertApiFilesParam(ConvertApiResponse response) : base("Files", response)
        {
        }

        public ConvertApiFilesParam(ProcessedFile processedFile) : this(processedFile.Url)
        {
        }
    }
}
