using System;
using System.Reflection;

namespace ConvertApiDotNet.Constants
{
    public static class ConvertApiConstants
    {
        static ConvertApiConstants()
        {
            UploadTimeout = TimeSpan.FromSeconds(600);
            DownloadTimeout = TimeSpan.FromSeconds(600);
            HttpClientTimeOut = TimeSpan.FromSeconds(1800);
            ConversionTimeoutDelta = TimeSpan.FromSeconds(10);
            HttpUserAgent = $"convertapi-dotnet/{new AssemblyName(typeof(ConvertApiConstants).Assembly.FullName).Version}";
        }

        public static TimeSpan HttpClientTimeOut { get; set; }
        public static TimeSpan UploadTimeout { get; set; }
        public static TimeSpan DownloadTimeout { get; set; }
        public static TimeSpan ConversionTimeoutDelta { get; set; }
        public static string HttpUserAgent { get; set; }
    }
}
