using System.Collections.Generic;
using System.Threading.Tasks;
using ConvertApiDotNet.Model;

namespace ConvertApiDotNet.Interface
{
    /// <summary>
    /// Provides access to ConvertAPI converters and tags by parsing the OpenAPI specification.
    /// </summary>
    public interface IConverterCatalog
    {
        /// <summary>
        /// Gets the complete list of available converters.
        /// </summary>
        List<ConverterDto> GetAllConverters();

        /// <summary>
        /// Gets a single converter definition by source and destination formats.
        /// </summary>
        ConverterDto GetConverter(string sourceFormat, string destinationFormat);

        /// <summary>
        /// Gets converters filtered by the specified tags.
        /// If tags is null or empty, returns all converters.
        /// </summary>
        List<ConverterDto> GetConvertersByTags(List<string> tags = null);

        /// <summary>
        /// Searches for converters by the provided search terms.
        /// </summary>
        List<ConverterDto> SearchConverters(string[] terms);

        /// <summary>
        /// Gets the list of available tags.
        /// </summary>
        List<TagDto> GetTags();

        /// <summary>
        /// Reloads converter and tag information from the service and updates the cache.
        /// </summary>
        Task Reload();
    }
}
