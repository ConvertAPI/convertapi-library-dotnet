using System.Collections.Generic;

namespace ConvertApiDotNet.Model
{
    /// <summary>
    /// Converter details
    /// </summary>
    public class ConverterDto
    {
        /// <summary>
        /// Converter title
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Short description of the converter
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Detailed description of the converter
        /// </summary>
        public string Overview { get; set; }
        
        /// <summary>
        /// Detailed description of the converter in md format
        /// </summary>
        public string OverviewMarkdown { get; set; }
        

        /// <summary>
        /// Meta title for SEO purposes
        /// </summary>
        public string MetaTitle { get; set; }

        /// <summary>
        /// Meta description for SEO purposes
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// List of tags associated with the converter
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// The endpoint source format
        /// </summary>
        public string SourceFormat { get; set; }

        /// <summary>
        /// The endpoint destination file format
        /// </summary>
        public string DestinationFormat { get; set; }

        /// <summary>
        /// List of supported source file extensions
        /// </summary>
        public List<string> SourceExtensions { get; set; }

        /// <summary>
        /// List of supported destination file extensions
        /// </summary>
        public List<string> DestinationExtensions { get; set; }

        /// <summary>
        /// Collection of converter parameter groups
        /// </summary>
        public IEnumerable<ConverterParameterGroupDto> ConverterParameterGroups { get; set; }

    }

    /// <summary>
    /// Group of converter parameters
    /// </summary>
    public class ConverterParameterGroupDto
    {
        /// <summary>
        /// Name of the parameter group
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Collection of parameters within the group
        /// </summary>
        public IEnumerable<ConverterParameterDto> ConverterParameters { get; set; }
    }

    /// <summary>
    /// Converter parameter details
    /// </summary>
    public class ConverterParameterDto
    {
        /// <summary>
        /// Technical name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display label for the parameter
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Description of what the parameter does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Name of the group this parameter belongs to
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Data type of the parameter
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Internal type representation of the property (for example Color, Collection, etc.)
        /// </summary>
        public string XType { get; set; }

        /// <summary>
        /// Representation of the parameter (e.g. text, select, checkbox)
        /// </summary>
        public string Representation { get; set; }

        /// <summary>
        /// Default value for the parameter
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// Dictionary of possible values (key-value pairs) if applicable
        /// </summary>
        public Dictionary<string, string> Values { get; set; }

        /// <summary>
        /// Range of allowed values for the parameter
        /// </summary>
        public ConverterParameterRangeDto Range { get; set; }

        /// <summary>
        /// Indicates if the parameter is mandatory
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Indicates if the parameter is featured/important
        /// </summary>
        public bool Featured { get; set; }

        /// <summary>
        /// Indicates if the parameter accepts an array of values
        /// </summary>
        public bool Array { get; set; }

        /// <summary>
        /// Array of allowed file extensions for this parameter
        /// </summary>
        public string[] AllowedExtensions { get; set; }
    }

    /// <summary>
    /// Range of allowed values
    /// </summary>
    public class ConverterParameterRangeDto
    {
        /// <summary>
        /// Start value of the range
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// End value of the range
        /// </summary>
        public string To { get; set; }
    }

    /// <summary>
    /// Tag details
    /// </summary>
    public class TagDto
    {
        /// <summary>
        /// Name of the tag
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Summary description of the tag
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Detailed description of the tag
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Title of the page associated with the tag
        /// </summary>
        public string PageTitle { get; set; }

        /// <summary>
        /// User-friendly name of the tag
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Meta title for the tag page
        /// </summary>
        public string MetaTitle { get; set; }

        /// <summary>
        /// Meta description for the tag page
        /// </summary>
        public string MetaDescription { get; set; }
        
        /// <summary>
        /// Category of the tag
        /// </summary>
        public string Category { get; set; }
    }
}
