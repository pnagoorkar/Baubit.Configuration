using Baubit.Configuration.Traceability;

namespace Baubit.Configuration.Test.ConfigurationSourceBuilder
{
    public class Test
    {
        #region CreateNew Tests
        
        [Fact]
        public void CreateNew_ShouldReturnSuccessResult()
        {
            // Act
            var result = Configuration.ConfigurationSourceBuilder.CreateNew();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void CreateNew_ShouldReturnNewInstance()
        {
            // Act
            var result1 = Configuration.ConfigurationSourceBuilder.CreateNew();
            var result2 = Configuration.ConfigurationSourceBuilder.CreateNew();

            // Assert
            Assert.NotSame(result1.Value, result2.Value);
        }

        #endregion

        #region BuildEmpty Tests

        [Fact]
        public void BuildEmpty_ShouldReturnSuccessResult()
        {
            // Act
            var result = Configuration.ConfigurationSourceBuilder.BuildEmpty();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void BuildEmpty_ShouldReturnEmptyConfigurationSource()
        {
            // Act
            var result = Configuration.ConfigurationSourceBuilder.BuildEmpty();

            // Assert
            Assert.Empty(result.Value.RawJsonStrings);
            Assert.Empty(result.Value.JsonUriStrings);
            Assert.Empty(result.Value.EmbeddedJsonResources);
            Assert.Empty(result.Value.LocalSecrets);
        }

        #endregion

        #region WithRawJsonStrings Tests

        [Fact]
        public void WithRawJsonStrings_ShouldAddSingleJsonString()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var result = builder.WithRawJsonStrings(jsonString)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.RawJsonStrings);
            Assert.Contains(jsonString, result.Value.RawJsonStrings);
        }

        [Fact]
        public void WithRawJsonStrings_ShouldAddMultipleJsonStrings()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var jsonString1 = "{\"key1\":\"value1\"}";
            var jsonString2 = "{\"key2\":\"value2\"}";
            var jsonString3 = "{\"key3\":\"value3\"}";

