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
        public static string AuthCredentials;
        public static string ApiBaseUri = "https://v2.convertapi.com";
        private static IConvertApiHttpClient _convertApiHttpClient;

        public ConvertApi()
        {
        }


        /// <summary>
        /// Initializes a new instance of the ConvertApi class.
        /// </summary>
        /// <param name="authCredentials">The authentication credentials, either a Secret or Token, for ConvertApi: https://www.convertapi.com/a</param>
        /// <param name="convertApiHttpClient">The HTTP client for making API requests.</param>
        public ConvertApi(string authCredentials, IConvertApiHttpClient convertApiHttpClient)
        {
            AuthCredentials = authCredentials;
            _convertApiHttpClient = convertApiHttpClient;
        }

        /// <summary>
        /// Initializes a new instance of the ConvertApi class.
        /// </summary>
        /// <param name="authCredentials">The authentication credentials, either a Secret or Token, for ConvertApi: https://www.convertapi.com/a</param>
        public ConvertApi(string authCredentials)
        {
            if (string.IsNullOrEmpty(authCredentials))
                throw new ArgumentNullException(nameof(authCredentials));

            AuthCredentials = authCredentials;
        }

        public static IConvertApiHttpClient GetClient()
        {
            return _convertApiHttpClient ?? (_convertApiHttpClient = new DefaultConvertApiHttpClient());
        }

        public async Task<ConvertApiResponse> ConvertAsync(string fromFormat, string toFormat, params ConvertApiBaseParam[] parameters)
        {
            return await ConvertAsync(fromFormat, toFormat, (IEnumerable<ConvertApiBaseParam>)parameters);
        }

        public async Task<ConvertApiResponse> ConvertAsync(string fromFormat, string toFormat, IEnumerable<ConvertApiBaseParam> parameters)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent("true"), "StoreFile" }                
            };

            var ignoredParameters = new[] { "StoreFile", "Async", "JobId" };


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
                else if (parameter is ConvertApiFileParam)
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
                //We give Token authentication priority if token provided and then Secret
                /*Query = !string.IsNullOrEmpty(Token) ? $"token={Token}&apikey={ApiKey}" : $"secret={AuthCredentials}"*/
            };

            TimeSpan? requestTimeOut = null;
            var timeoutParameter = dicList.Find("timeout");
            if (!string.IsNullOrEmpty(timeoutParameter) && int.TryParse(timeoutParameter, out var parsedTimeOut))
            {
                requestTimeOut = TimeSpan.FromSeconds(parsedTimeOut).Add(ConvertApiConstants.ConversionTimeoutDelta);
            }


            var response = await GetClient().PostAsync(url.Uri, requestTimeOut, content, AuthCredentials);
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
                Path = "user"
            };

            var response = await GetClient().GetAsync(url.Uri, ConvertApiConstants.DownloadTimeout, AuthCredentials);
            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new ConvertApiException(response.StatusCode, $"Retrieve user information failed. {response.ReasonPhrase}", result);
            return JsonConvert.DeserializeObject<ConvertApiUser>(result);
        }
    }
}