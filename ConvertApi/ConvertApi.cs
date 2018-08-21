using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ConvertApiDotNet.Exceptions;
using ConvertApiDotNet.Model;
using Newtonsoft.Json;

namespace ConvertApiDotNet
{
    public class ConvertApi : ConvertApiBase
    {
        private readonly string _secret;
        public static string ApiBaseUri;
        private readonly string _requestTimeoutInSeconds;

        /// <summary>
        /// Initiate new instance of ConvertAPI client
        /// </summary>
        /// <param name="secret">Secret to authorize conversion can be found https://www.convertapi.com/a</param>
        /// <param name="requestTimeoutInSeconds">Conversion/request timeout</param>
        /// <param name="apiBaseUri">Default API base URL, in most cases used default</param>
        public ConvertApi(string secret, int requestTimeoutInSeconds = 180, string apiBaseUri = "https://v2.convertapi.com") : base(requestTimeoutInSeconds)
        {
            _secret = secret;
            ApiBaseUri = apiBaseUri;
            _requestTimeoutInSeconds = requestTimeoutInSeconds.ToString();
        }

        public async Task<ConvertApiResponse> ConvertAsync(string fromFormat, string toFormat, params ConvertApiParam[] parameters)
        {
            return await ConvertAsync(fromFormat, toFormat, (IEnumerable<ConvertApiParam>)parameters);
        }

        public async Task<ConvertApiResponse> ConvertAsync(string fromFormat, string toFormat, IEnumerable<ConvertApiParam> parameters)
        {
            var url = new UriBuilder(ApiBaseUri)
            {
                Path = $"convert/{fromFormat}/to/{toFormat}",
                Query = $"secret={_secret}"
            };

            var content = new MultipartFormDataContent
            {
                {new StringContent("true"), "StoreFile"},
                {new StringContent(_requestTimeoutInSeconds), "TimeOut"}
            };

            var ignoredParameters = new[] { "StoreFile", "Async", "JobId", "TimeOut" };

            var filesArray = new List<string>();

            foreach (var parameter in parameters)
            {
                if (ignoredParameters.Contains(parameter.Name, StringComparer.OrdinalIgnoreCase)) continue;

                if (parameter.Name.ToLower() == "files" || parameter.GetValues().Length > 1)
                {
                    filesArray.AddRange(parameter.GetValues());
                }
                else
                {
                    content.Add(new StringContent(parameter.GetValues().First()), parameter.Name);
                }
            }

            for (var index = 0; index < filesArray.Count; index++)
            {
                var file = filesArray[index];
                var parameterName = $"Files[{index}]" ;
                content.Add(new StringContent(file), parameterName);
            }

            return await HttpClient.PostAsync(url.Uri, content).ContinueWith(t =>
            {
                var responseMessage = t.Result;
                if (responseMessage.StatusCode != HttpStatusCode.OK)
                    throw new ConvertApiException($"Conversion from {fromFormat} to {toFormat} error.", responseMessage);
                return JsonConvert.DeserializeObject<ConvertApiResponse>(responseMessage.Content.ReadAsStringAsync().Result);
            });
        }

        public async Task<ConvertApiUser> GetUserAsync()
        {
            var url = new UriBuilder(ApiBaseUri)
            {
                Path = "user",
                Query = $"secret={_secret}"
            };
            return await HttpClient.GetAsync(url.Uri).ContinueWith(t =>
             {
                 var responseMessage = t.Result;
                 if (responseMessage.StatusCode != HttpStatusCode.OK)
                     throw new ConvertApiException("Retrieve user information failed.", responseMessage);
                 return JsonConvert.DeserializeObject<ConvertApiUser>(responseMessage.Content.ReadAsStringAsync().Result);
             });
        }
    }
}
