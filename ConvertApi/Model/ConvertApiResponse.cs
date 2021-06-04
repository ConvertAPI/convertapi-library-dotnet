using System;
using Newtonsoft.Json;

namespace ConvertApiDotNet.Model
{
    public class ConvertApiResponse
    {
        public int ConversionCost { get; set; }
        [JsonProperty(PropertyName = "Files")]
        public ConvertApiFiles[] Files { get; set; }
    }
}
