using System.Collections.Generic;

namespace ConvertApiDotNet.Model
{
    public class ConverterDto
    {
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Overview { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public List<string> Tags { get; set; }
        public string SourceFormat { get; set; }
        public string DestinationFormat { get; set; }
        public List<string> SourceExtensions { get; set; }
        public List<string> DestinationExtensions { get; set; }
        public IEnumerable<ConverterParameterGroupDto> ConverterParameterGroups { get; set; }
    }

    public class ConverterParameterGroupDto
    {
        public string Name { get; set; }
        public IEnumerable<ConverterParameterDto> ConverterParameters { get; set; }
    }

    public class ConverterParameterDto
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }
        public string Type { get; set; }
        public string Representation { get; set; }
        public object Default { get; set; }
        public Dictionary<string, string> Values { get; set; }
        public ConverterParameterRangeDto Range { get; set; }
        public bool Required { get; set; }
        public bool Featured { get; set; }
        public bool Array { get; set; }
        public string[] AllowedExtensions { get; set; }
    }

    public class ConverterParameterRangeDto
    {
        public string From { get; set; }
        public string To { get; set; }
    }

    public class TagDto
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string PageTitle { get; set; }
        public string FriendlyName { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
    }
}
