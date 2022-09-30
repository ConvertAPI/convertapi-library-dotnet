using System.Net.Http;
using ConvertApiDotNet.Constants;
using ConvertApiDotNet.Interface;

namespace ConvertApiDotNet
{
    public class DefaultConvertApiHttpClient : IConvertApiHttpClient
    {
        public HttpClient Client { get; }

        public DefaultConvertApiHttpClient()
        {
            Client = new HttpClient();
            Client.Timeout = ConvertApiConstants.HttpClientTimeOut;
        }
    }
}