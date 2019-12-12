using System.Net.Http;
using ConvertApiDotNet.Interface;

namespace ConvertApiDotNet
{
    public class DefaultConvertApiHttpClient : IConvertApiHttpClient
    {
        public HttpClient Client { get; }

        public DefaultConvertApiHttpClient()
        {
            Client = new HttpClient();
        }
    }
}