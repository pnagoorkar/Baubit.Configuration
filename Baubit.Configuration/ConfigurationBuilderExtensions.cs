using Baubit.Validation;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Baubit.Configuration
{
    /// <summary>
    /// Provides extension methods for <see cref="Result{T}"/> of <see cref="ConfigurationBuilder"/> and <see cref="ConfigurationBuilder{TConfiguration}"/>.
    /// These extensions enable fluent, error-propagating configuration setup using the Result pattern.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class extends the Result pattern to configuration builders, allowing method chaining while preserving error state.
    /// All methods automatically propagate failures from the Result monad without requiring explicit error handling at each step.
    /// </para>
    /// <para>
    /// These extensions are particularly useful when building configuration pipelines that may fail at any step,
    /// as they eliminate the need for explicit <c>Bind</c> calls while maintaining the Result pattern benefits.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Without extensions (verbose):
    /// var result = ConfigurationBuilder.CreateNew()
    ///     .Bind(b => b.WithRawJsonStrings("{\"Key\":\"Value\"}"))
    ///     .Bind(b => b.Build());
    /// 
    /// // With extensions (concise):
    /// var result = ConfigurationBuilder.CreateNew()
    ///     .WithRawJsonStrings("{\"Key\":\"Value\"}")
    ///     .Build();
    /// </code>
    /// </example>
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds one or more JSON URI strings to the configuration sources, propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationBuilder"/> instance.</param>
        /// <param name="jsonUriStrings">An array of URI strings pointing to JSON configuration files.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the builder instance for method chaining if successful;
        /// otherwise, a failed result with error information from either the input Result or the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationBuilder.WithJsonUriStrings"/>.
        /// </para>
        /// <para>
        /// URIs can be file paths (file:///path/to/config.json) or remote URLs (https://example.com/config.json).
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = ConfigurationBuilder.CreateNew()
        ///     .WithJsonUriStrings("file:///app/config.json", "https://config.example.com/app.json")
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationBuilder> WithJsonUriStrings(this Result<ConfigurationBuilder> result, params string[] jsonUriStrings)
        {
            return result.Bind(cb => cb.WithJsonUriStrings(jsonUriStrings));
        }

        /// <summary>
        /// Adds one or more embedded JSON resource names to the configuration sources, propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationBuilder"/> instance.</param>
        /// <param name="embeddedJsonResources">
        /// An array of embedded resource identifiers in the format: "AssemblyName;Resource.Path.FileName.json"
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the builder instance for method chaining if successful;
        /// otherwise, a failed result with error information from either the input Result or the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationBuilder.WithEmbeddedJsonResources"/>
        /// </para>
        /// <para>
        /// Resource identifiers should follow the format: "AssemblyName;Namespace.Folder.FileName.json"
        /// where the semicolon separates the assembly name from the resource path.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = ConfigurationBuilder.CreateNew()
        ///     .WithEmbeddedJsonResources(
        ///         "MyApp;Config.appsettings.json",
        ///         "MyApp;Config.appsettings.Production.json")
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationBuilder> WithEmbeddedJsonResources(this Result<ConfigurationBuilder> result, params string[] embeddedJsonResources)
        {
            return result.Bind(cb => cb.WithEmbeddedJsonResources(embeddedJsonResources));
        }

        /// <summary>
        /// Adds one or more local secret identifiers to the configuration sources, propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationBuilder"/> instance.</param>
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
        /// <see cref="ConfigurationBuilder.WithLocalSecrets"/>.
        /// </para>
        /// <para>
        /// Local secrets are typically used during development and are stored in the user's profile directory.
        /// They provide a secure way to store sensitive configuration data outside of source control.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = ConfigurationBuilder.CreateNew()
        ///     .WithLocalSecrets("MyApp-Secrets", "a3c8f7e2-9b4d-4c6e-8f1a-2d3e4f5a6b7c")
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationBuilder> WithLocalSecrets(this Result<ConfigurationBuilder> result, params string[] localSecrets)
        {
            return result.Bind(cb => cb.WithLocalSecrets(localSecrets));
        }

        /// <summary>
        /// Adds one or more raw JSON strings to the configuration sources, propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationBuilder"/> instance.</param>
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
        /// <see cref="ConfigurationBuilder.WithRawJsonStrings"/>.
        /// </para>
        /// <para>
        /// The JSON strings should be well-formed and contain valid configuration data.
        /// This is useful for programmatically generated configurations or inline configuration during testing.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = ConfigurationBuilder.CreateNew()
        ///     .WithRawJsonStrings(
        ///         "{\"ConnectionString\":\"Server=localhost;Database=MyDb\"}",
        ///         "{\"Logging\":{\"LogLevel\":{\"Default\":\"Information\"}}}")
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationBuilder> WithRawJsonStrings(this Result<ConfigurationBuilder> result, params string[] rawJsonStrings)
        {
            return result.Bind(cb => cb.WithRawJsonStrings(rawJsonStrings));
        }

        /// <summary>
        /// Adds one or more pre-built <see cref="IConfiguration"/> instances to be merged with the final configuration,
        /// propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationBuilder"/> instance.</param>
        /// <param name="configurations">
        /// An array of <see cref="IConfiguration"/> instances to be merged into the final configuration.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the builder instance for method chaining if successful;
        /// otherwise, a failed result with error information from either the input Result or the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationBuilder.WithAdditionalConfigurations"/>.
        /// </para>
        /// <para>
        /// Additional configurations are useful for merging configurations from external sources,
        /// overlaying environment-specific settings, or integrating with other configuration systems.
        /// Later configurations override earlier ones in case of key conflicts.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var externalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
        ///     .AddInMemoryCollection(new Dictionary&lt;string, string&gt; 
        ///     {
        ///         { "Feature:Enabled", "true" }
        ///     })
        ///     .Build();
        /// 
        /// var result = ConfigurationBuilder.CreateNew()
        ///     .WithAdditionalConfigurations(externalConfig)
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationBuilder> WithAdditionalConfigurations(this Result<ConfigurationBuilder> result, params IConfiguration[] configurations)
        {
            return result.Bind(cb => cb.WithAdditionalConfigurations(configurations));
        }

        /// <summary>
        /// Adds additional configuration sources extracted from existing <see cref="IConfiguration"/> instances,
        /// propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationBuilder"/> instance.</param>
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
        /// <see cref="ConfigurationBuilder.WithAdditionalConfigurationSourcesFrom"/>.
        /// </para>
        /// <para>
        /// This method extracts the "configurationSource" section from each provided configuration.
        /// If the section does not exist or is not defined, it returns an empty configuration source.
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
        /// var result = ConfigurationBuilder.CreateNew()
        ///     .WithAdditionalConfigurationSourcesFrom(externalConfig)
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationBuilder> WithAdditionalConfigurationSourcesFrom(this Result<ConfigurationBuilder> result, params IConfiguration[] configurations)
        {
            return result.Bind(cb => cb.WithAdditionalConfigurationSourcesFrom(configurations));
        }

        /// <summary>
        /// Adds pre-built <see cref="ConfigurationSource"/> instances to the builder by merging their sources,
        /// propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationBuilder"/> instance.</param>
        /// <param name="configurationSources">
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
        /// <see cref="ConfigurationBuilder.WithAdditionalConfigurationSources"/>.
        /// </para>
        /// <para>
        /// This is useful when you have existing <see cref="ConfigurationSource"/> objects that you want to
        /// compose together or reuse across multiple configuration builds.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var baseSource = ConfigurationSourceBuilder.CreateNew()
        ///     .WithRawJsonStrings("{\"BaseKey\":\"BaseValue\"}")
        ///     .Build()
        ///     .Value;
        /// 
        /// var result = ConfigurationBuilder.CreateNew()
        ///     .WithAdditionalConfigurationSources(baseSource)
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationBuilder> WithAdditionalConfigurationSources(this Result<ConfigurationBuilder> result, params ConfigurationSource[] configurationSources)
        {
            return result.Bind(cb => cb.WithAdditionalConfigurationSources(configurationSources));
        }

        /// <summary>
        /// Adds additional configurations extracted from existing <see cref="IConfiguration"/> instances,
        /// propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationBuilder"/> instance.</param>
        /// <param name="configurations">
        /// An array of <see cref="IConfiguration"/> instances containing "configuration" sections to extract.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the builder instance for method chaining if successful;
        /// otherwise, a failed result with error information from either the input Result or the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationBuilder.WithAdditionalConfigurationsFrom"/>.
        /// </para>
        /// <para>
        /// This method extracts the "configuration" section from each provided configuration.
        /// If the "configuration" section does not exist in a configuration, that configuration is skipped with a failure reason.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var externalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
        ///     .AddInMemoryCollection(new Dictionary&lt;string, string&gt; 
        ///     {
        ///         { "configuration:Database", "Server=localhost" }
        ///     })
        ///     .Build();
        /// 
        /// var result = ConfigurationBuilder.CreateNew()
        ///     .WithAdditionalConfigurationsFrom(externalConfig)
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationBuilder> WithAdditionalConfigurationsFrom(this Result<ConfigurationBuilder> result, params IConfiguration[] configurations)
        {
            return result.Bind(cb => cb.WithAdditionalConfigurationsFrom(configurations));
        }

        /// <summary>
        /// Builds and returns an <see cref="IConfiguration"/> instance from the Result-wrapped builder,
        /// propagating any existing Result failure.
        /// </summary>
        /// <param name="result">The Result containing the <see cref="ConfigurationBuilder"/> instance.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the constructed <see cref="IConfiguration"/> if successful;
        /// otherwise, a failed result with error information from either the input Result or the build operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationBuilder.Build"/>.
        /// </para>
        /// <para>
        /// After calling this method, the builder is automatically disposed and cannot be reused.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = ConfigurationBuilder.CreateNew()
        ///     .WithRawJsonStrings("{\"Key\":\"Value\"}")
        ///     .Build();
        /// 
        /// if (result.IsSuccess)
        /// {
        ///     var config = result.Value;
        ///     Console.WriteLine(config["Key"]); // Outputs: Value
        /// }
        /// </code>
        /// </example>
        public static Result<IConfiguration> Build(this Result<ConfigurationBuilder> result)
        {
            return result.Bind(cb => cb.Build());
        }

        /// <summary>
        /// Adds one or more validators to the validation pipeline for the strongly-typed configuration,
        /// propagating any existing Result failure.
        /// </summary>
        /// <typeparam name="TConfiguration">
        /// The strongly-typed configuration class that inherits from <see cref="Configuration"/>.
        /// </typeparam>
        /// <param name="result">The Result containing the <see cref="ConfigurationBuilder{TConfiguration}"/> instance.</param>
        /// <param name="validators">
        /// An array of <see cref="IValidator{T}"/> instances to be executed during configuration validation.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the builder instance for method chaining if successful;
        /// otherwise, a failed result with error information from either the input Result or the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationBuilder{TConfiguration}.WithValidators"/>.
        /// </para>
        /// <para>
        /// Validators are executed sequentially after the configuration is bound and URIs are expanded.
        /// If any validator fails, the build process stops and returns a failed result.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyConfigValidator : IValidator&lt;MyConfig&gt;
        /// {
        ///     public Result Run(MyConfig config)
        ///     {
        ///         return Result.OkIf(config.Port > 0, "Port must be positive");
        ///     }
        /// }
        /// 
        /// var result = ConfigurationBuilder&lt;MyConfig&gt;.CreateNew()
        ///     .WithRawJsonStrings("{\"Port\":8080}")
        ///     .WithValidators(new MyConfigValidator())
        ///     .Build();
        /// </code>
        /// </example>
        public static Result<ConfigurationBuilder<TConfiguration>> WithValidators<TConfiguration>(this Result<ConfigurationBuilder<TConfiguration>> result, params IValidator<TConfiguration>[] validators) where TConfiguration : Configuration
        {
            return result.Bind(cb => cb.WithValidators(validators));
        }

        /// <summary>
        /// Builds and returns a strongly-typed configuration instance from the Result-wrapped builder,
        /// propagating any existing Result failure.
        /// </summary>
        /// <typeparam name="TConfiguration">
        /// The strongly-typed configuration class that inherits from <see cref="Configuration"/>.
        /// </typeparam>
        /// <param name="result">The Result containing the <see cref="ConfigurationBuilder{TConfiguration}"/> instance.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the constructed and validated <typeparamref name="TConfiguration"/> instance if successful;
        /// otherwise, a failed result with error information from either the input Result, binding, URI expansion, or validation failures.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This extension method automatically propagates failures from the input Result without executing
        /// the underlying operation. If the input Result is successful, it delegates to
        /// <see cref="ConfigurationBuilder{TConfiguration}.Build"/>.
        /// </para>
        /// <para>
        /// The build process executes binding, URI expansion, and validation in sequence.
        /// After calling this method, the builder is automatically disposed and cannot be reused.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class AppConfig : Configuration
        /// {
        ///     public string DatabaseConnection { get; set; }
        ///     public int Port { get; set; }
        /// }
        /// 
        /// var result = ConfigurationBuilder&lt;AppConfig&gt;.CreateNew()
        ///     .WithRawJsonStrings("{\"DatabaseConnection\":\"Server=localhost\",\"Port\":8080}")
        ///     .Build();
        /// 
        /// if (result.IsSuccess)
        /// {
        ///     var config = result.Value;
        ///     Console.WriteLine(config.Port); // Outputs: 8080
        /// }
        /// </code>
        /// </example>
        public static Result<TConfiguration> Build<TConfiguration>(this Result<ConfigurationBuilder<TConfiguration>> result) where TConfiguration : Configuration
        {
            return result.Bind(cb => cb.Build());
        }
    }
}
