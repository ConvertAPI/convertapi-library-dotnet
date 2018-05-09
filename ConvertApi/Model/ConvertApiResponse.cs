using System;
using Newtonsoft.Json;

namespace ConvertApi.Model
{
    public class ConvertApiResponse
    {
        public int ConversionCost { get; set; }
        [JsonProperty(PropertyName = "Files")]
        public ProcessedFile[] Files { get; set; }
    }
    
    public class ProcessedFile
    {
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public Uri Url { get; set; }
    }

}
