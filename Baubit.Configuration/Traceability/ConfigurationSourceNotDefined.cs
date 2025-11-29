using Baubit.Traceability.Reasons;

namespace Baubit.Configuration.Traceability
{
    /// <summary>
    /// Represents a reason indicating that the "configurationSource" section is not defined in the provided configuration.
    /// This reason is used when attempting to extract a configuration source section that does not exist.
    /// </summary>
    /// <remarks>
    /// This reason is typically returned when using methods like <see cref="ConfigurationSourceBuilder.WithAdditionalConfigurationSourcesFrom"/>
    /// and the provided configuration does not contain a "configurationSource" section.
    /// </remarks>
    public class ConfigurationSourceNotDefined : AReason
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSourceNotDefined"/> class.
        /// </summary>
        public ConfigurationSourceNotDefined() : base("Configuration source not defined !", default)
        {
        }
    }
}
