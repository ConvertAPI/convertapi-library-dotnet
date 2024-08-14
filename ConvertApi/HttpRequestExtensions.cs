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

        private static void PrepareHttpRequest(this HttpRequestMessage request, TimeSpan? timeOut, string userAgent, string bearerToken = null)
        {
            if (timeOut != null)
                request.SetTimeout(timeOut);
            request.Headers.Add("User-Agent", userAgent);
            if (bearerToken != null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        public static async Task<HttpResponseMessage> PostAsync(this IConvertApiHttpClient convertApiHttpClient, Uri url, TimeSpan? timeOut, HttpContent content, string bearerToken)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = url,
                Method = HttpMethod.Post,
                Content = content
            };
            request.PrepareHttpRequest(timeOut, ConvertApiConstants.HttpUserAgent, bearerToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await convertApiHttpClient.Client.SendAsync(request);
        }
        public static async Task<HttpResponseMessage> GetAsync(this IConvertApiHttpClient convertApiHttpClient, Uri url, TimeSpan timeOut, string bearerToken = null)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = url,
                Method = HttpMethod.Get
            };
        
            request.PrepareHttpRequest(timeOut, ConvertApiConstants.HttpUserAgent, bearerToken);
            return await convertApiHttpClient.Client.SendAsync(request);
        }
    }
}