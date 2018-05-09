using System;
using System.Net.Http;

namespace ConvertApi
{
    public class ConvertApiClientBase
    {
        protected HttpClient Client;

        public ConvertApiClientBase(int requestTimeoutInSeconds)
        {
            Client = new HttpClient
            {
                Timeout = new TimeSpan(0, 0, requestTimeoutInSeconds + 10)
            };
        }
    }
}