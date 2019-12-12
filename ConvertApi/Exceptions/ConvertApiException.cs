using System;
using System.Net;

namespace ConvertApiDotNet.Exceptions
{
    public class ConvertApiException : Exception
    {
        public ConvertApiException(HttpStatusCode statusCode, string message, string response) : base(message)
        {
            StatusCode = statusCode;
            Response = response;            
        }

        public HttpStatusCode StatusCode { get; }
        public string Response { get; }

    }
}
