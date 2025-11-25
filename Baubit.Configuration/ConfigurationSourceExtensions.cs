using Baubit.Configuration.Traceability;
using Baubit.Reflection;
using Baubit.Traceability;
using Baubit.Traceability.Exceptions;
using FluentResults;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Baubit.Configuration
{
    /// <summary>
    /// Provides extension methods for building and manipulating configuration sources.
    /// </summary>
    public static class ConfigurationSourceExtensions
    {
        /// <summary>
        /// Builds an <see cref="IConfiguration"/> from the specified <see cref="ConfigurationSource"/>.
        /// </summary>
        /// <param name="configurationSource">The configuration source to build from.</param>
        /// <returns>A <see cref="Result{T}"/> containing the built <see cref="IConfiguration"/>.</returns>
        public static Result<IConfiguration> Build(this ConfigurationSource configurationSource) 
            => configurationSource.Build(null);

        /// <summary>
        /// Builds an <see cref="IConfiguration"/> from the specified <see cref="ConfigurationSource"/> 
        /// and merges it with additional configurations.
        /// </summary>
        /// <param name="configurationSource">The configuration source to build from.</param>
        /// <param name="additionalConfigs">Additional configurations to merge.</param>
        /// <returns>A <see cref="Result{T}"/> containing the built and merged <see cref="IConfiguration"/>.</returns>
        /// <remarks>
        /// The build process:
        /// 1. Validates the configuration source is not null
        /// 2. Expands URI placeholders (e.g., ${HOME})
        /// 3. Adds JSON files from URIs
        /// 4. Loads embedded resource files
        /// 5. Adds raw JSON strings
        /// 6. Adds user secrets
        /// 7. Merges additional configurations
        /// 8. Returns the final configuration
        /// </remarks>
        public static Result<IConfiguration> Build(this ConfigurationSource configurationSource, params IConfiguration[] additionalConfigs)
        {
            var configurationBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            
            return Result.OkIf(configurationSource != null, "ConfigurationSource cannot be null")
                         .Bind(() => configurationSource.ExpandURIs())
                         .Bind(configSource => configurationSource.AddJsonFiles(configurationBuilder))
                         .Bind(cs => cs.LoadResourceFiles())
                         .Bind(cs => cs.AddRawJsonStrings(configurationBuilder))
                         .Bind(cs => cs.AddSecrets(configurationBuilder))
                         .Bind(_ => AddAdditionalConfigurations(configurationBuilder, additionalConfigs))
                         .Bind(() => Result.Ok<IConfiguration>(configurationBuilder.Build()));
        }

        /// <summary>
        /// Expands environment variable placeholders in URI-decorated properties of the specified object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object whose URI properties should be expanded.</param>
        /// <returns>A <see cref="Result{T}"/> containing the object with expanded URIs.</returns>
        /// <remarks>
        /// This method processes all properties decorated with <see cref="URIAttribute"/> and replaces
        /// environment variable placeholders (e.g., ${HOME}) with their actual values.
        /// Supports both <see cref="string"/> and <see cref="List{T}"/> of <see cref="string"/> properties.
        /// </remarks>
        public static Result<T> ExpandURIs<T>(this T obj)
        {
            if (obj == null) return Result.Ok(obj);

            try
            {
                var uriProperties = GetUriProperties(obj.GetType());
                
                foreach (var property in uriProperties)
                {
                    var result = ExpandPropertyUri(property, obj);
                    if (result.IsFailed) return result.ToResult<T>();
                }

                return Result.Ok(obj);
            }
            catch (FailedOperationException failedOpEx)
            {
                return Result.Fail<T>(Enumerable.Empty<IError>()).WithReasons(failedOpEx.Result.Reasons);
            }
        }

        /// <summary>
        /// Gets all properties decorated with <see cref="URIAttribute"/> from the specified type.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns>An enumerable of properties that have the URI attribute.</returns>
        private static IEnumerable<PropertyInfo> GetUriProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                      .Where(property => property.CustomAttributes
                                                .Any(att => att.AttributeType.Equals(typeof(URIAttribute))));
        }

        /// <summary>
        /// Expands URI placeholders in the specified property based on its type.
        /// </summary>
        /// <param name="property">The property to expand.</param>
        /// <param name="obj">The object instance containing the property.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
        private static Result ExpandPropertyUri(PropertyInfo property, object obj)
        {
            if (property.PropertyType == typeof(string))
            {
                return ExpandStringProperty(property, obj);
            }
            else if (property.PropertyType == typeof(List<string>))
            {
                return ExpandListProperty(property, obj);
            }
            else
            {
                return Result.Fail($"Unsupported URI property type: {property.PropertyType.Name}");
            }
        }

        /// <summary>
        /// Expands URI placeholders in a string property.
        /// </summary>
        /// <param name="property">The string property to expand.</param>
        /// <param name="obj">The object instance containing the property.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
        private static Result ExpandStringProperty(PropertyInfo property, object obj)
        {
            var currentValue = (string)property.GetValue(obj);
            
            return currentValue.ExpandURIString()
                              .Bind(expanded =>
                              {
                                  property.SetValue(obj, expanded);
                                  return Result.Ok();
                              });
        }

        /// <summary>
        /// Expands URI placeholders in a list of strings property.
        /// </summary>
        /// <param name="property">The list property to expand.</param>
        /// <param name="obj">The object instance containing the property.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
        private static Result ExpandListProperty(PropertyInfo property, object obj)
        {
            var currentValues = (List<string>)property.GetValue(obj);
            
            if (currentValues == null || currentValues.Count == 0) 
                return Result.Ok();

            return Result.Try(() =>
            {
                var expandedValues = currentValues
                    .Select(val => val.ExpandURIString().ThrowIfFailed().Value)
                    .ToList();
                    
                property.SetValue(obj, expandedValues);
            });
        }

        /// <summary>
        /// Expands environment variable placeholders in a URI string.
        /// </summary>
        /// <param name="value">The string containing placeholders (e.g., ${HOME}/config).</param>
        /// <returns>A <see cref="Result{T}"/> containing the expanded string.</returns>
        /// <remarks>
        /// Placeholders use the format ${VARIABLE_NAME} and are replaced with the corresponding
        /// environment variable value. If a variable is not found, an error is returned.
        /// </remarks>
        private static Result<string> ExpandURIString(this string value)
        {
            if (string.IsNullOrEmpty(value)) 
                return Result.Ok(value);

            return Result.Try(
                () => Regex.Replace(value, @"\$\{(.*?)\}", ReplaceEnvironmentVariable),
                HandleMissingEnvVariables
            );
        }

        /// <summary>
        /// Replaces a regex match with the corresponding environment variable value.
        /// </summary>
        /// <param name="match">The regex match containing the variable name.</param>
        /// <returns>The environment variable value.</returns>
        /// <exception cref="EnvironmentVariableNotFound">Thrown when the environment variable is not found.</exception>
        private static string ReplaceEnvironmentVariable(Match match)
        {
            var key = match.Groups[1].Value;
            var replacement = Environment.GetEnvironmentVariable(key);
            
            if (replacement == null)
                throw new EnvironmentVariableNotFound(match.Value);
                
            return replacement;
        }

        /// <summary>
        /// Handles exceptions that occur during environment variable expansion.
        /// </summary>
        /// <param name="exp">The exception to handle.</param>
        /// <returns>An <see cref="IError"/> representing the error.</returns>
        private static IError HandleMissingEnvVariables(Exception exp)
        {
            if (exp is EnvironmentVariableNotFound envVarNotFoundExp)
            {
                return new EnvVarNotFound(envVarNotFoundExp.EnvVariable);
            }
            else
            {
                return new ExceptionalError(exp);
            }
        }

        /// <summary>
        /// Adds additional configurations to the configuration builder.
        /// </summary>
        /// <param name="configurationBuilder">The configuration builder to add to.</param>
        /// <param name="additionalConfigs">The additional configurations to add.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
        private static Result AddAdditionalConfigurations(
            IConfigurationBuilder configurationBuilder, 
            IConfiguration[] additionalConfigs)
        {
            if (additionalConfigs == null) 
                return Result.Ok();

            return additionalConfigs.Aggregate(
                Result.Ok(), 
                (seed, next) => seed.Bind(() => configurationBuilder.AddConfigurationToBuilder(next))
            );
        }

        /// <summary>
        /// Adds a configuration to the configuration builder.
        /// </summary>
        /// <param name="configurationBuilder">The builder to add to.</param>
        /// <param name="configuration">The configuration to add.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
        private static Result AddConfigurationToBuilder(
            this IConfigurationBuilder configurationBuilder, 
            IConfiguration configuration)
        {
            return Result.Try(() =>
            {
                if (configuration != null) 
                    configurationBuilder.AddConfiguration(configuration);
            });
        }

        /// <summary>
        /// Adds JSON files from URIs to the configuration builder.
        /// </summary>
        /// <param name="configurationSource">The source containing JSON URIs.</param>
        /// <param name="configurationBuilder">The builder to add JSON files to.</param>
        /// <returns>A <see cref="Result{T}"/> containing the configuration source.</returns>
        /// <remarks>
        /// Only file URIs are processed. Remote HTTP(S) URIs are ignored.
        /// </remarks>
        private static Result<ConfigurationSource> AddJsonFiles(
            this ConfigurationSource configurationSource, 
            Microsoft.Extensions.Configuration.ConfigurationBuilder configurationBuilder)
        {
            return Result.Try(() =>
            {
                var fileUris = configurationSource.JsonUriStrings
                    .Select(uriString => new Uri(uriString))
                    .Where(uri => uri.IsFile);

                foreach (var uri in fileUris)
                {
                    configurationBuilder.AddJsonFile(uri.LocalPath);
                }
                
                return configurationSource;
            });
        }

        /// <summary>
        /// Loads embedded JSON resource files and adds them as raw JSON strings.
        /// </summary>
        /// <param name="configurationSource">The source containing embedded resource identifiers.</param>
        /// <returns>A <see cref="Result{T}"/> containing the configuration source with loaded resources.</returns>
        private static Result<ConfigurationSource> LoadResourceFiles(
            this ConfigurationSource configurationSource)
        {
            return Result.Try(() =>
            {
                foreach (var embeddedJsonResource in configurationSource.EmbeddedJsonResources)
                {
                    var resourceContent = LoadEmbeddedResource(embeddedJsonResource);
                    configurationSource.RawJsonStrings.Add(resourceContent);
                }
                
                return configurationSource;
            });
        }

        /// <summary>
        /// Loads the content of an embedded resource.
        /// </summary>
        /// <param name="embeddedJsonResource">The resource identifier (format: AssemblyName;ResourcePath).</param>
        /// <returns>The resource content as a string.</returns>
        /// <exception cref="Exception">Thrown when the resource cannot be read.</exception>
        private static string LoadEmbeddedResource(string embeddedJsonResource)
        {
            var (assemblyName, resourceName) = ParseEmbeddedResourceIdentifier(embeddedJsonResource);
            
            var readResult = assemblyName.TryResolveAssembly()
                                        ?.ReadResource(resourceName)
                                        .GetAwaiter()
                                        .GetResult();

            if (readResult?.IsSuccess != true)
                throw new Exception($"Failed to read embedded json resource {embeddedJsonResource}");
                
            return readResult.Value;
        }

        /// <summary>
        /// Parses an embedded resource identifier into assembly name and resource name.
        /// </summary>
        /// <param name="embeddedJsonResource">The resource identifier (format: AssemblyName;ResourcePath).</param>
        /// <returns>A tuple containing the assembly name and fully qualified resource name.</returns>
        private static (AssemblyName assemblyName, string resourceName) ParseEmbeddedResourceIdentifier(
            string embeddedJsonResource)
        {
            var identifierParts = embeddedJsonResource.Split(';');
            var assemblyNamePart = identifierParts[0];
            var fileNamePart = identifierParts[1];

            var assemblyName = assemblyNamePart.Contains("/")
                ? Reflection.AssemblyExtensions.GetAssemblyNameFromPersistableString(assemblyNamePart)
                : new AssemblyName(assemblyNamePart);
                
            var resourceName = $"{assemblyName.Name}.{fileNamePart}";
            
            return (assemblyName, resourceName);
        }

        /// <summary>
        /// Adds raw JSON strings as configuration streams.
        /// </summary>
        /// <param name="configurationSource">The source containing raw JSON strings.</param>
        /// Adds raw JSON strings as configuration streams.
        /// </summary>
        /// <param name="configurationSource">The source containing raw JSON strings.</param>
        /// <param name="configurationBuilder">The builder to add JSON streams to.</param>
        /// <returns>A <see cref="Result{T}"/> containing the configuration source.</returns>
        private static Result<ConfigurationSource> AddRawJsonStrings(
            this ConfigurationSource configurationSource, 
            Microsoft.Extensions.Configuration.ConfigurationBuilder configurationBuilder)
        {
            return Result.Try(() =>
            {
                var jsonStreams = configurationSource.RawJsonStrings
                    .Select(rawJson => new MemoryStream(Encoding.UTF8.GetBytes(rawJson)));

                foreach (var stream in jsonStreams)
                {
                    configurationBuilder.AddJsonStream(stream);
                }

                return configurationSource;
            });
        }

        /// <summary>
        /// Adds user secrets to the configuration builder.
        /// </summary>
        /// <param name="configurationSource">The source containing local secret identifiers.</param>
        /// <param name="configurationBuilder">The builder to add secrets to.</param>
        /// <returns>A <see cref="Result{T}"/> containing the configuration source.</returns>
        private static Result<ConfigurationSource> AddSecrets(
            this ConfigurationSource configurationSource, 
            Microsoft.Extensions.Configuration.ConfigurationBuilder configurationBuilder)
        {
            return Result.Try(() =>
            {
                foreach (var localSecretsId in configurationSource.LocalSecrets)
                {
                    configurationBuilder.AddUserSecrets(localSecretsId);
                }

                return configurationSource;
            });
        }
    }
}
