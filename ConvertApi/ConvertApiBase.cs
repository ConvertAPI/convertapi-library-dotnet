using System;
using System.Net.Http;
using ConvertApiDotNet.Constants;

namespace ConvertApiDotNet
{
    public class ConvertApiBase
    {
        public HttpClient HttpClient;

        public ConvertApiBase(int requestTimeoutInSeconds)
        {
            HttpClient = new HttpClient
            {

                Timeout = new TimeSpan(0, 0, requestTimeoutInSeconds + 10)
            };
            HttpClient.DefaultRequestHeaders.Add("User-Agent", ConvertApiConstants.HttpUserAgent);
        }
    }
}