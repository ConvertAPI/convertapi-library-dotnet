using System.Net.Http;

namespace ConvertApiDotNet.Interface
{
    public interface IConvertApiHttpClient
    {
        HttpClient Client { get; }
    }
}
