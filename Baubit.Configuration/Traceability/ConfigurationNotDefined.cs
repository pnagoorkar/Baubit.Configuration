using Baubit.Traceability.Reasons;

namespace Baubit.Configuration.Traceability
{
    /// <summary>
    /// Represents a reason indicating that the "configuration" section is not defined in the provided configuration.
    /// This reason is used when attempting to extract a configuration section that does not exist.
    /// </summary>
    /// <remarks>
    /// This reason is typically returned when using methods like <see cref="ConfigurationBuilder.WithAdditionaConfigurationsFrom"/>
    /// and the provided configuration does not contain a "configuration" section.
    /// </remarks>
    public class ConfigurationNotDefined : AReason
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationNotDefined"/> class.
        /// </summary>
        public ConfigurationNotDefined() : base("Configuration not defined !", default)
        {
        }
    }
}