            // Act
            var result = builder.WithRawJsonStrings(jsonString1, jsonString2, jsonString3)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value.RawJsonStrings.Count);
            Assert.Contains(jsonString1, result.Value.RawJsonStrings);
            Assert.Contains(jsonString2, result.Value.RawJsonStrings);
            Assert.Contains(jsonString3, result.Value.RawJsonStrings);
        }

        [Fact]
        public void WithRawJsonStrings_CalledMultipleTimes_ShouldAccumulateValues()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var jsonString1 = "{\"key1\":\"value1\"}";
            var jsonString2 = "{\"key2\":\"value2\"}";

            // Act
            var result = builder.WithRawJsonStrings(jsonString1)
                               .Bind(b => b.WithRawJsonStrings(jsonString2))
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.RawJsonStrings.Count);
            Assert.Contains(jsonString1, result.Value.RawJsonStrings);
            Assert.Contains(jsonString2, result.Value.RawJsonStrings);
        }

        [Fact]
        public void WithRawJsonStrings_AfterDispose_ShouldReturnFailure()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            builder.Dispose();

            // Act
            var result = builder.WithRawJsonStrings("{\"key\":\"value\"}");

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        #endregion

        #region WithJsonUriStrings Tests

        [Fact]
        public void WithJsonUriStrings_ShouldAddSingleUri()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var uri = "https://example.com/config.json";

            // Act
            var result = builder.WithJsonUriStrings(uri)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.JsonUriStrings);
            Assert.Contains(uri, result.Value.JsonUriStrings);
        }

        [Fact]
        public void WithJsonUriStrings_ShouldAddMultipleUris()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var uri1 = "https://example.com/config1.json";
            var uri2 = "https://example.com/config2.json";
            var uri3 = "file:///path/to/config3.json";

            // Act
            var result = builder.WithJsonUriStrings(uri1, uri2, uri3)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value.JsonUriStrings.Count);
            Assert.Contains(uri1, result.Value.JsonUriStrings);
            Assert.Contains(uri2, result.Value.JsonUriStrings);
            Assert.Contains(uri3, result.Value.JsonUriStrings);
        }

        [Fact]
        public void WithJsonUriStrings_CalledMultipleTimes_ShouldAccumulateValues()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var uri1 = "https://example.com/config1.json";
            var uri2 = "https://example.com/config2.json";

            // Act
            var result = builder.WithJsonUriStrings(uri1)
                               .Bind(b => b.WithJsonUriStrings(uri2))
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.JsonUriStrings.Count);
            Assert.Contains(uri1, result.Value.JsonUriStrings);
            Assert.Contains(uri2, result.Value.JsonUriStrings);
        }

        [Fact]
        public void WithJsonUriStrings_AfterDispose_ShouldReturnFailure()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            builder.Dispose();

            // Act
            var result = builder.WithJsonUriStrings("https://example.com/config.json");

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        #endregion

        #region WithEmbeddedJsonResources Tests

        [Fact]
        public void WithEmbeddedJsonResources_ShouldAddSingleResource()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var resource = "MyApp.Config.appsettings.json";

            // Act
            var result = builder.WithEmbeddedJsonResources(resource)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.EmbeddedJsonResources);
            Assert.Contains(resource, result.Value.EmbeddedJsonResources);
        }

        [Fact]
        public void WithEmbeddedJsonResources_ShouldAddMultipleResources()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var resource1 = "MyApp.Config.appsettings.json";
            var resource2 = "MyApp.Config.appsettings.dev.json";
            var resource3 = "MyApp.Config.secrets.json";

            // Act
            var result = builder.WithEmbeddedJsonResources(resource1, resource2, resource3)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value.EmbeddedJsonResources.Count);
            Assert.Contains(resource1, result.Value.EmbeddedJsonResources);
            Assert.Contains(resource2, result.Value.EmbeddedJsonResources);
            Assert.Contains(resource3, result.Value.EmbeddedJsonResources);
        }

        [Fact]
        public void WithEmbeddedJsonResources_CalledMultipleTimes_ShouldAccumulateValues()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var resource1 = "MyApp.Config.appsettings.json";
            var resource2 = "MyApp.Config.appsettings.dev.json";

            // Act
            var result = builder.WithEmbeddedJsonResources(resource1)
                               .Bind(b => b.WithEmbeddedJsonResources(resource2))
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.EmbeddedJsonResources.Count);
            Assert.Contains(resource1, result.Value.EmbeddedJsonResources);
            Assert.Contains(resource2, result.Value.EmbeddedJsonResources);
        }

        [Fact]
        public void WithEmbeddedJsonResources_AfterDispose_ShouldReturnFailure()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            builder.Dispose();

            // Act
            var result = builder.WithEmbeddedJsonResources("MyApp.Config.appsettings.json");

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        #endregion

        #region WithLocalSecrets Tests

        [Fact]
        public void WithLocalSecrets_ShouldAddSingleSecret()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var secret = "MyApp.Secrets";

            // Act
            var result = builder.WithLocalSecrets(secret)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.LocalSecrets);
            Assert.Contains(secret, result.Value.LocalSecrets);
        }

        [Fact]
        public void WithLocalSecrets_ShouldAddMultipleSecrets()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var secret1 = "MyApp.Secrets.Dev";
            var secret2 = "MyApp.Secrets.Prod";
            var secret3 = "MyApp.Secrets.Test";

            // Act
            var result = builder.WithLocalSecrets(secret1, secret2, secret3)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value.LocalSecrets.Count);
            Assert.Contains(secret1, result.Value.LocalSecrets);
            Assert.Contains(secret2, result.Value.LocalSecrets);
            Assert.Contains(secret3, result.Value.LocalSecrets);
        }

        [Fact]
        public void WithLocalSecrets_CalledMultipleTimes_ShouldAccumulateValues()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var secret1 = "MyApp.Secrets.Dev";
            var secret2 = "MyApp.Secrets.Prod";

            // Act
            var result = builder.WithLocalSecrets(secret1)
                               .Bind(b => b.WithLocalSecrets(secret2))
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.LocalSecrets.Count);
            Assert.Contains(secret1, result.Value.LocalSecrets);
            Assert.Contains(secret2, result.Value.LocalSecrets);
        }

        [Fact]
        public void WithLocalSecrets_AfterDispose_ShouldReturnFailure()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            builder.Dispose();

            // Act
            var result = builder.WithLocalSecrets("MyApp.Secrets");

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        #endregion

        #region Build Tests

        [Fact]
        public void Build_WithAllConfigurationTypes_ShouldSucceed()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var rawJson = "{\"key\":\"value\"}";
            var uri = "https://example.com/config.json";
            var resource = "MyApp.Config.appsettings.json";
            var secret = "MyApp.Secrets";

            // Act
            var result = builder.WithRawJsonStrings(rawJson)
                               .Bind(b => b.WithJsonUriStrings(uri))
                               .Bind(b => b.WithEmbeddedJsonResources(resource))
                               .Bind(b => b.WithLocalSecrets(secret))
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.RawJsonStrings);
            Assert.Single(result.Value.JsonUriStrings);
            Assert.Single(result.Value.EmbeddedJsonResources);
            Assert.Single(result.Value.LocalSecrets);
            Assert.Contains(rawJson, result.Value.RawJsonStrings);
            Assert.Contains(uri, result.Value.JsonUriStrings);
            Assert.Contains(resource, result.Value.EmbeddedJsonResources);
            Assert.Contains(secret, result.Value.LocalSecrets);
        }

        [Fact]
        public void Build_WithNoConfiguration_ShouldReturnEmptyLists()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;

            // Act
            var result = builder.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value.RawJsonStrings);
            Assert.Empty(result.Value.JsonUriStrings);
            Assert.Empty(result.Value.EmbeddedJsonResources);
            Assert.Empty(result.Value.LocalSecrets);
        }

        [Fact]
        public void Build_AfterDispose_ShouldReturnFailure()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            builder.Dispose();

            // Act
            var result = builder.Build();

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        [Fact]
        public void Build_ShouldDisposeBuilderAutomatically()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;

            // Act
            var buildResult = builder.Build();
            var secondBuildResult = builder.Build();

            // Assert
            Assert.True(buildResult.IsSuccess);
            Assert.True(secondBuildResult.IsFailed);
            Assert.Contains(secondBuildResult.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        #endregion

        #region Dispose Tests

        [Fact]
        public void Dispose_ShouldAllowMultipleCalls()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;

            // Act & Assert (should not throw)
            builder.Dispose();
            builder.Dispose();
            builder.Dispose();
        }

        [Fact]
        public void Dispose_ShouldPreventFurtherOperations()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            builder.Dispose();

            // Act
            var rawJsonResult = builder.WithRawJsonStrings("{\"key\":\"value\"}");
            var jsonUriResult = builder.WithJsonUriStrings("https://example.com/config.json");
            var embeddedResourceResult = builder.WithEmbeddedJsonResources("MyApp.Config.appsettings.json");
            var localSecretResult = builder.WithLocalSecrets("MyApp.Secrets");
            var buildResult = builder.Build();

            // Assert
            Assert.True(rawJsonResult.IsFailed);
            Assert.True(jsonUriResult.IsFailed);
            Assert.True(embeddedResourceResult.IsFailed);
            Assert.True(localSecretResult.IsFailed);
            Assert.True(buildResult.IsFailed);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void FluentBuilder_ShouldWorkWithMethodChaining()
        {
            // Arrange & Act
            var result = Configuration.ConfigurationSourceBuilder.CreateNew()
                .Bind(b => b.WithRawJsonStrings("{\"key1\":\"value1\"}", "{\"key2\":\"value2\"}"))
                .Bind(b => b.WithJsonUriStrings("https://example.com/config1.json", "https://example.com/config2.json"))
                .Bind(b => b.WithEmbeddedJsonResources("MyApp.Config.appsettings.json"))
                .Bind(b => b.WithLocalSecrets("MyApp.Secrets"))
                .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.RawJsonStrings.Count);
            Assert.Equal(2, result.Value.JsonUriStrings.Count);
            Assert.Single(result.Value.EmbeddedJsonResources);
            Assert.Single(result.Value.LocalSecrets);
        }

        [Fact]
        public void Builder_WithEmptyArrays_ShouldSucceed()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;

            // Act
            var result = builder.WithRawJsonStrings()
                               .Bind(b => b.WithJsonUriStrings())
                               .Bind(b => b.WithEmbeddedJsonResources())
                               .Bind(b => b.WithLocalSecrets())
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value.RawJsonStrings);
            Assert.Empty(result.Value.JsonUriStrings);
            Assert.Empty(result.Value.EmbeddedJsonResources);
            Assert.Empty(result.Value.LocalSecrets);
        }

        [Fact]
        public void Builder_ConfigurationSourceShouldBeIndependent()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var json = "{\"key\":\"value\"}";

            // Act
            var result = builder.WithRawJsonStrings(json)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            
            // Modifying the result should not affect future builds
            result.Value.RawJsonStrings.Add("{\"another\":\"json\"}");
            
            // The built configuration should have its own copy
            Assert.Single(result.Value.RawJsonStrings.Where(s => s == json));
        }

        #endregion

        #region WithAdditionalConfigurationSources Tests

        [Fact]
        public void WithAdditionalConfigurationSources_WithSingleSource_ShouldMergeAll()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var existingSource = Configuration.ConfigurationSourceBuilder.CreateNew()
                .Bind(b => b.WithRawJsonStrings("{\"Key\":\"Value\"}"))
                .Bind(b => b.WithJsonUriStrings("file:///test.json"))
                .Bind(b => b.Build())
                .Value;

            // Act
            var result = builder.WithAdditionalConfigurationSources(existingSource)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.RawJsonStrings);
            Assert.Single(result.Value.JsonUriStrings);
            Assert.Contains("{\"Key\":\"Value\"}", result.Value.RawJsonStrings);
            Assert.Contains("file:///test.json", result.Value.JsonUriStrings);
        }

        [Fact]
        public void WithAdditionalConfigurationSources_WithMultipleSources_ShouldMergeAll()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var source1 = Configuration.ConfigurationSourceBuilder.CreateNew()
                .Bind(b => b.WithRawJsonStrings("{\"Key1\":\"Value1\"}"))
                .Bind(b => b.Build())
                .Value;
            var source2 = Configuration.ConfigurationSourceBuilder.CreateNew()
                .Bind(b => b.WithJsonUriStrings("file:///test.json"))
                .Bind(b => b.Build())
                .Value;

            // Act
            var result = builder.WithAdditionalConfigurationSources(source1, source2)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.RawJsonStrings);
            Assert.Single(result.Value.JsonUriStrings);
        }

        [Fact]
        public void WithAdditionalConfigurationSources_WithAllSourceTypes_ShouldMergeEverything()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var existingSource = Configuration.ConfigurationSourceBuilder.CreateNew()
                .Bind(b => b.WithRawJsonStrings("{\"Key\":\"Value\"}"))
                .Bind(b => b.WithJsonUriStrings("file:///test.json"))
                .Bind(b => b.WithEmbeddedJsonResources("TestApp;Config.json"))
                .Bind(b => b.WithLocalSecrets("TestSecret"))
                .Bind(b => b.Build())
                .Value;

            // Act
            var result = builder.WithAdditionalConfigurationSources(existingSource)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.RawJsonStrings);
            Assert.Single(result.Value.JsonUriStrings);
            Assert.Single(result.Value.EmbeddedJsonResources);
            Assert.Single(result.Value.LocalSecrets);
        }

        [Fact]
        public void WithAdditionalConfigurationSources_CombinedWithOtherMethods_ShouldAccumulate()
        {
            // Arrange
            var builder = Configuration.ConfigurationSourceBuilder.CreateNew().Value;
            var existingSource = Configuration.ConfigurationSourceBuilder.CreateNew()
                .Bind(b => b.WithRawJsonStrings("{\"Key1\":\"Value1\"}"))
                .Bind(b => b.Build())
                .Value;

            // Act
            var result = builder.WithRawJsonStrings("{\"Key2\":\"Value2\"}")
                               .Bind(b => b.WithAdditionalConfigurationSources(existingSource))
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.RawJsonStrings.Count);
            Assert.Contains("{\"Key1\":\"Value1\"}", result.Value.RawJsonStrings);
            Assert.Contains("{\"Key2\":\"Value2\"}", result.Value.RawJsonStrings);
        }

        #endregion
    }
}
