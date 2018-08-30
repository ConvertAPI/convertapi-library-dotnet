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

        public async Task<ConvertApiResponse> ConvertAsync(string fromFormat, string toFormat, params ConvertApiBaseParam[] parameters)
        {
            return await ConvertAsync(fromFormat, toFormat, (IEnumerable<ConvertApiBaseParam>)parameters);
        }

        public class Dic
        {
            private readonly Dictionary<string, List<object>> _dictionary;

            public Dic()
            {
                _dictionary = new Dictionary<string, List<object>>();
            }

            //Check for duplicate string and add S at the end of parameter
            public Dictionary<string, object> Get()
            {
                var dic = new Dictionary<string, object>();
                foreach (var keyValuePair in _dictionary)
                {
                    if (keyValuePair.Value.Count == 1)
                        dic.Add(keyValuePair.Key, keyValuePair.Value[0]);
                    else
                    {
                        for (var index = 0; index < keyValuePair.Value.Count; index++)
                        {
                            string name;
                            if (!keyValuePair.Key.EndsWith("s"))
                                name = keyValuePair.Key + "s";
                            else
                                name = keyValuePair.Key;
                            dic.Add(name + "[" + index + "]", keyValuePair.Value[index]);
                        }
                    }
                }

                return dic;
            }


            public void Add(string key, object value)
            {
                var keyToAdd = key.ToLower();

                if (!_dictionary.ContainsKey(keyToAdd))
                {
                    _dictionary.Add(keyToAdd, new List<object> { value });
                }
                else
                {
                    _dictionary[keyToAdd].Add(value);
                }
            }
        }

        public async Task<ConvertApiResponse> ConvertAsync(string fromFormat, string toFormat, IEnumerable<ConvertApiBaseParam> parameters)
        {
            var content = new MultipartFormDataContent
            {
                {new StringContent("true"), "StoreFile"},
                {new StringContent(_requestTimeoutInSeconds), "TimeOut"}
            };

            var ignoredParameters = new[] { "StoreFile", "Async", "JobId", "TimeOut" };


            var validParameters = parameters.Where(n => !ignoredParameters.Contains(n.Name, StringComparer.OrdinalIgnoreCase)).ToList();


            var dicList = new Dic();
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
                    var convertApiUpload = (parameter as ConvertApiFileParam).GetValue();
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
                    case ConvertApiUpload upload:
                        content.Add(new StringContent(upload.FileId), s.Key);

                        //Set FROM format if it is not set
                        if (string.Equals(fromFormat.ToLower(), "*", StringComparison.OrdinalIgnoreCase))
                        {
                            fromFormat = upload.FileExt;
                        }

                        break;

                }
            }


            var url = new UriBuilder(ApiBaseUri)
            {
                Path = $"convert/{fromFormat}/to/{toFormat}",
                Query = $"secret={_secret}"
            };

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
