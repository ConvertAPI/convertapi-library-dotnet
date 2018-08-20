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

        public ConvertApiParam(string name, string value) : this(name, new[] { value }) { }

        public ConvertApiParam(string name, int value) : this(name, value.ToString()) { }

        public ConvertApiParam(string name, decimal value) : this(name, value.ToString(CultureInfo.InvariantCulture)) { }

        public ConvertApiParam(string name, Stream value, string fileName) : this(name, new string[0])
        {
            var client = new ConvertApiBase(ConvertApiConstants.UploadTimeoutInSeconds).HttpClient;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            var content = new StreamContent(value);
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
                   return uploadTask.Result.Content.ReadAsStringAsync().Result;
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
        public ConvertApiFileParam(Uri url) : base("File", url.ToString())
        {

        }

        public ConvertApiFileParam(string path) : base("File", File.OpenRead(path))
        {
        }

        public ConvertApiFileParam(Stream fileStream, string fileName) : base("File", fileStream, fileName)
        {
        }

        public ConvertApiFileParam(ConvertApiResponse response) : base("File", response)
        {
        }

        public ConvertApiFileParam(ProcessedFile processedFile) : this(processedFile.Url)
        {
        }
    }
}
