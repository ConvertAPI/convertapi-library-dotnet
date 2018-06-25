using System.Reflection;

namespace ConvertApiDotNet.Constants
{
    public static class ConvertApiConstants
    {
        static ConvertApiConstants()
        {
            UploadTimeoutInSeconds = 600;
            DownloadTimeoutInSeconds = 600;
            HttpUserAgent = $"convertapi-dotnet-{new AssemblyName(typeof(ConvertApiConstants).Assembly.FullName).Version}";
        }

        public static int UploadTimeoutInSeconds { get; set; }
        public static int DownloadTimeoutInSeconds { get; set; }
        public static string HttpUserAgent { get; set; }
    }
}
