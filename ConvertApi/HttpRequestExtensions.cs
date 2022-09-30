using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ConvertApiDotNet.Constants;
using ConvertApiDotNet.Interface;

namespace ConvertApiDotNet
{
    public static class HttpRequestExtensions
    {
        private const string TimeoutPropertyKey = "RequestTimeout";

        public static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.Properties[TimeoutPropertyKey] = timeout;
        }

        public static TimeSpan? GetTimeout(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Properties.TryGetValue(TimeoutPropertyKey, out var value) && value is TimeSpan timeout)
                return timeout;
            return null;
        }

        public static async Task<HttpResponseMessage> PostAsync(this IConvertApiHttpClient convertApiHttpClient, Uri url, TimeSpan? timeOut, HttpContent content)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = url,
                Method = HttpMethod.Post,
                Content = content
            };
            if (timeOut != null)
                request.SetTimeout(timeOut);
            request.Headers.Add("User-Agent", ConvertApiConstants.HttpUserAgent);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await convertApiHttpClient.Client.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> GetAsync(this IConvertApiHttpClient convertApiHttpClient, Uri url, TimeSpan timeOut)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = url,
                Method = HttpMethod.Get
            };
            request.SetTimeout(timeOut);
            request.Headers.Add("User-Agent", ConvertApiConstants.HttpUserAgent);
            return await convertApiHttpClient.Client.SendAsync(request);
        }
    }
}