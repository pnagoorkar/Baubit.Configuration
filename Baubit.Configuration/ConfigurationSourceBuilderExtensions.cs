using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Baubit.Configuration
{
    public static class ConfigurationSourceBuilderExtensions
    {
        public static Result<ConfigurationSourceBuilder> WithJsonUriStrings(this Result<ConfigurationSourceBuilder> result, params string[] jsonUriStrings)
        {
            return result.Bind(csb => csb.WithJsonUriStrings(jsonUriStrings));
        }

        public static Result<ConfigurationSourceBuilder> WithEmbeddedJsonResources(this Result<ConfigurationSourceBuilder> result, params string[] embeddedJsonResources)
        {
            return result.Bind(csb => csb.WithEmbeddedJsonResources(embeddedJsonResources));
        }

        public static Result<ConfigurationSourceBuilder> WithLocalSecrets(this Result<ConfigurationSourceBuilder> result, params string[] localSecrets)
        {
            return result.Bind(csb => csb.WithLocalSecrets(localSecrets));
        }

        public static Result<ConfigurationSourceBuilder> WithRawJsonStrings(this Result<ConfigurationSourceBuilder> result, params string[] rawJsonStrings)
        {
            return result.Bind(csb => csb.WithRawJsonStrings(rawJsonStrings));
        }

        public static Result<ConfigurationSourceBuilder> WithAdditionalConfigurationSources(this Result<ConfigurationSourceBuilder> result, params ConfigurationSource[] configSources)
        {
            return result.Bind(csb => csb.WithAdditionalConfigurationSources(configSources));
        }

        public static Result<ConfigurationSourceBuilder> WithAdditionalConfigurationSourcesFrom(this Result<ConfigurationSourceBuilder> result, params IConfiguration[] configurations)
        {
            return result.Bind(csb => csb.WithAdditionalConfigurationSourcesFrom(configurations));
        }

        public static Result<ConfigurationSource> Build(this Result<ConfigurationSourceBuilder> result)
        {
            return result.Bind(csb => csb.Build());
        }
    }
}
