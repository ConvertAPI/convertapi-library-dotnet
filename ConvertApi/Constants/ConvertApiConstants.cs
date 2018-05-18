namespace ConvertApi.Constants
{
    public static class ConvertApiConstants
    {
        static ConvertApiConstants()
        {
            UploadTimeoutInSeconds = 600;
            DownloadTimeoutInSeconds = 600;
        }

        public static int UploadTimeoutInSeconds { get; set; }
        public static int DownloadTimeoutInSeconds { get; set; }
    }
}
