using System;
using System.Net;
using System.Net.Http;

namespace ConvertApiDotNet.Exceptions
{
    public class ConvertApiException : Exception
    {
        public ConvertApiException(HttpStatusCode statusCode, string message, string response) : base(message)
        {
            StatusCode = statusCode;
            Response = response;            
        }

        public ConvertApiException(string message, HttpResponseMessage responseMessage) : base($"{message} {responseMessage.ReasonPhrase}")
        {
            StatusCode = responseMessage.StatusCode;
            Response = responseMessage.Content.ReadAsStringAsync().Result;            
        }

        public HttpStatusCode StatusCode { get; }
        public string Response { get; }

    }
}
