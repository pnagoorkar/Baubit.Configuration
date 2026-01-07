using Baubit.Configuration.Traceability;
using Baubit.Validation;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Baubit.Configuration.Test.ConfigurationBuilderExtensions
{
    #region Test Helpers

    // Test configuration class
    public class TestConfiguration : global::Baubit.Configuration.Configuration
    {
        public string? TestValue { get; set; }
        public int TestNumber { get; set; }
    }

    // Test validator that always passes
    public class PassingValidator : IValidator<TestConfiguration>
    {
        public Result Run(TestConfiguration validatable)
        {
            return Result.Ok();
        }
    }

    // Test validator that always fails
    public class FailingValidator : IValidator<TestConfiguration>
    {
        public Result Run(TestConfiguration validatable)
        {
            return Result.Fail("Validation failed");
        }
    }

    #endregion

    public class Test
    {
        #region WithJsonUriStrings Tests

        [Fact]
        public void WithJsonUriStrings_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();

            // Act
            var extensionResult = result.WithJsonUriStrings("https://example.com/config.json");

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithJsonUriStrings_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder>("Initial failure");

            // Act
            var extensionResult = failedResult.WithJsonUriStrings("https://example.com/config.json");

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithJsonUriStrings_WithMultipleUris_ShouldAccumulate()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();

            // Act
            var extensionResult = result.WithJsonUriStrings("https://example.com/config1.json", "https://example.com/config2.json")
                                        .Build();

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithJsonUriStrings_ChainedCalls_ShouldAccumulate()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithJsonUriStrings("https://example.com/config1.json")
                .WithJsonUriStrings("https://example.com/config2.json")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
        }

        #endregion

        #region WithEmbeddedJsonResources Tests

        [Fact]
        public void WithEmbeddedJsonResources_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();

            // Act
            var extensionResult = result.WithEmbeddedJsonResources("MyApp;Config.appsettings.json");

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithEmbeddedJsonResources_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder>("Initial failure");

            // Act
            var extensionResult = failedResult.WithEmbeddedJsonResources("MyApp;Config.appsettings.json");

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithEmbeddedJsonResources_WithMultipleResources_ShouldAccumulate()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();

            // Act
            var extensionResult = result.WithEmbeddedJsonResources(
                "MyApp;Config.appsettings.json",
                "MyApp;Config.appsettings.dev.json");

            // Assert - Don't call Build() because embedded resources don't exist
            Assert.True(extensionResult.IsSuccess);
        }

        #endregion

        #region WithLocalSecrets Tests

        [Fact]
        public void WithLocalSecrets_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();

            // Act
            var extensionResult = result.WithLocalSecrets("MyApp.Secrets");

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithLocalSecrets_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder>("Initial failure");

            // Act
            var extensionResult = failedResult.WithLocalSecrets("MyApp.Secrets");

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithLocalSecrets_WithMultipleSecrets_ShouldAccumulate()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();

            // Act
            var extensionResult = result.WithLocalSecrets("MyApp.Secrets.Dev", "MyApp.Secrets.Prod")
                .Build();

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        #endregion

        #region WithRawJsonStrings Tests

        [Fact]
        public void WithRawJsonStrings_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();

            // Act
            var extensionResult = result.WithRawJsonStrings("{\"Key\":\"Value\"}");

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithRawJsonStrings_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder>("Initial failure");

            // Act
            var extensionResult = failedResult.WithRawJsonStrings("{\"Key\":\"Value\"}");

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithRawJsonStrings_WithValidJson_ShouldBuildSuccessfully()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithRawJsonStrings("{\"TestKey\":\"TestValue\"}")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("TestValue", result.Value["TestKey"]);
        }

        [Fact]
        public void WithRawJsonStrings_WithMultipleJsonStrings_ShouldMerge()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key1\":\"Value1\"}", "{\"Key2\":\"Value2\"}")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value1", result.Value["Key1"]);
            Assert.Equal("Value2", result.Value["Key2"]);
        }

        #endregion

        #region WithAdditionalConfigurations Tests

        [Fact]
        public void WithAdditionalConfigurations_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

            // Act
            var extensionResult = result.WithAdditionalConfigurations(config);

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithAdditionalConfigurations_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder>("Initial failure");
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

            // Act
            var extensionResult = failedResult.WithAdditionalConfigurations(config);

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithAdditionalConfigurations_ShouldMergeConfiguration()
        {
            // Arrange
            var additionalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "AdditionalKey", "AdditionalValue" }
                })
                .Build();

            // Act
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithRawJsonStrings("{\"MainKey\":\"MainValue\"}")
                .WithAdditionalConfigurations(additionalConfig)
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("MainValue", result.Value["MainKey"]);
            Assert.Equal("AdditionalValue", result.Value["AdditionalKey"]);
        }

        #endregion

        #region WithAdditionalConfigurationSourcesFrom Tests

        [Fact]
        public void WithAdditionalConfigurationSourcesFrom_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "configurationSource:RawJsonStrings:0", "{\"Key\":\"Value\"}" }
                })
                .Build();

            // Act
            var extensionResult = result.WithAdditionalConfigurationSourcesFrom(config);

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithAdditionalConfigurationSourcesFrom_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder>("Initial failure");
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

            // Act
            var extensionResult = failedResult.WithAdditionalConfigurationSourcesFrom(config);

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithAdditionalConfigurationSourcesFrom_WithValidConfiguration_ShouldExtractAndMerge()
        {
            // Arrange
            var externalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "configurationSource:RawJsonStrings:0", "{\"SourceKey\":\"SourceValue\"}" }
                })
                .Build();

            // Act
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithAdditionalConfigurationSourcesFrom(externalConfig)
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("SourceValue", result.Value["SourceKey"]);
        }

        #endregion

        #region WithAdditionalConfigurationSources Tests

        [Fact]
        public void WithAdditionalConfigurationSources_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();
            var configSource = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key\":\"Value\"}")
                .Build()
                .Value;

            // Act
            var extensionResult = result.WithAdditionalConfigurationSources(configSource);

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithAdditionalConfigurationSources_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder>("Initial failure");
            var configSource = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key\":\"Value\"}")
                .Build()
                .Value;

            // Act
            var extensionResult = failedResult.WithAdditionalConfigurationSources(configSource);

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithAdditionalConfigurationSources_ShouldMergeSources()
        {
            // Arrange
            var configSource = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"SourceKey\":\"SourceValue\"}")
                .Build()
                .Value;

            // Act
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithRawJsonStrings("{\"BuilderKey\":\"BuilderValue\"}")
                .WithAdditionalConfigurationSources(configSource)
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("BuilderValue", result.Value["BuilderKey"]);
            Assert.Equal("SourceValue", result.Value["SourceKey"]);
        }

        #endregion

        #region WithAdditionalConfigurationsFrom Tests

        [Fact]
        public void WithAdditionalConfigurationsFrom_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "configuration:TestKey", "TestValue" }
                })
                .Build();

            // Act
            var extensionResult = result.WithAdditionalConfigurationsFrom(config);

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithAdditionalConfigurationsFrom_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder>("Initial failure");
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

            // Act
            var extensionResult = failedResult.WithAdditionalConfigurationsFrom(config);

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithAdditionalConfigurationsFrom_WithValidConfiguration_ShouldExtractAndMerge()
        {
            // Arrange
            var externalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "configuration:Database", "Server=localhost" }
                })
                .Build();

            // Act
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithAdditionalConfigurationsFrom(externalConfig)
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Server=localhost", result.Value["Database"]);
        }

        #endregion

        #region Build Tests

        [Fact]
        public void Build_WithSuccessResult_ShouldReturnConfiguration()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew();

            // Act
            var buildResult = result.Build();

            // Assert
            Assert.True(buildResult.IsSuccess);
            Assert.NotNull(buildResult.Value);
        }

        [Fact]
        public void Build_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder>("Initial failure");

            // Act
            var buildResult = failedResult.Build();

            // Assert
            Assert.True(buildResult.IsFailed);
            Assert.Contains(buildResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void Build_WithConfiguration_ShouldReturnValidConfiguration()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key\":\"Value\"}")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value", result.Value["Key"]);
        }

        #endregion

        #region WithValidators Tests (Generic)

        [Fact]
        public void Generic_WithValidators_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>.CreateNew();
            var validator = new PassingValidator();

            // Act
            var extensionResult = result.WithValidators(validator);

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void Generic_WithValidators_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>>("Initial failure");
            var validator = new PassingValidator();

            // Act
            var extensionResult = failedResult.WithValidators(validator);

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void Generic_WithValidators_WithMultipleValidators_ShouldAccumulate()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>.CreateNew();
            var validator1 = new PassingValidator();
            var validator2 = new PassingValidator();

            // Act
            var extensionResult = result.WithValidators(validator1, validator2);

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        #endregion

        #region Build Tests (Generic)

        [Fact]
        public void Generic_Build_WithSuccessResult_ShouldReturnTypedConfiguration()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>.CreateNew();

            // Act
            var buildResult = result.Build();

            // Assert
            Assert.True(buildResult.IsSuccess);
            Assert.NotNull(buildResult.Value);
            Assert.IsType<TestConfiguration>(buildResult.Value);
        }

        [Fact]
        public void Generic_Build_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>>("Initial failure");

            // Act
            var buildResult = failedResult.Build();

            // Assert
            Assert.True(buildResult.IsFailed);
            Assert.Contains(buildResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void Generic_Build_WithValidJson_ShouldPopulateConfiguration()
        {
            // Arrange
            var builder = global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>.CreateNew().Value;

            // Act
            var result = builder.WithRawJsonStrings("{\"TestValue\":\"TestData\",\"TestNumber\":42}")
                .Bind(b => ((global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>)b).Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("TestData", result.Value.TestValue);
            Assert.Equal(42, result.Value.TestNumber);
        }

        [Fact]
        public void Generic_Build_WithValidators_ShouldExecuteValidation()
        {
            // Arrange
            var validator = new FailingValidator();
            var builder = global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>.CreateNew().Value;

            // Act
            var result = builder.WithRawJsonStrings("{\"TestValue\":\"TestData\"}")
                .Bind(b => ((global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>)b).WithValidators(validator))
                .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("Validation failed"));
        }

        #endregion

        #region Fluent Integration Tests

        [Fact]
        public void FluentChain_WithAllExtensions_ShouldWorkCorrectly()
        {
            // Arrange
            var additionalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "AdditionalKey", "AdditionalValue" }
                })
                .Build();

            var configSource = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"SourceKey\":\"SourceValue\"}")
                .Build()
                .Value;

            // Act - Skip JsonUri, EmbeddedResources, and LocalSecrets which may fail during build
            var result = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key1\":\"Value1\"}")
                .WithAdditionalConfigurations(additionalConfig)
                .WithAdditionalConfigurationSources(configSource)
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value1", result.Value["Key1"]);
            Assert.Equal("AdditionalValue", result.Value["AdditionalKey"]);
            Assert.Equal("SourceValue", result.Value["SourceKey"]);
        }

        [Fact]
        public void FluentChain_Generic_WithAllExtensions_ShouldWorkCorrectly()
        {
            // Arrange
            var validator = new PassingValidator();
            var builder = global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>.CreateNew().Value;

            // Act
            var result = builder.WithRawJsonStrings("{\"TestValue\":\"FluentTest\",\"TestNumber\":99}")
                .Bind(b => ((global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>)b).WithValidators(validator))
                .Bind(b => ((global::Baubit.Configuration.ConfigurationBuilder<TestConfiguration>)b).Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("FluentTest", result.Value.TestValue);
            Assert.Equal(99, result.Value.TestNumber);
        }

        [Fact]
        public void FluentChain_WithFailureInMiddle_ShouldPropagateFailure()
        {
            // Arrange
            var failingBuilder = Result.Fail<global::Baubit.Configuration.ConfigurationBuilder>("Middle failure");

            // Act
            var result = failingBuilder
                .WithRawJsonStrings("{\"Key\":\"Value\"}")
                .WithJsonUriStrings("https://example.com/config.json")
                .Build();

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("Middle failure"));
        }

        [Fact]
        public void FluentChain_ShortCircuitOnFailure_ShouldNotExecuteLaterSteps()
        {
            // Arrange
            var executionCount = 0;
            var builder = global::Baubit.Configuration.ConfigurationBuilder.CreateNew().Value;
            
            // Force a failure by disposing
            builder.Dispose();
            var disposedBuilder = Result.Ok(builder);
            var failedResult = disposedBuilder.WithRawJsonStrings("{\"Key\":\"Value\"}");

            // Act - This should not execute the lambda because the result is already failed
            var result = failedResult.Bind(b =>
            {
                executionCount++;
                return Result.Ok(b);
            });

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(0, executionCount); // Lambda should not have executed
        }

        #endregion

        #region Error Propagation Tests

        [Fact]
        public void AllExtensions_WithDisposedBuilder_ShouldReturnConfigurationBuilderDisposed()
        {
            // Arrange
            var builder = global::Baubit.Configuration.ConfigurationBuilder.CreateNew().Value;
            builder.Dispose();
            var disposedResult = Result.Ok(builder);

            // Act
            var rawJsonResult = disposedResult.WithRawJsonStrings("{\"Key\":\"Value\"}");
            var jsonUriResult = disposedResult.WithJsonUriStrings("https://example.com/config.json");
            var embeddedResourceResult = disposedResult.WithEmbeddedJsonResources("MyApp;Config.json");
            var localSecretResult = disposedResult.WithLocalSecrets("MyApp.Secrets");

            // Assert
            Assert.True(rawJsonResult.IsFailed);
            Assert.True(jsonUriResult.IsFailed);
            Assert.True(embeddedResourceResult.IsFailed);
            Assert.True(localSecretResult.IsFailed);
            
            Assert.Contains(rawJsonResult.Reasons, r => r is ConfigurationBuilderDisposed);
            Assert.Contains(jsonUriResult.Reasons, r => r is ConfigurationBuilderDisposed);
            Assert.Contains(embeddedResourceResult.Reasons, r => r is ConfigurationBuilderDisposed);
            Assert.Contains(localSecretResult.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        #endregion
    }
}
