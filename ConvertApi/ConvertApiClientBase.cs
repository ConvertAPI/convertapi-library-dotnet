using System;
using System.Net.Http;
using ConvertApi.Constants;

namespace ConvertApi
{
    public class ConvertApiClientBase
    {
        public HttpClient HttpClient;

        public ConvertApiClientBase(int requestTimeoutInSeconds)
        {
            HttpClient = new HttpClient
            {

                Timeout = new TimeSpan(0, 0, requestTimeoutInSeconds + 10)
            };
            HttpClient.DefaultRequestHeaders.Add("User-Agent", ConvertApiConstants.HttpUserAgent);
        }
    }
}