using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Baubit.Configuration
{
    /// <summary>
    /// Configuration source descriptor for <see cref="IConfiguration"/>
    /// </summary>
    public class ConfigurationSource
    {
        public static ConfigurationSource Empty => new ConfigurationSource();
        public List<string> RawJsonStrings { get; private set; }
        [URI]
        public List<string> JsonUriStrings { get; private set; }
        [URI]
        public List<string> EmbeddedJsonResources { get; private set; }
        [URI]
        public List<string> LocalSecrets { get; private set; }

        public ConfigurationSource(List<string> rawJsonStrings,
                                   List<string> jsonUriStrings,
                                   List<string> embeddedJsonResources,
                                   List<string> localSecrets)
        {
            RawJsonStrings = rawJsonStrings ?? new List<string>();
            JsonUriStrings = jsonUriStrings ?? new List<string>();
            EmbeddedJsonResources = embeddedJsonResources ?? new List<string>();
            LocalSecrets = localSecrets ?? new List<string>();
        }
        public ConfigurationSource() : this(null, null, null, null)
        {

        }
    }
}
