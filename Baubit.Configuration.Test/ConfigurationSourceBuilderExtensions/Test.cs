using Baubit.Configuration.Traceability;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Baubit.Configuration.Test.ConfigurationSourceBuilderExtensions
{
    public class Test
    {
        #region WithJsonUriStrings Tests

        [Fact]
        public void WithJsonUriStrings_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew();

            // Act
            var extensionResult = result.WithJsonUriStrings("https://example.com/config.json");

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithJsonUriStrings_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationSourceBuilder>("Initial failure");

            // Act
            var extensionResult = failedResult.WithJsonUriStrings("https://example.com/config.json");

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithJsonUriStrings_WithMultipleUris_ShouldAccumulate()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithJsonUriStrings("https://example.com/config1.json", "https://example.com/config2.json")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.JsonUriStrings.Count);
            Assert.Contains("https://example.com/config1.json", result.Value.JsonUriStrings);
            Assert.Contains("https://example.com/config2.json", result.Value.JsonUriStrings);
        }

        [Fact]
        public void WithJsonUriStrings_ChainedCalls_ShouldAccumulate()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithJsonUriStrings("https://example.com/config1.json")
                .WithJsonUriStrings("https://example.com/config2.json")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.JsonUriStrings.Count);
        }

        #endregion

        #region WithEmbeddedJsonResources Tests

        [Fact]
        public void WithEmbeddedJsonResources_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew();

            // Act
            var extensionResult = result.WithEmbeddedJsonResources("Baubit.Configuration.Test;TestResources.config1.json");

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithEmbeddedJsonResources_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationSourceBuilder>("Initial failure");

            // Act
            var extensionResult = failedResult.WithEmbeddedJsonResources("Baubit.Configuration.Test;TestResources.config1.json");

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithEmbeddedJsonResources_WithMultipleResources_ShouldAccumulateAndLoadConfigurations()
        {
            // Arrange & Act
            var sourceResult = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithEmbeddedJsonResources(
                    "Baubit.Configuration.Test;TestResources.config1.json",
                    "Baubit.Configuration.Test;TestResources.config2.json")
                .Build();

            // Assert - Verify the source was built successfully
            Assert.True(sourceResult.IsSuccess);
            Assert.Equal(2, sourceResult.Value.EmbeddedJsonResources.Count);
            Assert.Contains("Baubit.Configuration.Test;TestResources.config1.json", sourceResult.Value.EmbeddedJsonResources);
            Assert.Contains("Baubit.Configuration.Test;TestResources.config2.json", sourceResult.Value.EmbeddedJsonResources);

            // Build configuration from the source and verify values are loaded
            var configResult = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithAdditionalConfigurationSources(sourceResult.Value)
                .Build();

            Assert.True(configResult.IsSuccess);
            Assert.Equal("EmbeddedValue1", configResult.Value["EmbeddedKey1"]);
            Assert.Equal("42", configResult.Value["EmbeddedNumber"]);
            Assert.Equal("EmbeddedValue2", configResult.Value["EmbeddedKey2"]);
            Assert.Equal("True", configResult.Value["EmbeddedBoolean"]);
        }

        [Fact]
        public void WithEmbeddedJsonResources_ChainedCalls_ShouldAccumulateAndLoad()
        {
            // Arrange & Act
            var sourceResult = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithEmbeddedJsonResources("Baubit.Configuration.Test;TestResources.config1.json")
                .WithEmbeddedJsonResources("Baubit.Configuration.Test;TestResources.config2.json")
                .Build();

            // Assert - Verify the source contains both resources
            Assert.True(sourceResult.IsSuccess);
            Assert.Equal(2, sourceResult.Value.EmbeddedJsonResources.Count);

            // Build configuration and verify both resources were loaded
            var configResult = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithAdditionalConfigurationSources(sourceResult.Value)
                .Build();

            Assert.True(configResult.IsSuccess);
            Assert.Equal("EmbeddedValue1", configResult.Value["EmbeddedKey1"]);
            Assert.Equal("EmbeddedValue2", configResult.Value["EmbeddedKey2"]);
        }

        [Fact]
        public void WithEmbeddedJsonResources_SingleResource_ShouldLoadConfiguration()
        {
            // Arrange & Act
            var sourceResult = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithEmbeddedJsonResources("Baubit.Configuration.Test;TestResources.source-config.json")
                .Build();

            // Assert
            Assert.True(sourceResult.IsSuccess);
            Assert.Single(sourceResult.Value.EmbeddedJsonResources);

            // Build configuration and verify the resource was loaded
            var configResult = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithAdditionalConfigurationSources(sourceResult.Value)
                .Build();

            Assert.True(configResult.IsSuccess);
            Assert.Equal("SourceValue", configResult.Value["SourceKey"]);
            Assert.Equal("100", configResult.Value["SourceNumber"]);
        }

        #endregion

        #region WithLocalSecrets Tests

        [Fact]
        public void WithLocalSecrets_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew();

            // Act
            var extensionResult = result.WithLocalSecrets("MyApp.Secrets");

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithLocalSecrets_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationSourceBuilder>("Initial failure");

            // Act
            var extensionResult = failedResult.WithLocalSecrets("MyApp.Secrets");

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithLocalSecrets_WithMultipleSecrets_ShouldAccumulate()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithLocalSecrets("MyApp.Secrets.Dev", "MyApp.Secrets.Prod")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.LocalSecrets.Count);
            Assert.Contains("MyApp.Secrets.Dev", result.Value.LocalSecrets);
            Assert.Contains("MyApp.Secrets.Prod", result.Value.LocalSecrets);
        }

        [Fact]
        public void WithLocalSecrets_ChainedCalls_ShouldAccumulate()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithLocalSecrets("MyApp.Secrets.Dev")
                .WithLocalSecrets("MyApp.Secrets.Prod")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.LocalSecrets.Count);
        }

        #endregion

        #region WithRawJsonStrings Tests

        [Fact]
        public void WithRawJsonStrings_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew();

            // Act
            var extensionResult = result.WithRawJsonStrings("{\"Key\":\"Value\"}");

            // Assert
            Assert.True(extensionResult.IsSuccess);
        }

        [Fact]
        public void WithRawJsonStrings_WithFailedResult_ShouldPropagateFailure()
        {
            // Arrange
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationSourceBuilder>("Initial failure");

            // Act
            var extensionResult = failedResult.WithRawJsonStrings("{\"Key\":\"Value\"}");

            // Assert
            Assert.True(extensionResult.IsFailed);
            Assert.Contains(extensionResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void WithRawJsonStrings_WithMultipleJsonStrings_ShouldAccumulate()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key1\":\"Value1\"}", "{\"Key2\":\"Value2\"}")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.RawJsonStrings.Count);
            Assert.Contains("{\"Key1\":\"Value1\"}", result.Value.RawJsonStrings);
            Assert.Contains("{\"Key2\":\"Value2\"}", result.Value.RawJsonStrings);
        }

        [Fact]
        public void WithRawJsonStrings_ChainedCalls_ShouldAccumulate()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key1\":\"Value1\"}")
                .WithRawJsonStrings("{\"Key2\":\"Value2\"}")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.RawJsonStrings.Count);
        }

        #endregion

        #region WithAdditionalConfigurationSources Tests

        [Fact]
        public void WithAdditionalConfigurationSources_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew();
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
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationSourceBuilder>("Initial failure");
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
            var existingSource = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key1\":\"Value1\"}")
                .WithJsonUriStrings("file:///test.json")
                .Build()
                .Value;

            // Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key2\":\"Value2\"}")
                .WithAdditionalConfigurationSources(existingSource)
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.RawJsonStrings.Count);
            Assert.Single(result.Value.JsonUriStrings);
            Assert.Contains("{\"Key1\":\"Value1\"}", result.Value.RawJsonStrings);
            Assert.Contains("{\"Key2\":\"Value2\"}", result.Value.RawJsonStrings);
            Assert.Contains("file:///test.json", result.Value.JsonUriStrings);
        }

        [Fact]
        public void WithAdditionalConfigurationSources_WithMultipleSources_ShouldMergeAll()
        {
            // Arrange
            var source1 = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key1\":\"Value1\"}")
                .Build()
                .Value;
            var source2 = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithJsonUriStrings("file:///test.json")
                .Build()
                .Value;

            // Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithAdditionalConfigurationSources(source1, source2)
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.RawJsonStrings);
            Assert.Single(result.Value.JsonUriStrings);
        }

        #endregion

        #region WithAdditionalConfigurationSourcesFrom Tests

        [Fact]
        public void WithAdditionalConfigurationSourcesFrom_WithSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew();
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
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationSourceBuilder>("Initial failure");
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
                    { "configurationSource:RawJsonStrings:0", "{\"SourceKey\":\"SourceValue\"}" },
                    { "configurationSource:JsonUriStrings:0", "file:///external.json" }
                })
                .Build();

            // Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithAdditionalConfigurationSourcesFrom(externalConfig)
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.RawJsonStrings);
            Assert.Single(result.Value.JsonUriStrings);
            Assert.Contains("{\"SourceKey\":\"SourceValue\"}", result.Value.RawJsonStrings);
            Assert.Contains("file:///external.json", result.Value.JsonUriStrings);
        }

        [Fact]
        public void WithAdditionalConfigurationSourcesFrom_WithMissingSection_ShouldUseEmptySource()
        {
            // Arrange
            var configWithoutSection = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "SomeOtherKey", "SomeValue" }
                })
                .Build();

            // Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key\":\"Value\"}")
                .WithAdditionalConfigurationSourcesFrom(configWithoutSection)
                .Build();

            // Assert - Should succeed with only the original raw JSON string
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.RawJsonStrings);
            Assert.Contains("{\"Key\":\"Value\"}", result.Value.RawJsonStrings);
        }

        #endregion

        #region Build Tests

        [Fact]
        public void Build_WithSuccessResult_ShouldReturnConfigurationSource()
        {
            // Arrange
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew();

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
            var failedResult = Result.Fail<global::Baubit.Configuration.ConfigurationSourceBuilder>("Initial failure");

            // Act
            var buildResult = failedResult.Build();

            // Assert
            Assert.True(buildResult.IsFailed);
            Assert.Contains(buildResult.Errors, e => e.Message.Contains("Initial failure"));
        }

        [Fact]
        public void Build_WithAllSourceTypes_ShouldBuildAndLoadSuccessfully()
        {
            // Arrange & Act
            var sourceResult = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"RawKey\":\"RawValue\"}")
                .WithEmbeddedJsonResources("Baubit.Configuration.Test;TestResources.config1.json")
                .Build();

            // Assert - Verify source built successfully
            Assert.True(sourceResult.IsSuccess);
            Assert.Single(sourceResult.Value.RawJsonStrings);
            Assert.Single(sourceResult.Value.EmbeddedJsonResources);

            // Build configuration and verify all sources were loaded
            var configResult = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithAdditionalConfigurationSources(sourceResult.Value)
                .Build();

            Assert.True(configResult.IsSuccess);
            Assert.Equal("RawValue", configResult.Value["RawKey"]);
            Assert.Equal("EmbeddedValue1", configResult.Value["EmbeddedKey1"]);
            Assert.Equal("42", configResult.Value["EmbeddedNumber"]);
        }

        [Fact]
        public void Build_EmptyBuilder_ShouldReturnEmptyConfigurationSource()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value.RawJsonStrings);
            Assert.Empty(result.Value.JsonUriStrings);
            Assert.Empty(result.Value.EmbeddedJsonResources);
            Assert.Empty(result.Value.LocalSecrets);
        }

        #endregion

        #region Fluent Integration Tests

        [Fact]
        public void FluentChain_WithAllExtensions_ShouldWorkCorrectly()
        {
            // Arrange
            var existingSource = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"ExistingKey\":\"ExistingValue\"}")
                .Build()
                .Value;

            var externalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "configurationSource:EmbeddedJsonResources:0", "Baubit.Configuration.Test;TestResources.config2.json" }
                })
                .Build();

            // Act - Build source with all extension methods including real embedded resources
            var sourceResult = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key1\":\"Value1\"}")
                .WithEmbeddedJsonResources("Baubit.Configuration.Test;TestResources.config1.json")
                .WithAdditionalConfigurationSources(existingSource)
                .WithAdditionalConfigurationSourcesFrom(externalConfig)
                .Build();

            // Assert - Verify source was built successfully
            Assert.True(sourceResult.IsSuccess);
            Assert.Equal(2, sourceResult.Value.RawJsonStrings.Count); // Original + Existing
            Assert.Equal(2, sourceResult.Value.EmbeddedJsonResources.Count); // config1 + config2 from external
            Assert.Contains("{\"Key1\":\"Value1\"}", sourceResult.Value.RawJsonStrings);
            Assert.Contains("{\"ExistingKey\":\"ExistingValue\"}", sourceResult.Value.RawJsonStrings);

            // Build configuration from the source and verify embedded resources were loaded
            var configResult = global::Baubit.Configuration.ConfigurationBuilder.CreateNew()
                .WithAdditionalConfigurationSources(sourceResult.Value)
                .Build();

            Assert.True(configResult.IsSuccess);
            Assert.Equal("Value1", configResult.Value["Key1"]);
            Assert.Equal("ExistingValue", configResult.Value["ExistingKey"]);
            Assert.Equal("EmbeddedValue1", configResult.Value["EmbeddedKey1"]);
            Assert.Equal("42", configResult.Value["EmbeddedNumber"]);
            Assert.Equal("EmbeddedValue2", configResult.Value["EmbeddedKey2"]);
            Assert.Equal("True", configResult.Value["EmbeddedBoolean"]);
        }

        [Fact]
        public void FluentChain_WithFailureInMiddle_ShouldPropagateFailure()
        {
            // Arrange
            var failingBuilder = Result.Fail<global::Baubit.Configuration.ConfigurationSourceBuilder>("Middle failure");

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
            var builder = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            
            // Force a failure
            builder.Dispose();
            var failedResult = builder.WithRawJsonStrings("{\"Key\":\"Value\"}");

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
            var builder = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            builder.Dispose();
            var disposedResult = Result.Ok(builder);

            // Act
            var rawJsonResult = disposedResult.WithRawJsonStrings("{\"Key\":\"Value\"}");
            var jsonUriResult = disposedResult.WithJsonUriStrings("https://example.com/config.json");
            var embeddedResourceResult = disposedResult.WithEmbeddedJsonResources("MyApp;Config.json");
            var localSecretResult = disposedResult.WithLocalSecrets("MyApp.Secrets");
            var buildResult = disposedResult.Build();

            // Assert
            Assert.True(rawJsonResult.IsFailed);
            Assert.True(jsonUriResult.IsFailed);
            Assert.True(embeddedResourceResult.IsFailed);
            Assert.True(localSecretResult.IsFailed);
            Assert.True(buildResult.IsFailed);
            
            Assert.Contains(rawJsonResult.Reasons, r => r is ConfigurationBuilderDisposed);
            Assert.Contains(jsonUriResult.Reasons, r => r is ConfigurationBuilderDisposed);
            Assert.Contains(embeddedResourceResult.Reasons, r => r is ConfigurationBuilderDisposed);
            Assert.Contains(localSecretResult.Reasons, r => r is ConfigurationBuilderDisposed);
            Assert.Contains(buildResult.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        #endregion

        #region Chaining Multiple Operations Tests

        [Fact]
        public void MultipleOperations_ChainedInSequence_ShouldAccumulateCorrectly()
        {
            // Arrange & Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Key1\":\"Value1\"}")
                .WithRawJsonStrings("{\"Key2\":\"Value2\"}")
                .WithJsonUriStrings("https://example.com/config1.json")
                .WithJsonUriStrings("https://example.com/config2.json")
                .WithEmbeddedJsonResources("MyApp;Config1.json")
                .WithEmbeddedJsonResources("MyApp;Config2.json")
                .WithLocalSecrets("Secret1")
                .WithLocalSecrets("Secret2")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.RawJsonStrings.Count);
            Assert.Equal(2, result.Value.JsonUriStrings.Count);
            Assert.Equal(2, result.Value.EmbeddedJsonResources.Count);
            Assert.Equal(2, result.Value.LocalSecrets.Count);
        }

        [Fact]
        public void ComplexChain_WithAllFeatures_ShouldProduceCorrectResult()
        {
            // Arrange
            var baseSource1 = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Base1\":\"Value1\"}")
                .Build()
                .Value;

            var baseSource2 = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithJsonUriStrings("file:///base2.json")
                .Build()
                .Value;

            var externalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "configurationSource:EmbeddedJsonResources:0", "External;Config.json" }
                })
                .Build();

            // Act
            var result = global::Baubit.Configuration.ConfigurationSourceBuilder.CreateNew()
                .WithRawJsonStrings("{\"Main\":\"MainValue\"}")
                .WithAdditionalConfigurationSources(baseSource1)
                .WithAdditionalConfigurationSources(baseSource2)
                .WithAdditionalConfigurationSourcesFrom(externalConfig)
                .WithLocalSecrets("MainSecret")
                .Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.RawJsonStrings.Count); // Main + Base1
            Assert.Single(result.Value.JsonUriStrings); // Base2
            Assert.Single(result.Value.EmbeddedJsonResources); // External
            Assert.Single(result.Value.LocalSecrets); // Main
        }

        #endregion
    }
}
