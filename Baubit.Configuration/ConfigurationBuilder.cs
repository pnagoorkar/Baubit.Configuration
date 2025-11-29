using Baubit.Configuration.Traceability;
using Baubit.Traceability;
using Baubit.Validation;
using FluentResults;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Baubit.Configuration
{
    /// <summary>
    /// Provides a fluent builder for constructing <see cref="IConfiguration"/> instances from multiple configuration sources.
    /// This class orchestrates the creation of configuration by delegating to <see cref="ConfigurationSourceBuilder"/> and supporting additional configurations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The builder follows the builder pattern and implements <see cref="IDisposable"/> to ensure proper resource cleanup.
    /// It automatically disposes after calling <see cref="Build"/>, enforcing single-use semantics.
    /// </para>
    /// <para>
    /// This class serves as the base for <see cref="ConfigurationBuilder{TConfiguration}"/> which adds type-safe configuration binding and validation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = ConfigurationBuilder.CreateNew()
    ///     .Bind(b => b.WithRawJsonStrings("{\"Key\":\"Value\"}"))
    ///     .Bind(b => b.WithJsonUriStrings("file:///path/to/config.json"))
    ///     .Bind(b => b.Build());
    /// 
    /// if (result.IsSuccess)
    /// {
    ///     var config = result.Value;
    ///     var value = config["Key"];
    /// }
    /// </code>
    /// </example>
    public class ConfigurationBuilder : IDisposable
    {
        /// <summary>
        /// The configuration section key used to identify configuration data within an <see cref="IConfiguration"/> instance.
        /// This constant is used when extracting configuration sections via <see cref="WithAdditionalConfigurationsFrom"/>.
        /// </summary>
        /// <remarks>
        /// When loading configurations from external sources, this key identifies the section containing
        /// the actual configuration data to be merged. The value is "configuration".
        /// </remarks>
        public const string ConfigurationSectionKey = "configuration";
        private ConfigurationSourceBuilder _configurationSourceBuilder;
        private List<IConfiguration> additionalConfigs = new List<IConfiguration>();
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationBuilder"/> class.
        /// This constructor is protected to allow inheritance while enforcing the use of factory methods for direct instantiation.
        /// </summary>
        protected ConfigurationBuilder()
        {
            _configurationSourceBuilder = ConfigurationSourceBuilder.CreateNew().Value;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationBuilder"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a new <see cref="ConfigurationBuilder"/> instance.
        /// </returns>
        /// <example>
        /// <code>
        /// var builderResult = ConfigurationBuilder.CreateNew();
        /// if (builderResult.IsSuccess)
        /// {
        ///     var builder = builderResult.Value;
        ///     // Use the builder...
        /// }
        /// </code>
        /// </example>
        public static Result<ConfigurationBuilder> CreateNew() => Result.Ok(new ConfigurationBuilder());

        /// <summary>
        /// Adds one or more JSON URI strings to the configuration sources.
        /// These URIs will be used to load JSON configuration from remote or local file locations.
        /// </summary>
        /// <param name="jsonUriStrings">An array of URI strings pointing to JSON configuration files.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result with <see cref="ConfigurationBuilderDisposed"/> if the builder has been disposed.
        /// </returns>
        /// <remarks>
        /// This method can be called multiple times to accumulate multiple JSON URIs.
        /// URIs can be file paths (file:///path/to/config.json) or remote URLs (https://example.com/config.json).
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = builder.WithJsonUriStrings(
        ///     "file:///app/config.json",
        ///     "https://config-server.com/app-config.json"
        /// );
        /// </code>
        /// </example>
        public Result<ConfigurationBuilder> WithJsonUriStrings(params string[] jsonUriStrings)
        {
            return FailIfDisposed().Bind(() => _configurationSourceBuilder.WithJsonUriStrings(jsonUriStrings))
                                   .Bind(_ => Result.Ok(this));
        }

        /// <summary>
        /// Adds one or more embedded JSON resource names to the configuration sources.
        /// These resources should be embedded in the assembly as manifest resources.
        /// </summary>
        /// <param name="embeddedJsonResources">
        /// An array of embedded resource identifiers in the format: "AssemblyName;Resource.Path.FileName.json"
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result with <see cref="ConfigurationBuilderDisposed"/> if the builder has been disposed.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method can be called multiple times to accumulate multiple embedded resources.
        /// </para>
        /// <para>
        /// Resource identifiers should follow the format: "AssemblyName;Namespace.Folder.FileName.json"
        /// where the semicolon separates the assembly name from the resource path.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = builder.WithEmbeddedJsonResources(
        ///     "MyApp;Config.appsettings.json",
        ///     "MyApp;Config.appsettings.Production.json"
        /// );
        /// </code>
        /// </example>
        public Result<ConfigurationBuilder> WithEmbeddedJsonResources(params string[] embeddedJsonResources)
        {
            return FailIfDisposed().Bind(() => _configurationSourceBuilder.WithEmbeddedJsonResources(embeddedJsonResources))
                                   .Bind(_ => Result.Ok(this));
        }

        /// <summary>
        /// Adds one or more local secret identifiers to the configuration sources.
        /// These identifiers will be used to load user secrets from the local development environment.
        /// </summary>
        /// <param name="localSecrets">
        /// An array of user secret identifiers (typically project names or GUIDs).
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result with <see cref="ConfigurationBuilderDisposed"/> if the builder has been disposed.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method can be called multiple times to accumulate multiple secret identifiers.
        /// </para>
        /// <para>
        /// Local secrets are typically used during development and are stored in the user's profile directory.
        /// They provide a secure way to store sensitive configuration data outside of source control.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = builder.WithLocalSecrets(
        ///     "MyApp-12345",
        ///     "a3c8f7e2-9b4d-4c6e-8f1a-2d3e4f5a6b7c"
        /// );
        /// </code>
        /// </example>
        public Result<ConfigurationBuilder> WithLocalSecrets(params string[] localSecrets)
        {
            return FailIfDisposed().Bind(() => _configurationSourceBuilder.WithLocalSecrets(localSecrets))
                                   .Bind(_ => Result.Ok(this));
        }

        /// <summary>
        /// Adds one or more raw JSON strings to the configuration sources.
        /// These strings contain JSON configuration data directly in string format.
        /// </summary>
        /// <param name="rawJsonStrings">
        /// An array of strings containing valid JSON configuration data.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result with <see cref="ConfigurationBuilderDisposed"/> if the builder has been disposed.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method can be called multiple times to accumulate multiple JSON strings.
        /// </para>
        /// <para>
        /// The JSON strings should be well-formed and contain valid configuration data.
        /// This is useful for programmatically generated configurations or inline configuration during testing.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = builder.WithRawJsonStrings(
        ///     "{\"ConnectionString\":\"Server=localhost;Database=MyDb\"}",
        ///     "{\"Logging\":{\"LogLevel\":{\"Default\":\"Information\"}}}"
        /// );
        /// </code>
        /// </example>
        public Result<ConfigurationBuilder> WithRawJsonStrings(params string[] rawJsonStrings)
        {
            return FailIfDisposed().Bind(() => _configurationSourceBuilder.WithRawJsonStrings(rawJsonStrings))
                                   .Bind(_ => Result.Ok(this));
        }

        /// <summary>
        /// Adds one or more pre-built <see cref="IConfiguration"/> instances to be merged with the final configuration.
        /// These configurations will be added after all other sources have been processed.
        /// </summary>
        /// <param name="configurations">
        /// An array of <see cref="IConfiguration"/> instances to be merged into the final configuration.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method can be called multiple times to accumulate multiple additional configurations.
        /// </para>
        /// <para>
        /// Additional configurations are useful for:
        /// - Merging configurations from external sources
        /// - Overlaying environment-specific settings
        /// - Integrating with other configuration systems
        /// </para>
        /// <para>
        /// Later configurations override earlier ones in case of key conflicts.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var externalConfig = new ConfigurationBuilder()
        ///     .AddInMemoryCollection(new Dictionary&lt;string, string&gt; 
        ///     {
        ///         { "Feature:Enabled", "true" }
        ///     })
        ///     .Build();
        /// 
        /// var result = builder.WithAdditionalConfigurations(externalConfig);
        /// </code>
        /// </example>
        public Result<ConfigurationBuilder> WithAdditionalConfigurations(params IConfiguration[] configurations)
        {
            return Result.Try(() => additionalConfigs.AddRange(configurations))
                         .Bind(() => Result.Ok(this));
        }

        /// <summary>
        /// Adds additional configuration sources extracted from existing <see cref="IConfiguration"/> instances.
        /// Extracts the "configurationSource" section from each provided configuration and merges them.
        /// </summary>
        /// <param name="configurations">
        /// An array of <see cref="IConfiguration"/> instances containing "configurationSource" sections to extract.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result if extraction or merging fails.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method extracts the "configurationSource" section from each provided configuration.
        /// If the section does not exist or is not defined, it returns an empty configuration source.
        /// </para>
        /// <para>
        /// The extracted sources are merged with existing sources in the builder.
        /// This is useful for loading configuration sources from external configurations or configuration files.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var externalConfig = new ConfigurationBuilder()
        ///     .AddInMemoryCollection(new Dictionary&lt;string, string&gt; 
        ///     {
        ///         { "configurationSource:RawJsonStrings:0", "{\"Key\":\"Value\"}" }
        ///     })
        ///     .Build();
        /// 
        /// var result = builder.WithAdditionalConfigurationSourcesFrom(externalConfig);
        /// </code>
        /// </example>
        public Result<ConfigurationBuilder> WithAdditionalConfigurationSourcesFrom(params IConfiguration[] configurations)
        {
            return _configurationSourceBuilder.WithAdditionalConfigurationSourcesFrom(configurations).Bind(_ => Result.Ok(this));
        }

        /// <summary>
        /// Adds pre-built <see cref="ConfigurationSource"/> instances to the builder by merging their sources.
        /// This method directly adds configuration sources without extracting from <see cref="IConfiguration"/> instances.
        /// </summary>
        /// <param name="configurationSources">
        /// An array of <see cref="ConfigurationSource"/> instances whose sources will be merged with the current builder.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result with appropriate error information.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method delegates to <see cref="ConfigurationSourceBuilder.WithAdditionalConfigurationSources"/>
        /// to merge all source types (RawJsonStrings, JsonUriStrings, EmbeddedJsonResources, LocalSecrets)
        /// from the provided configuration sources into the current builder.
        /// </para>
        /// <para>
        /// This is useful when you have existing <see cref="ConfigurationSource"/> objects that you want to
        /// compose together or reuse across multiple configuration builds.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var baseSource = ConfigurationSourceBuilder.CreateNew()
        ///     .Bind(b => b.WithRawJsonStrings("{\"BaseKey\":\"BaseValue\"}"))
        ///     .Bind(b => b.Build())
        ///     .Value;
        /// 
        /// var result = ConfigurationBuilder.CreateNew()
        ///     .Bind(b => b.WithAdditionalConfigurationSources(baseSource))
        ///     .Bind(b => b.WithRawJsonStrings("{\"AdditionalKey\":\"AdditionalValue\"}"))
        ///     .Bind(b => b.Build());
        /// </code>
        /// </example>
        public Result<ConfigurationBuilder> WithAdditionalConfigurationSources(params ConfigurationSource[] configurationSources)
        {
            return _configurationSourceBuilder.WithAdditionalConfigurationSources(configurationSources).Bind(_ => Result.Ok(this));
        }

        /// <summary>
        /// Adds additional configurations extracted from existing <see cref="IConfiguration"/> instances.
        /// Extracts the "configuration" section from each provided configuration and merges them with the final configuration.
        /// </summary>
        /// <param name="configurations">
        /// An array of <see cref="IConfiguration"/> instances containing "configuration" sections to extract.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result if extraction fails.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method extracts the "configuration" section from each provided configuration.
        /// If the "configuration" section does not exist in a configuration, that configuration is skipped with a failure reason.
        /// </para>
        /// <para>
        /// The extracted configurations are added as additional configurations to be merged with the final configuration.
        /// This is useful for loading configuration data from external configurations or configuration files.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var externalConfig = new ConfigurationBuilder()
        ///     .AddInMemoryCollection(new Dictionary&lt;string, string&gt; 
        ///     {
        ///         { "configuration:Database", "Server=localhost" }
        ///     })
        ///     .Build();
        /// 
        /// var result = builder.WithAdditionalConfigurationsFrom(externalConfig);
        /// </code>
        /// </example>
        public Result<ConfigurationBuilder> WithAdditionalConfigurationsFrom(params IConfiguration[] configurations)
        {
            return WithAdditionalConfigurations(configurations.Select(configuration => GetObjectConfigurationOrDefault(configuration).ThrowIfFailed().Value).ToArray());
        }

        private static Result<IConfigurationSection> GetObjectConfigurationOrDefault(IConfiguration configuration)
        {
            return Result.Ok(GetObjectConfigurationSection(configuration).ValueOrDefault);
        }

        private static Result<IConfigurationSection> GetObjectConfigurationSection(IConfiguration configurationSection)
        {
            var objectConfigurationSection = configurationSection.GetSection(ConfigurationSectionKey);
            return objectConfigurationSection.Exists() ?
                   Result.Ok(objectConfigurationSection) :
                   Result.Fail(Enumerable.Empty<IError>()).WithReason(new ConfigurationNotDefined());
        }

        /// <summary>
        /// Builds and returns an <see cref="IConfiguration"/> instance containing all the configuration sources added to this builder.
        /// The builder is automatically disposed after this method completes successfully.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the constructed <see cref="IConfiguration"/> if successful;
        /// otherwise, a failed result with appropriate error information.
        /// </returns>
        /// <remarks>
        /// <para>
        /// After calling this method, the builder is disposed and cannot be reused.
        /// Any subsequent operations will fail with a <see cref="ConfigurationBuilderDisposed"/> reason.
        /// </para>
        /// <para>
        /// The build process:
        /// 1. Builds the <see cref="ConfigurationSource"/> from accumulated sources
        /// 2. Creates an <see cref="IConfiguration"/> from the source
        /// 3. Merges any additional configurations
        /// 4. Disposes the builder
        /// 5. Returns the final configuration
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = ConfigurationBuilder.CreateNew()
        ///     .Bind(b => b.WithRawJsonStrings("{\"Key\":\"Value\"}"))
        ///     .Bind(b => b.Build());
        /// 
        /// if (result.IsSuccess)
        /// {
        ///     var config = result.Value;
        ///     Console.WriteLine(config["Key"]); // Outputs: Value
        /// }
        /// </code>
        /// </example>
        public Result<IConfiguration> Build()
        {
            return FailIfDisposed().Bind(_configurationSourceBuilder.Build)
                                   .Bind(configSource => configSource.Build(additionalConfigs.ToArray()))
                                   .Bind(configuration => Result.Try(() => { Dispose(); return configuration; }));
        }

        /// <summary>
        /// Checks if the builder has been disposed and returns a failed result if so.
        /// </summary>
        /// <returns>
        /// A <see cref="Result"/> that is successful if the builder is not disposed;
        /// otherwise, a failed result with <see cref="ConfigurationBuilderDisposed"/> reason.
        /// </returns>
        private Result FailIfDisposed()
        {
            return Result.FailIf(_isDisposed, new Error(string.Empty))
                         .AddReasonIfFailed(new ConfigurationBuilderDisposed());
        }

        /// <summary>
        /// Releases all resources used by the <see cref="ConfigurationBuilder"/>.
        /// </summary>
        /// <remarks>
        /// This method implements the dispose pattern and can be called multiple times safely.
        /// After disposal, all builder operations will fail with a <see cref="ConfigurationBuilderDisposed"/> reason.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ConfigurationBuilder"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _configurationSourceBuilder.Dispose();
                    _configurationSourceBuilder = null;
                }
                _isDisposed = true;
            }
        }
    }

    /// <summary>
    /// Provides a type-safe fluent builder for constructing strongly-typed configuration instances with validation support.
    /// Extends <see cref="ConfigurationBuilder"/> to add configuration binding, URI expansion, and validation capabilities.
    /// </summary>
    /// <typeparam name="TConfiguration">
    /// The strongly-typed configuration class that inherits from <see cref="AConfiguration"/>.
    /// This type will be populated with values from the configuration sources.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// This generic builder extends the base <see cref="ConfigurationBuilder"/> by adding:
    /// - Automatic binding of <see cref="IConfiguration"/> to strongly-typed configuration objects
    /// - URI expansion for properties decorated with <see cref="URIAttribute"/>
    /// - Validation pipeline using <see cref="IValidator{T}"/>
    /// </para>
    /// <para>
    /// The build process executes in the following order:
    /// 1. Build base <see cref="IConfiguration"/>
    /// 2. Bind to <typeparamref name="TConfiguration"/>
    /// 3. Expand URIs in configuration properties
    /// 4. Run all registered validators
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public record MyConfig : AConfiguration
    /// {
    ///     public string DatabaseConnection { get; set; }
    ///     public int MaxRetries { get; set; }
    /// }
    /// 
    /// var result = new ConfigurationBuilder&lt;MyConfig&gt;()
    ///     .WithRawJsonStrings("{\"DatabaseConnection\":\"Server=localhost\",\"MaxRetries\":3}")
    ///     .Bind(b => b.WithValidators(new MyConfigValidator()))
    ///     .Bind(b => b.Build());
    /// 
    /// if (result.IsSuccess)
    /// {
    ///     var config = result.Value;
    ///     Console.WriteLine(config.DatabaseConnection);
    /// }
    /// </code>
    /// </example>
    public class ConfigurationBuilder<TConfiguration> : ConfigurationBuilder where TConfiguration : AConfiguration
    {
        private List<IValidator<TConfiguration>> validators = new List<IValidator<TConfiguration>>();

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationBuilder{TConfiguration}"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a new <see cref="ConfigurationBuilder{TConfiguration}"/> instance.
        /// </returns>
        /// <example>
        /// <code>
        /// var builderResult = ConfigurationBuilder&lt;MyConfiguration&gt;.CreateNew();
        /// if (builderResult.IsSuccess)
        /// {
        ///     var builder = builderResult.Value;
        ///     // Use the builder...
        /// }
        /// </code>
        /// </example>
        public static new Result<ConfigurationBuilder<TConfiguration>> CreateNew() => Result.Ok(new ConfigurationBuilder<TConfiguration>());

        /// <summary>
        /// Adds one or more validators to the validation pipeline for the configuration object.
        /// Validators are executed in the order they are added during the build process.
        /// </summary>
        /// <param name="validators">
        /// An array of <see cref="IValidator{T}"/> instances to be executed during configuration validation.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Validators are executed sequentially after the configuration is bound and URIs are expanded.
        /// If any validator fails, the build process stops and returns a failed result.
        /// </para>
        /// <para>
        /// This method can be called multiple times to accumulate validators.
        /// All validators will be executed in the order they were added.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = builder.WithValidators(
        ///     new ConnectionStringValidator(),
        ///     new PortRangeValidator(),
        ///     new RequiredFieldsValidator()
        /// );
        /// </code>
        /// </example>
        public Result<ConfigurationBuilder<TConfiguration>> WithValidators(params IValidator<TConfiguration>[] validators)
        {
            return Result.Try(() => { this.validators.AddRange(validators); })
                         .Bind(() => Result.Ok(this));
        }

        /// <summary>
        /// Builds and returns a strongly-typed <typeparamref name="TConfiguration"/> instance containing all the configuration sources,
        /// with URI expansion and validation applied. The builder is automatically disposed after this method completes successfully.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the constructed and validated <typeparamref name="TConfiguration"/> instance if successful;
        /// otherwise, a failed result with appropriate error information from binding, URI expansion, or validation failures.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The build process executes the following steps in order:
        /// 1. Calls base <see cref="ConfigurationBuilder.Build"/> to create <see cref="IConfiguration"/>
        /// 2. Binds the configuration to <typeparamref name="TConfiguration"/> using <see cref="IConfiguration.Get{T}"/>
        /// 3. If binding returns null, creates a new instance using <see cref="Activator.CreateInstance{T}"/>
        /// 4. Expands all URI properties marked with <see cref="URIAttribute"/> (environment variable substitution)
        /// 5. Executes all registered validators in sequence
        /// 6. Returns the validated configuration instance
        /// </para>
        /// <para>
        /// If any step fails, the build process stops and returns a failed result with error details.
        /// The builder is disposed regardless of success or failure.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public record AppConfig : AConfiguration
        /// {
        ///     [URI]
        ///     public string LogPath { get; set; } // e.g., "${HOME}/logs"
        ///     public int Port { get; set; }
        /// }
        /// 
        /// var result = new ConfigurationBuilder&lt;AppConfig&gt;()
        ///     .WithRawJsonStrings("{\"LogPath\":\"${HOME}/logs\",\"Port\":8080}")
        ///     .Bind(b => b.WithValidators(new PortValidator()))
        ///     .Bind(b => b.Build());
        /// 
        /// if (result.IsSuccess)
        /// {
        ///     var config = result.Value;
        ///     // LogPath will have ${HOME} expanded to actual home directory
        ///     Console.WriteLine(config.LogPath); // e.g., "/home/user/logs"
        /// }
        /// </code>
        /// </example>
        public new Result<TConfiguration> Build()
        {
            return base.Build()
                       .Bind(configuration => Result.Try(() => configuration.Get<TConfiguration>() ??
                                                               Activator.CreateInstance<TConfiguration>()))
                       .Bind(configuration => configuration.ExpandURIs())
                       .Bind(configuration => validators.Aggregate(Result.Ok(), (seed, next) => seed.Bind(() => next.Run(configuration)))
                                                        .Bind(() => Result.Ok(configuration)));
        }
    }
}
