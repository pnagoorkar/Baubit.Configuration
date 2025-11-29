using Baubit.Configuration.Traceability;
using Baubit.Traceability;
using FluentResults;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Baubit.Configuration
{
    /// <summary>
    /// Provides a fluent builder for constructing <see cref="ConfigurationSource"/> instances.
    /// This class follows the builder pattern and implements <see cref="IDisposable"/> to ensure proper resource cleanup.
    /// </summary>
    /// <remarks>
    /// The builder is automatically disposed after calling <see cref="Build"/>. 
    /// Any subsequent operations on a disposed builder will return a failed result with <see cref="ConfigurationBuilderDisposed"/> reason.
    /// </remarks>
    public sealed class ConfigurationSourceBuilder : IDisposable
    {
        /// <summary>
        /// The configuration source section key used to identify configuration source metadata within an <see cref="IConfiguration"/> instance.
        /// This constant is used when extracting configuration sources via <see cref="WithAdditionalConfigurationSourcesFrom"/>.
        /// </summary>
        /// <remarks>
        /// When loading configuration sources from external sources, this key identifies the section containing
        /// the source definitions (RawJsonStrings, JsonUriStrings, EmbeddedJsonResources, LocalSecrets). The value is "configurationSource".
        /// </remarks>
        public const string ConfigurationSourceSectionKey = "configurationSource";
        private List<string> RawJsonStrings { get; set; } = new List<string>();
        private List<string> JsonUriStrings { get; set; } = new List<string>();
        private List<string> EmbeddedJsonResources { get; set; } = new List<string>();
        private List<string> LocalSecrets { get; set; } = new List<string>();

        private bool _isDisposed;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSourceBuilder"/> class.
        /// This constructor is private to enforce the use of factory methods.
        /// </summary>
        private ConfigurationSourceBuilder()
        {
        }

        /// <summary>
        /// Creates and builds an empty <see cref="ConfigurationSource"/> with no configuration sources.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing an empty <see cref="ConfigurationSource"/> if successful.
        /// </returns>
        /// <exception cref="Exception">
        /// Throws if the build operation fails (via <see cref="Result{T}.ThrowIfFailed"/>).
        /// </exception>
        public static Result<ConfigurationSource> BuildEmpty() => CreateNew().Bind(configSourceBuilder => configSourceBuilder.Build()).ThrowIfFailed();

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationSourceBuilder"/>
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a new <see cref="ConfigurationSourceBuilder"/> instance
        /// </returns>
        public static Result<ConfigurationSourceBuilder> CreateNew() => Result.Ok(new ConfigurationSourceBuilder());

        /// <summary>
        /// Adds one or more JSON URI strings to the configuration source.
        /// These URIs will be used to load JSON configuration from remote or local file locations.
        /// </summary>
        /// <param name="jsonUriStrings">An array of URI strings pointing to JSON configuration files.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result with <see cref="ConfigurationBuilderDisposed"/> if the builder has been disposed.
        /// </returns>
        /// <remarks>
        /// This method can be called multiple times to accumulate multiple JSON URIs.
        /// </remarks>
        public Result<ConfigurationSourceBuilder> WithJsonUriStrings(params string[] jsonUriStrings)
        {
            return Result.FailIf(_isDisposed, new Error(string.Empty))
                         .AddReasonIfFailed(new ConfigurationBuilderDisposed())
                         .Bind(() => Result.Try(() => JsonUriStrings.AddRange(jsonUriStrings)))
                         .Bind(() => Result.Ok(this));
        }

        /// <summary>
        /// Adds one or more embedded JSON resource names to the configuration source.
        /// These resources should be embedded in the assembly as manifest resources.
        /// </summary>
        /// <param name="embeddedJsonResources">An array of fully qualified embedded resource names.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result with <see cref="ConfigurationBuilderDisposed"/> if the builder has been disposed.
        /// </returns>
        /// <remarks>
        /// This method can be called multiple times to accumulate multiple embedded resources.
        /// Resource names should follow the format: Namespace.Folder.FileName.json
        /// </remarks>
        public Result<ConfigurationSourceBuilder> WithEmbeddedJsonResources(params string[] embeddedJsonResources)
        {
            return Result.FailIf(_isDisposed, new Error(string.Empty))
                         .AddReasonIfFailed(new ConfigurationBuilderDisposed())
                         .Bind(() => Result.Try(() => EmbeddedJsonResources.AddRange(embeddedJsonResources)))
                         .Bind(() => Result.Ok(this));
        }

        /// <summary>
        /// Adds one or more local secret identifiers to the configuration source.
        /// These identifiers will be used to load user secrets from the local development environment.
        /// </summary>
        /// <param name="localSecrets">An array of user secret identifiers (typically project names or GUIDs).</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result with <see cref="ConfigurationBuilderDisposed"/> if the builder has been disposed.
        /// </returns>
        /// <remarks>
        /// This method can be called multiple times to accumulate multiple secret identifiers.
        /// Local secrets are typically used during development and stored in the user's profile directory.
        /// </remarks>
        public Result<ConfigurationSourceBuilder> WithLocalSecrets(params string[] localSecrets)
        {
            return Result.FailIf(_isDisposed, new Error(string.Empty))
                         .AddReasonIfFailed(new ConfigurationBuilderDisposed())
                         .Bind(() => Result.Try(() => LocalSecrets.AddRange(localSecrets)))
                         .Bind(() => Result.Ok(this));
        }

        /// <summary>
        /// Adds one or more raw JSON strings to the configuration source.
        /// These strings contain JSON configuration data directly in string format.
        /// </summary>
        /// <param name="rawJsonStrings">An array of strings containing valid JSON configuration data.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result with <see cref="ConfigurationBuilderDisposed"/> if the builder has been disposed.
        /// </returns>
        /// <remarks>
        /// This method can be called multiple times to accumulate multiple JSON strings.
        /// The JSON strings should be well-formed and contain valid configuration data.
        /// </remarks>
        public Result<ConfigurationSourceBuilder> WithRawJsonStrings(params string[] rawJsonStrings)
        {
            return Result.FailIf(_isDisposed, new Error(string.Empty))
                         .AddReasonIfFailed(new ConfigurationBuilderDisposed())
                         .Bind(() => Result.Try(() => RawJsonStrings.AddRange(rawJsonStrings)))
                         .Bind(() => Result.Ok(this));
        }

        /// <summary>
        /// Adds additional configuration sources by merging them with the current builder.
        /// This method combines all configuration sources from the provided <see cref="ConfigurationSource"/> instances.
        /// </summary>
        /// <param name="configSources">
        /// An array of <see cref="ConfigurationSource"/> instances whose sources will be merged with the current builder.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current builder instance for method chaining if successful;
        /// otherwise, a failed result with appropriate error information.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method extracts all source types (RawJsonStrings, JsonUriStrings, EmbeddedJsonResources, LocalSecrets)
        /// from the provided configuration sources and adds them to the current builder.
        /// </para>
        /// <para>
        /// Sources are accumulated in the order they are provided.
        /// This is useful for composing configuration sources from multiple existing sources.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var existingSource = ConfigurationSourceBuilder.CreateNew()
        ///     .Bind(b => b.WithRawJsonStrings("{\"Key\":\"Value\"}"))
        ///     .Bind(b => b.Build())
        ///     .Value;
        /// 
        /// var result = builder.WithAdditionalConfigurationSources(existingSource);
        /// </code>
        /// </example>
        public Result<ConfigurationSourceBuilder> WithAdditionalConfigurationSources(params ConfigurationSource[] configSources)
        {
            var rawJsonStrings = new List<string>();
            var jsonUriStrings = new List<string>();
            var embeddedJsonResources = new List<string>();
            var localSecrets = new List<string>();
            return Result.Try(() =>
            {
                foreach (var configSource in configSources)
                {
                    rawJsonStrings.AddRange(configSource.RawJsonStrings);
                    jsonUriStrings.AddRange(configSource.JsonUriStrings);
                    embeddedJsonResources.AddRange(configSource.EmbeddedJsonResources);
                    localSecrets.AddRange(configSource.LocalSecrets);
                }
            }).Bind(() => WithRawJsonStrings(rawJsonStrings.ToArray()))
              .Bind(_ => WithJsonUriStrings(jsonUriStrings.ToArray()))
              .Bind(_ => WithEmbeddedJsonResources(embeddedJsonResources.ToArray()))
              .Bind(_ => WithLocalSecrets(localSecrets.ToArray()));
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
        /// This method extracts the "configurationSource" section from each provided configuration
        /// and binds it to a <see cref="ConfigurationSource"/> object.
        /// If the section does not exist or cannot be bound, an empty configuration source is used.
        /// </para>
        /// <para>
        /// The extracted sources are then merged using <see cref="WithAdditionalConfigurationSources"/>.
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
        public Result<ConfigurationSourceBuilder> WithAdditionalConfigurationSourcesFrom(params IConfiguration[] configurations)
        {
            return WithAdditionalConfigurationSources(configurations.Select(configuration => GetObjectConfigurationSourceOrDefault(configuration).ThrowIfFailed().Value).ToArray());
        }

        private static Result<ConfigurationSource> GetObjectConfigurationSourceOrDefault(IConfiguration configuration)
        {
            return Result.Ok(GetObjectConfigurationSourceSection(configuration).ValueOrDefault?.Get<ConfigurationSource>() ?? BuildEmpty().Value);
        }

        private static Result<IConfigurationSection> GetObjectConfigurationSourceSection(IConfiguration configurationSection)
        {
            var objectConfigurationSourceSection = configurationSection.GetSection(ConfigurationSourceSectionKey);
            return objectConfigurationSourceSection.Exists() ?
                   Result.Ok(objectConfigurationSourceSection) :
                   Result.Fail(Enumerable.Empty<IError>()).WithReason(new ConfigurationSourceNotDefined());
        }

        /// <summary>
        /// Builds and returns a <see cref="ConfigurationSource"/> containing all the configuration sources added to this builder.
        /// The builder is automatically disposed after this method completes successfully.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the constructed <see cref="ConfigurationSource"/> if successful;
        /// otherwise, a failed result with <see cref="ConfigurationBuilderDisposed"/> if the builder has already been disposed.
        /// </returns>
        /// <remarks>
        /// After calling this method, the builder is disposed and cannot be reused.
        /// Any subsequent operations will fail with a <see cref="ConfigurationBuilderDisposed"/> reason.
        /// The returned <see cref="ConfigurationSource"/> contains independent copies of all configuration data.
        /// </remarks>
        public Result<ConfigurationSource> Build()
        {
            return Result.FailIf(_isDisposed, new Error(string.Empty))
                         .AddReasonIfFailed(new ConfigurationBuilderDisposed())
                         .Bind(() => Result.Try(() => new ConfigurationSource(RawJsonStrings.ToList(), JsonUriStrings.ToList(), EmbeddedJsonResources.ToList(), LocalSecrets.ToList())))
                         .Bind(configSource => Result.Try(() => { Dispose(); return configSource; }));
        }

        /// <summary>
        /// Releases all resources used by the <see cref="ConfigurationSourceBuilder"/>.
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
        /// Releases the unmanaged resources used by the <see cref="ConfigurationSourceBuilder"/> and optionally releases the managed resources.
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
                    RawJsonStrings.Clear();
                    JsonUriStrings.Clear();
                    EmbeddedJsonResources.Clear();
                    LocalSecrets.Clear();

                    RawJsonStrings = null;
                    JsonUriStrings = null;
                    EmbeddedJsonResources = null;
                    LocalSecrets = null;
                }
                _isDisposed = true;
            }
        }
    }
}
