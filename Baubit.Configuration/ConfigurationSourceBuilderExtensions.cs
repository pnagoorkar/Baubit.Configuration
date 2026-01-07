using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Baubit.Configuration
{
    /// <summary>
    /// Provides extension methods for <see cref="Result{T}"/> of <see cref="ConfigurationSourceBuilder"/>.
    /// These extensions enable fluent, error-propagating configuration source setup using the Result pattern.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class extends the Result pattern to configuration source builders, allowing method chaining while preserving error state.
    /// All methods automatically propagate failures from the Result monad without requiring explicit error handling at each step.
    /// </para>
    /// <para>
    /// These extensions are particularly useful when building configuration source pipelines that may fail at any step,
    /// as they eliminate the need for explicit <c>Bind</c> calls while maintaining the Result pattern benefits.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Without extensions (verbose):
    /// var result = ConfigurationSourceBuilder.CreateNew()
    ///     .Bind(b => b.WithRawJsonStrings("{\"Key\":\"Value\"}"))
    ///     .Bind(b => b.Build());
    /// 
    /// // With extensions (concise):
    /// var result = ConfigurationSourceBuilder.CreateNew()
    ///     .WithRawJsonStrings("{\"Key\":\"Value\"}")
    ///     .Build();
    /// </code>
    /// </example>
    public static class ConfigurationSourceBuilderExtensions
    {
        /// <summary>
        /// Adds one or more JSON URI strings to the configuration source, propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationSourceBuilder"/> instance.</param>
        /// <param name="jsonUriStrings">An array of URI strings pointing to JSON configuration files.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the builder instance for method chaining if successful;
        /// otherwise, a failed result with error information from either the input Result or the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationSourceBuilder.WithJsonUriStrings"/>.
        /// </para>
        /// <para>
        /// URIs can be file paths (file:///path/to/config.json) or remote URLs (https://example.com/config.json).
        /// This method can be called multiple times to accumulate multiple JSON URIs.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = ConfigurationSourceBuilder.CreateNew()
        ///     .WithJsonUriStrings(
        ///         "file:///app/config.json",
        ///         "https://config-server.com/app-config.json")
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationSourceBuilder> WithJsonUriStrings(this Result<ConfigurationSourceBuilder> result, params string[] jsonUriStrings)
        {
            return result.Bind(csb => csb.WithJsonUriStrings(jsonUriStrings));
        }

        /// <summary>
        /// Adds one or more embedded JSON resource names to the configuration source, propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationSourceBuilder"/> instance.</param>
        /// <param name="embeddedJsonResources">
        /// An array of fully qualified embedded resource names in the format: "AssemblyName;Resource.Path.FileName.json"
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the builder instance for method chaining if successful;
        /// otherwise, a failed result with error information from either the input Result or the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationSourceBuilder.WithEmbeddedJsonResources"/>.
        /// </para>
        /// <para>
        /// Resource names should follow the format: "AssemblyName;Namespace.Folder.FileName.json"
        /// where the semicolon separates the assembly name from the resource path.
        /// This method can be called multiple times to accumulate multiple embedded resources.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = ConfigurationSourceBuilder.CreateNew()
        ///     .WithEmbeddedJsonResources(
        ///         "MyApp;Config.appsettings.json",
        ///         "MyApp;Config.appsettings.Production.json")
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationSourceBuilder> WithEmbeddedJsonResources(this Result<ConfigurationSourceBuilder> result, params string[] embeddedJsonResources)
        {
            return result.Bind(csb => csb.WithEmbeddedJsonResources(embeddedJsonResources));
        }

        /// <summary>
        /// Adds one or more local secret identifiers to the configuration source, propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationSourceBuilder"/> instance.</param>
        /// <param name="localSecrets">
        /// An array of user secret identifiers (typically project names or GUIDs).
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the builder instance for method chaining if successful;
        /// otherwise, a failed result with error information from either the input Result or the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationSourceBuilder.WithLocalSecrets"/>.
        /// </para>
        /// <para>
        /// Local secrets are typically used during development and are stored in the user's profile directory.
        /// They provide a secure way to store sensitive configuration data outside of source control.
        /// This method can be called multiple times to accumulate multiple secret identifiers.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = ConfigurationSourceBuilder.CreateNew()
        ///     .WithLocalSecrets(
        ///         "MyApp-Secrets",
        ///         "a3c8f7e2-9b4d-4c6e-8f1a-2d3e4f5a6b7c")
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationSourceBuilder> WithLocalSecrets(this Result<ConfigurationSourceBuilder> result, params string[] localSecrets)
        {
            return result.Bind(csb => csb.WithLocalSecrets(localSecrets));
        }

        /// <summary>
        /// Adds one or more raw JSON strings to the configuration source, propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationSourceBuilder"/> instance.</param>
        /// <param name="rawJsonStrings">
        /// An array of strings containing valid JSON configuration data.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the builder instance for method chaining if successful;
        /// otherwise, a failed result with error information from either the input Result or the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationSourceBuilder.WithRawJsonStrings"/>.
        /// </para>
        /// <para>
        /// The JSON strings should be well-formed and contain valid configuration data.
        /// This is useful for programmatically generated configurations or inline configuration during testing.
        /// This method can be called multiple times to accumulate multiple JSON strings.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = ConfigurationSourceBuilder.CreateNew()
        ///     .WithRawJsonStrings(
        ///         "{\"ConnectionString\":\"Server=localhost;Database=MyDb\"}",
        ///         "{\"Logging\":{\"LogLevel\":{\"Default\":\"Information\"}}}")
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationSourceBuilder> WithRawJsonStrings(this Result<ConfigurationSourceBuilder> result, params string[] rawJsonStrings)
        {
            return result.Bind(csb => csb.WithRawJsonStrings(rawJsonStrings));
        }

        /// <summary>
        /// Adds additional configuration sources by merging them with the current builder, propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationSourceBuilder"/> instance.</param>
        /// <param name="configSources">
        /// An array of <see cref="ConfigurationSource"/> instances whose sources will be merged with the current builder.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the builder instance for method chaining if successful;
        /// otherwise, a failed result with error information from either the input Result or the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationSourceBuilder.WithAdditionalConfigurationSources"/>.
        /// </para>
        /// <para>
        /// This method extracts all source types (RawJsonStrings, JsonUriStrings, EmbeddedJsonResources, LocalSecrets)
        /// from the provided configuration sources and adds them to the current builder.
        /// Sources are accumulated in the order they are provided.
        /// This is useful for composing configuration sources from multiple existing sources.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var existingSource = ConfigurationSourceBuilder.CreateNew()
        ///     .WithRawJsonStrings("{\"Key\":\"Value\"}")
        ///     .Build()
        ///     .Value;
        /// 
        /// var result = ConfigurationSourceBuilder.CreateNew()
        ///     .WithAdditionalConfigurationSources(existingSource)
        ///     .WithRawJsonStrings("{\"AnotherKey\":\"AnotherValue\"}")
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationSourceBuilder> WithAdditionalConfigurationSources(this Result<ConfigurationSourceBuilder> result, params ConfigurationSource[] configSources)
        {
            return result.Bind(csb => csb.WithAdditionalConfigurationSources(configSources));
        }

        /// <summary>
        /// Adds additional configuration sources extracted from existing <see cref="IConfiguration"/> instances,
        /// propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationSourceBuilder"/> instance.</param>
        /// <param name="configurations">
        /// An array of <see cref="IConfiguration"/> instances containing "configurationSource" sections to extract.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the builder instance for method chaining if successful;
        /// otherwise, a failed result with error information from either the input Result or the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationSourceBuilder.WithAdditionalConfigurationSourcesFrom"/>.
        /// </para>
        /// <para>
        /// This method extracts the "configurationSource" section from each provided configuration
        /// and binds it to a <see cref="ConfigurationSource"/> object.
        /// If the section does not exist or cannot be bound, an empty configuration source is used.
        /// The extracted sources are then merged using <see cref="ConfigurationSourceBuilder.WithAdditionalConfigurationSources"/>.
        /// This is useful for loading configuration sources from external configurations or configuration files.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var externalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
        ///     .AddInMemoryCollection(new Dictionary&lt;string, string&gt; 
        ///     {
        ///         { "configurationSource:RawJsonStrings:0", "{\"Key\":\"Value\"}" }
        ///     })
        ///     .Build();
        /// 
        /// var result = ConfigurationSourceBuilder.CreateNew()
        ///     .WithAdditionalConfigurationSourcesFrom(externalConfig)
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationSourceBuilder> WithAdditionalConfigurationSourcesFrom(this Result<ConfigurationSourceBuilder> result, params IConfiguration[] configurations)
        {
            return result.Bind(csb => csb.WithAdditionalConfigurationSourcesFrom(configurations));
        }

        /// <summary>
        /// Builds and returns a <see cref="ConfigurationSource"/> from the Result-wrapped builder,
        /// propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationSourceBuilder"/> instance.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the constructed <see cref="ConfigurationSource"/> if successful;
        /// otherwise, a failed result with error information from either the input Result or the build operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationSourceBuilder.Build"/>.
        /// </para>
        /// <para>
        /// After calling this method, the builder is automatically disposed and cannot be reused.
        /// The returned <see cref="ConfigurationSource"/> contains independent copies of all configuration data.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = ConfigurationSourceBuilder.CreateNew()
        ///     .WithRawJsonStrings("{\"Key\":\"Value\"}")
        ///     .WithJsonUriStrings("file:///config.json")
        ///     .Build();
        /// 
        /// if (result.IsSuccess)
        /// {
        ///     var configSource = result.Value;
        ///     // Use the configuration source...
        /// }
        /// </code>
        /// </example>
        public static Result<ConfigurationSource> Build(this Result<ConfigurationSourceBuilder> result)
        {
            return result.Bind(csb => csb.Build());
        }
    }
}
