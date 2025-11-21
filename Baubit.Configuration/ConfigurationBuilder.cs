using Baubit.Configuration.Traceability;
using Baubit.Traceability;
using Baubit.Validation;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Baubit.Configuration
{
    public class ConfigurationBuilder : IDisposable
    {
        private ConfigurationSourceBuilder _configurationSourceBuilder;
        private List<IConfiguration> additionalConfigs = new List<IConfiguration>();
        private bool _isDisposed;
        protected ConfigurationBuilder()
        {
            _configurationSourceBuilder = ConfigurationSourceBuilder.CreateNew().Value;
        }

        public static Result<ConfigurationBuilder> CreateNew() => Result.Ok(new ConfigurationBuilder());

        public Result<ConfigurationBuilder> WithJsonUriStrings(params string[] jsonUriStrings)
        {
            return FailIfDisposed().Bind(() => _configurationSourceBuilder.WithJsonUriStrings(jsonUriStrings))
                                   .Bind(_ => Result.Ok(this));
        }
        public Result<ConfigurationBuilder> WithEmbeddedJsonResources(params string[] embeddedJsonResources)
        {
            return FailIfDisposed().Bind(() => _configurationSourceBuilder.WithEmbeddedJsonResources(embeddedJsonResources))
                                   .Bind(_ => Result.Ok(this));
        }
        public Result<ConfigurationBuilder> WithLocalSecrets(params string[] localSecrets)
        {
            return FailIfDisposed().Bind(() => _configurationSourceBuilder.WithLocalSecrets(localSecrets))
                                   .Bind(_ => Result.Ok(this));
        }
        public Result<ConfigurationBuilder> WithRawJsonStrings(params string[] rawJsonStrings)
        {
            return FailIfDisposed().Bind(() => _configurationSourceBuilder.WithRawJsonStrings(rawJsonStrings))
                                   .Bind(_ => Result.Ok(this));
        }
        public Result<ConfigurationBuilder> WithAdditionalConfigurations(params IConfiguration[] configurations)
        {
            return Result.Try(() => additionalConfigs.AddRange(configurations))
                         .Bind(() => Result.Ok(this));
        }
        public Result<IConfiguration> Build()
        {
            return FailIfDisposed().Bind(_configurationSourceBuilder.Build)
                                   .Bind(configSource => configSource.Build(additionalConfigs.ToArray()))
                                   .Bind(configuration => Result.Try(() => { Dispose(); return configuration; }));
        }

        private Result FailIfDisposed()
        {
            return Result.FailIf(_isDisposed, new Error(string.Empty))
                         .AddReasonIfFailed(new ConfigurationBuilderDisposed());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

    public class ConfigurationBuilder<TConfiguration> : ConfigurationBuilder where TConfiguration : AConfiguration
    {
        private List<IValidator<TConfiguration>> validators = new List<IValidator<TConfiguration>>();
        public Result<ConfigurationBuilder<TConfiguration>> WithValidators(params IValidator<TConfiguration>[] validators)
        {
            return Result.Try(() => { this.validators.AddRange(validators); })
                         .Bind(() => Result.Ok(this));
        }

        public new Result<TConfiguration> Build()
        {
            return base.Build()
                       .Bind(configuration => Result.Try(() => configuration.Get<TConfiguration>() ??
                                                               Activator.CreateInstance<TConfiguration>()!))
                       .Bind(configuration => configuration.ExpandURIs())
                       .Bind(configuration => validators.Aggregate(Result.Ok(), (seed, next) => seed.Bind(() => next.Run(configuration)))
                                                        .Bind(() => Result.Ok(configuration)));
        }
    }
}
