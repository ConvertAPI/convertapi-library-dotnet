using System;
using System.Collections.Generic;

namespace ConvertApiDotNet.Model
{
    /// <summary>
    /// Strongly typed metadata for a converter endpoint, derived from OpenAPI.
    /// </summary>
    [Obsolete("Use ConverterDto instead")]
    public class Converter
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        /// <summary>
        /// Comma-separated list for HTML input accept attribute, e.g. ".docx,.doc".
        /// </summary>
        public string AcceptsFormats { get; set; }
        /// <summary>
        /// Whether the converter accepts multiple files (files[] with binary items).
        /// </summary>
        public bool AcceptsMultiple { get; set; }
        /// <summary>
        /// Additional parameters (name => label or caption) derived from OpenAPI schema and extensions.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
