using System;
using System.Net;

namespace ConvertApiDotNet.Exceptions
{
    /// <summary>
    /// The ConvertAPI service, or a download from it, answered with an error. <see cref="StatusCode"/> is the HTTP
    /// status and <see cref="Response"/> the body the service sent.
    /// </summary>
    /// <remarks>
    /// <para>401 Unauthorized: the API token is missing or invalid. The SDK also throws 401, before sending
    /// anything, when no token is set in code or in the CONVERTAPI_TOKEN environment variable.</para>
    /// <para>403 Forbidden: the account has no conversions left, or it is suspended.</para>
    /// <para>For both, the message says what to do and links to the page that helps.</para>
    /// </remarks>
    public class ConvertApiException : Exception
    {
        private const string Utm = "utm_source=dotnet-sdk&utm_medium=referral&utm_campaign=dotnet-sdk&utm_content=";
        internal const string TokenHelpUrl = "https://www.convertapi.com/a/signup?ReturnUrl=%2Fa%2Fauthentication&" + Utm + "error-401";
        internal const string PlansUrl = "https://www.convertapi.com/pricing?" + Utm + "error-403";

        /// <summary>
        /// Create the exception for an error answer.
        /// </summary>
        /// <param name="statusCode">The HTTP status of the answer.</param>
        /// <param name="message">What failed.</param>
        /// <param name="response">The body of the answer.</param>
        public ConvertApiException(HttpStatusCode statusCode, string message, string response) : base(message)
        {
            StatusCode = statusCode;
            Response = response;            
        }

        /// <summary>
        /// The HTTP status of the answer, for example 401 for a missing or invalid token and 403 when no
        /// conversions are left.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// The body of the answer, usually JSON with the service's own error message.
        /// </summary>
        public string Response { get; }

        /// <summary>
        /// The message for an error answer: <paramref name="message"/>, plus what to do for 401 and 403.
        /// </summary>
        internal static string MessageFor(HttpStatusCode statusCode, string message)
        {
            if (statusCode != HttpStatusCode.Unauthorized && statusCode != HttpStatusCode.Forbidden)
                return message;
            // "... error. Unauthorized" ends without a full stop; add one before the next sentence.
            message = message.TrimEnd();
            if (!message.EndsWith("."))
                message += ".";
            if (statusCode == HttpStatusCode.Unauthorized)
                return message + " The API token is missing or invalid. Find or get your token at " + TokenHelpUrl;
            if (statusCode == HttpStatusCode.Forbidden)
                return message + " No conversions left or the account is suspended. Plans: " + PlansUrl;
            return message;
        }
    }
}
