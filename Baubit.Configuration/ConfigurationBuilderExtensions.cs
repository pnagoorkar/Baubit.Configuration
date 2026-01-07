using Baubit.Validation;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Baubit.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static Result<ConfigurationBuilder> WithJsonUriStrings(this Result<ConfigurationBuilder> result, params string[] jsonUriStrings)
        {
            return result.Bind(cb => cb.WithJsonUriStrings(jsonUriStrings));
        }

        public static Result<ConfigurationBuilder> WithEmbeddedJsonResources(this Result<ConfigurationBuilder> result, params string[] embeddedJsonResources)
        {
            return result.Bind(cb => cb.WithEmbeddedJsonResources(embeddedJsonResources));
        }

        public static Result<ConfigurationBuilder> WithLocalSecrets(this Result<ConfigurationBuilder> result, params string[] localSecrets)
        {
            return result.Bind(cb => cb.WithLocalSecrets(localSecrets));
        }

        public static Result<ConfigurationBuilder> WithRawJsonStrings(this Result<ConfigurationBuilder> result, params string[] rawJsonStrings)
        {
            return result.Bind(cb => cb.WithRawJsonStrings(rawJsonStrings));
        }

        public static Result<ConfigurationBuilder> WithAdditionalConfigurations(this Result<ConfigurationBuilder> result, params IConfiguration[] configurations)
        {
            return result.Bind(cb => cb.WithAdditionalConfigurations(configurations));
        }

        public static Result<ConfigurationBuilder> WithAdditionalConfigurationSourcesFrom(this Result<ConfigurationBuilder> result, params IConfiguration[] configurations)
        {
            return result.Bind(cb => cb.WithAdditionalConfigurationSourcesFrom(configurations));
        }

        public static Result<ConfigurationBuilder> WithAdditionalConfigurationSources(this Result<ConfigurationBuilder> result, params ConfigurationSource[] configurationSources)
        {
            return result.Bind(cb => cb.WithAdditionalConfigurationSources(configurationSources));
        }

        public static Result<ConfigurationBuilder> WithAdditionalConfigurationsFrom(this Result<ConfigurationBuilder> result, params IConfiguration[] configurations)
        {
            return result.Bind(cb => cb.WithAdditionalConfigurationsFrom(configurations));
        }

        public static Result<IConfiguration> Build(this Result<ConfigurationBuilder> result)
        {
            return result.Bind(cb => cb.Build());
        }

        public static Result<ConfigurationBuilder<TConfiguration>> WithValidators<TConfiguration>(this Result<ConfigurationBuilder<TConfiguration>> result, params IValidator<TConfiguration>[] validators) where TConfiguration : Configuration
        {
            return result.Bind(cb => cb.WithValidators(validators));
        }

        public static Result<TConfiguration> Build<TConfiguration>(this Result<ConfigurationBuilder<TConfiguration>> result) where TConfiguration : Configuration
        {
            return result.Bind(cb => cb.Build());
        }
    }
}
