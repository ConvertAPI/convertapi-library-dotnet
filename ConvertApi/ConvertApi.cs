using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ConvertApiDotNet.Constants;
using ConvertApiDotNet.Exceptions;
using ConvertApiDotNet.Interface;
using ConvertApiDotNet.Model;
using Newtonsoft.Json;

namespace ConvertApiDotNet
{
    public class ConvertApi
    {
        private readonly string _secret;
        public static string ApiBaseUri = "https://v2.convertapi.com";
        private int _requestTimeoutInSeconds = 180;
        private static IConvertApiHttpClient _convertApiHttpClient;

        /// <summary>
        /// Initiate new instance of ConvertAPI client
        /// </summary>
        /// <param name="secret">Secret to authorize conversion can be found https://www.convertapi.com/a</param>
        /// <param name="convertApiHttpClient">Inject your own HttpClient</param>
        public ConvertApi(string secret, IConvertApiHttpClient convertApiHttpClient)
        {
            _secret = secret;
            _convertApiHttpClient = convertApiHttpClient;
        }

        public ConvertApi(string secret)
        {
            _secret = secret;
        }

        public static IConvertApiHttpClient GetClient()
        {
            return _convertApiHttpClient ?? (_convertApiHttpClient = new DefaultConvertApiHttpClient());
        }

        public void SetTimeOut(int requestTimeoutInSeconds)
        {
            _requestTimeoutInSeconds = requestTimeoutInSeconds;
        }

        public async Task<ConvertApiResponse> ConvertAsync(string fromFormat, string toFormat, params ConvertApiBaseParam[] parameters)
        {
            return await ConvertAsync(fromFormat, toFormat,  (IEnumerable<ConvertApiBaseParam>)parameters);
        }
        
        public async Task<ConvertApiResponse> ConvertAsync(string fromFormat, string toFormat, IEnumerable<ConvertApiBaseParam> parameters)
        {
            var content = new MultipartFormDataContent
            {
                {new StringContent("true"), "StoreFile"},
                {new StringContent(_requestTimeoutInSeconds.ToString()), "TimeOut"}
            };

            var ignoredParameters = new[] { "StoreFile", "Async", "JobId", "TimeOut" };


            var validParameters = parameters.Where(n => !ignoredParameters.Contains(n.Name, StringComparer.OrdinalIgnoreCase)).ToList();

            var dicList = new ParamDictionary();
            foreach (var parameter in validParameters)
            {
                if (parameter is ConvertApiParam)
                {
                    foreach (var value in (parameter as ConvertApiParam).GetValues())
                    {
                        dicList.Add(parameter.Name, value);
                    }
                }
                else
                if (parameter is ConvertApiFileParam)
                {
                    var convertApiUpload = await (parameter as ConvertApiFileParam).GetValueAsync();
                    if (convertApiUpload != null)
                    {
                        dicList.Add(parameter.Name, convertApiUpload);
                    }
                    else
                    {
                        foreach (var value in (parameter as ConvertApiFileParam).GetValues())
                        {
                            dicList.Add(parameter.Name, value);
                        }
                    }
                }
            }


            foreach (var s in dicList.Get())
            {
                switch (s.Value)
                {
                    case string value:
                        content.Add(new StringContent(value), s.Key);
                        break;
                    case ConvertApiFiles upload:
                        content.Add(new StringContent(upload.FileId), s.Key);

                        //Set FROM format if it is not set
                        if (string.Equals(fromFormat.ToLower(), "*", StringComparison.OrdinalIgnoreCase))
                        {
                            fromFormat = upload.FileExt;
                        }

                        break;

                }
            }

            var converter = dicList.Find("converter");

            if (!string.IsNullOrEmpty(converter))
                converter = $"/converter/{converter}";

            var url = new UriBuilder(ApiBaseUri)
            {
                Path = $"convert/{fromFormat}/to/{toFormat}{converter}",
                Query = $"secret={_secret}"
            };

            var response = await GetClient().PostAsync(url.Uri, _requestTimeoutInSeconds + 10, content);
            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new ConvertApiException(response.StatusCode,
                    $"Conversion from {fromFormat} to {toFormat} error. {response.ReasonPhrase}", result);
            return JsonConvert.DeserializeObject<ConvertApiResponse>(result);
        }

        /// <summary>
        /// Get user/account information
        /// </summary>
        /// <returns>Returns account status like user name, credits left and other information</returns>
        public async Task<ConvertApiUser> GetUserAsync()
        {
            var url = new UriBuilder(ApiBaseUri)
            {
                Path = "user",
                Query = $"secret={_secret}"
            };

            var response = await GetClient().GetAsync(url.Uri, ConvertApiConstants.DownloadTimeoutInSeconds);
            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new ConvertApiException(response.StatusCode, $"Retrieve user information failed. {response.ReasonPhrase}", result);
            return JsonConvert.DeserializeObject<ConvertApiUser>(result);
        }
    }
}
