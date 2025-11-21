using Baubit.Configuration.Traceability;
using Baubit.Validation;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Baubit.Configuration.Test.ConfigurationBuilder
{
    #region Test Helpers

    // Test configuration class
    public record TestConfiguration : AConfiguration
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

    // Test validator that checks specific condition
    public class ConditionalValidator : IValidator<TestConfiguration>
    {
        private readonly Func<TestConfiguration, bool> _condition;
        private readonly string _errorMessage;

        public ConditionalValidator(Func<TestConfiguration, bool> condition, string errorMessage)
        {
            _condition = condition;
            _errorMessage = errorMessage;
        }

        public Result Run(TestConfiguration validatable)
        {
            return _condition(validatable) ? Result.Ok() : Result.Fail(_errorMessage);
        }
    }

    #endregion

    public class Test
    {
        #region ConfigurationBuilder - CreateNew Tests

        [Fact]
        public void CreateNew_ShouldReturnSuccessResult()
        {
            // Act
            var result = Configuration.ConfigurationBuilder.CreateNew();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void CreateNew_ShouldReturnNewInstance()
        {
            // Act
            var result1 = Configuration.ConfigurationBuilder.CreateNew();
            var result2 = Configuration.ConfigurationBuilder.CreateNew();

            // Assert
            Assert.NotSame(result1.Value, result2.Value);
        }

        #endregion

        #region ConfigurationBuilder - WithRawJsonStrings Tests

        [Fact]
        public void WithRawJsonStrings_ShouldAddSingleJsonString()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var jsonString = "{\"TestValue\":\"value\"}";

            // Act
            var result = builder.WithRawJsonStrings(jsonString);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Same(builder, result.Value);
        }

        [Fact]
        public void WithRawJsonStrings_ShouldAddMultipleJsonStrings()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var json1 = "{\"Key1\":\"value1\"}";
            var json2 = "{\"Key2\":\"value2\"}";

            // Act
            var result = builder.WithRawJsonStrings(json1, json2);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Same(builder, result.Value);
        }

        [Fact]
        public void WithRawJsonStrings_CalledMultipleTimes_ShouldAccumulate()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var json1 = "{\"Key1\":\"value1\"}";
            var json2 = "{\"Key2\":\"value2\"}";

            // Act
            var result = builder.WithRawJsonStrings(json1)
                               .Bind(b => b.WithRawJsonStrings(json2));

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void WithRawJsonStrings_AfterDispose_ShouldFail()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            builder.Dispose();

            // Act
            var result = builder.WithRawJsonStrings("{\"Key\":\"value\"}");

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        #endregion

        #region ConfigurationBuilder - WithJsonUriStrings Tests

        [Fact]
        public void WithJsonUriStrings_ShouldAddSingleUri()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var uri = "https://example.com/config.json";

            // Act
            var result = builder.WithJsonUriStrings(uri);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Same(builder, result.Value);
        }

        [Fact]
        public void WithJsonUriStrings_ShouldAddMultipleUris()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var uri1 = "https://example.com/config1.json";
            var uri2 = "https://example.com/config2.json";

            // Act
            var result = builder.WithJsonUriStrings(uri1, uri2);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void WithJsonUriStrings_AfterDispose_ShouldFail()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            builder.Dispose();

            // Act
            var result = builder.WithJsonUriStrings("https://example.com/config.json");

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        #endregion

        #region ConfigurationBuilder - WithEmbeddedJsonResources Tests

        [Fact]
        public void WithEmbeddedJsonResources_ShouldAddSingleResource()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var resource = "MyApp;Config.appsettings.json";

            // Act
            var result = builder.WithEmbeddedJsonResources(resource);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Same(builder, result.Value);
        }

        [Fact]
        public void WithEmbeddedJsonResources_ShouldAddMultipleResources()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var resource1 = "MyApp;Config.appsettings.json";
            var resource2 = "MyApp;Config.appsettings.dev.json";

            // Act
            var result = builder.WithEmbeddedJsonResources(resource1, resource2);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void WithEmbeddedJsonResources_AfterDispose_ShouldFail()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            builder.Dispose();

            // Act
            var result = builder.WithEmbeddedJsonResources("MyApp;Config.appsettings.json");

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        #endregion

        #region ConfigurationBuilder - WithLocalSecrets Tests

        [Fact]
        public void WithLocalSecrets_ShouldAddSingleSecret()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var secret = "MyApp.Secrets";

            // Act
            var result = builder.WithLocalSecrets(secret);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Same(builder, result.Value);
        }

        [Fact]
        public void WithLocalSecrets_ShouldAddMultipleSecrets()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var secret1 = "MyApp.Secrets.Dev";
            var secret2 = "MyApp.Secrets.Prod";

            // Act
            var result = builder.WithLocalSecrets(secret1, secret2);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void WithLocalSecrets_AfterDispose_ShouldFail()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            builder.Dispose();

            // Act
            var result = builder.WithLocalSecrets("MyApp.Secrets");

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Reasons, r => r is ConfigurationBuilderDisposed);
        }

        #endregion

        #region ConfigurationBuilder - WithAdditionalConfigurations Tests

        [Fact]
        public void WithAdditionalConfigurations_ShouldAcceptSingleConfiguration()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

            // Act
            var result = builder.WithAdditionalConfigurations(config);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Same(builder, result.Value);
        }

        [Fact]
        public void WithAdditionalConfigurations_ShouldAcceptMultipleConfigurations()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var config1 = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
            var config2 = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

            // Act
            var result = builder.WithAdditionalConfigurations(config1, config2);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void WithAdditionalConfigurations_CalledMultipleTimes_ShouldAccumulate()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var config1 = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
            var config2 = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

            // Act
            var result = builder.WithAdditionalConfigurations(config1)
                               .Bind(b => b.WithAdditionalConfigurations(config2));

            // Assert
            Assert.True(result.IsSuccess);
        }

        #endregion

        #region ConfigurationBuilder - Build Tests

        [Fact]
        public void Build_WithEmptyBuilder_ShouldSucceed()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;

            // Act
            var result = builder.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void Build_WithRawJsonStrings_ShouldCreateConfiguration()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var json = "{\"TestKey\":\"TestValue\"}";

            // Act
            var result = builder.WithRawJsonStrings(json)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("TestValue", result.Value["TestKey"]);
        }

        [Fact]
        public void Build_WithMultipleJsonStrings_ShouldMergeConfiguration()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var json1 = "{\"Key1\":\"Value1\"}";
            var json2 = "{\"Key2\":\"Value2\"}";

            // Act
            var result = builder.WithRawJsonStrings(json1, json2)
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value1", result.Value["Key1"]);
            Assert.Equal("Value2", result.Value["Key2"]);
        }

        [Fact]
        public void Build_AfterDispose_ShouldFail()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            builder.Dispose();

            // Act & Assert
            // After disposal, _configurationSourceBuilder is null, so calling Build will throw
            // This is expected behavior - the builder enforces single use through disposal
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void Build_ShouldDisposeBuilderAutomatically()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;

            // Act
            var buildResult = builder.Build();
            
            // Assert
            Assert.True(buildResult.IsSuccess);
            
            // After building, the builder is disposed and _configurationSourceBuilder is null
            // Attempting to build again will throw
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void Build_WithAdditionalConfiguration_ShouldMergeConfigurations()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            var additionalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "AdditionalKey", "AdditionalValue" }
                })
                .Build();

            // Act
            var result = builder.WithRawJsonStrings("{\"MainKey\":\"MainValue\"}")
                               .Bind(b => b.WithAdditionalConfigurations(additionalConfig))
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("MainValue", result.Value["MainKey"]);
            Assert.Equal("AdditionalValue", result.Value["AdditionalKey"]);
        }

        #endregion

        #region ConfigurationBuilder - Dispose Tests

        [Fact]
        public void Dispose_ShouldAllowMultipleCalls()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;

            // Act & Assert (should not throw)
            builder.Dispose();
            builder.Dispose();
            builder.Dispose();
        }

        [Fact]
        public void Dispose_ShouldPreventAllOperations()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;
            builder.Dispose();

            // Act
            var rawJsonResult = builder.WithRawJsonStrings("{\"key\":\"value\"}");
            var jsonUriResult = builder.WithJsonUriStrings("https://example.com/config.json");
            var embeddedResourceResult = builder.WithEmbeddedJsonResources("MyApp;Config.json");
            var localSecretResult = builder.WithLocalSecrets("MyApp.Secrets");

            // Assert
            Assert.True(rawJsonResult.IsFailed);
            Assert.True(jsonUriResult.IsFailed);
            Assert.True(embeddedResourceResult.IsFailed);
            Assert.True(localSecretResult.IsFailed);
            
            // Build will throw ArgumentException because _configurationSourceBuilder is null after disposal
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        #endregion

        #region ConfigurationBuilder - Integration Tests

        [Fact]
        public void FluentBuilder_ShouldWorkWithMethodChaining()
        {
            // Arrange & Act
            var result = Configuration.ConfigurationBuilder.CreateNew()
                .Bind(b => b.WithRawJsonStrings("{\"Key1\":\"Value1\"}"))
                .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value1", result.Value["Key1"]);
        }

        [Fact]
        public void Builder_WithEmptyParameters_ShouldSucceed()
        {
            // Arrange
            var builder = Configuration.ConfigurationBuilder.CreateNew().Value;

            // Act
            var result = builder.WithRawJsonStrings()
                               .Bind(b => b.WithJsonUriStrings())
                               .Bind(b => b.WithEmbeddedJsonResources())
                               .Bind(b => b.WithLocalSecrets())
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
        }

        #endregion

        #region ConfigurationBuilder<T> - WithValidators Tests

        [Fact]
        public void Generic_WithValidators_ShouldAddSingleValidator()
        {
            // Arrange
            var builder = new Configuration.ConfigurationBuilder<TestConfiguration>();
            var validator = new PassingValidator();

            // Act
            var result = builder.WithValidators(validator);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Generic_WithValidators_ShouldAddMultipleValidators()
        {
            // Arrange
            var builder = new Configuration.ConfigurationBuilder<TestConfiguration>();
            var validator1 = new PassingValidator();
            var validator2 = new PassingValidator();

            // Act
            var result = builder.WithValidators(validator1, validator2);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Generic_WithValidators_CalledMultipleTimes_ShouldAccumulate()
        {
            // Arrange
            var builder = new Configuration.ConfigurationBuilder<TestConfiguration>();
            var validator1 = new PassingValidator();
            var validator2 = new PassingValidator();

            // Act
            var result = builder.WithValidators(validator1)
                               .Bind(b => b.WithValidators(validator2));

            // Assert
            Assert.True(result.IsSuccess);
        }

        #endregion

        #region ConfigurationBuilder<T> - Build Tests

        [Fact]
        public void Generic_Build_WithEmptyConfiguration_ShouldCreateDefaultInstance()
        {
            // Arrange
            var builder = new Configuration.ConfigurationBuilder<TestConfiguration>();

            // Act
            var result = builder.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.IsType<TestConfiguration>(result.Value);
        }

        [Fact]
        public void Generic_Build_WithJsonConfiguration_ShouldPopulateProperties()
        {
            // Arrange
            var builder = new Configuration.ConfigurationBuilder<TestConfiguration>();
            var json = "{\"TestValue\":\"TestData\",\"TestNumber\":42}";

            // Act
            var result = builder.WithRawJsonStrings(json)
                               .Bind(b => ((Configuration.ConfigurationBuilder<TestConfiguration>)b).Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("TestData", result.Value.TestValue);
            Assert.Equal(42, result.Value.TestNumber);
        }

        [Fact]
        public void Generic_Build_WithPassingValidator_ShouldSucceed()
        {
            // Arrange
            var builder = new Configuration.ConfigurationBuilder<TestConfiguration>();
            var validator = new PassingValidator();
            var json = "{\"TestValue\":\"TestData\"}";

            // Act
            var result = builder.WithRawJsonStrings(json)
                               .Bind(b => ((Configuration.ConfigurationBuilder<TestConfiguration>)b).WithValidators(validator))
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Generic_Build_WithFailingValidator_ShouldFail()
        {
            // Arrange
            var builder = new Configuration.ConfigurationBuilder<TestConfiguration>();
            var validator = new FailingValidator();
            var json = "{\"TestValue\":\"TestData\"}";

            // Act
            var result = builder.WithRawJsonStrings(json)
                               .Bind(b => ((Configuration.ConfigurationBuilder<TestConfiguration>)b).WithValidators(validator))
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("Validation failed"));
        }

        [Fact]
        public void Generic_Build_WithMultipleValidators_ShouldRunAllValidators()
        {
            // Arrange
            var builder = new Configuration.ConfigurationBuilder<TestConfiguration>();
            var validator1 = new ConditionalValidator(
                cfg => cfg.TestValue == "ValidValue",
                "TestValue must be 'ValidValue'"
            );
            var validator2 = new ConditionalValidator(
                cfg => cfg.TestNumber > 0,
                "TestNumber must be greater than 0"
            );
            var json = "{\"TestValue\":\"ValidValue\",\"TestNumber\":10}";

            // Act
            var result = builder.WithRawJsonStrings(json)
                               .Bind(b => ((Configuration.ConfigurationBuilder<TestConfiguration>)b).WithValidators(validator1, validator2))
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Generic_Build_WithMultipleValidators_FirstFails_ShouldFail()
        {
            // Arrange
            var builder = new Configuration.ConfigurationBuilder<TestConfiguration>();
            var validator1 = new ConditionalValidator(
                cfg => cfg.TestValue == "ValidValue",
                "TestValue must be 'ValidValue'"
            );
            var validator2 = new PassingValidator();
            var json = "{\"TestValue\":\"InvalidValue\",\"TestNumber\":10}";

            // Act
            var result = builder.WithRawJsonStrings(json)
                               .Bind(b => ((Configuration.ConfigurationBuilder<TestConfiguration>)b).WithValidators(validator1, validator2))
                               .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("TestValue must be 'ValidValue'"));
        }

        [Fact]
        public void Generic_Build_InheritsBaseBuilderMethods()
        {
            // Arrange
            var builder = new Configuration.ConfigurationBuilder<TestConfiguration>();
            var json = "{\"TestValue\":\"InheritedTest\"}";

            // Act
            var result = builder.WithRawJsonStrings(json)
                               .Bind(b => builder.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("InheritedTest", result.Value.TestValue);
        }

        [Fact]
        public void Generic_Build_ShouldDisposeBuilderAutomatically()
        {
            // Arrange
            var builder = new Configuration.ConfigurationBuilder<TestConfiguration>();

            // Act
            var buildResult = builder.Build();

            // Assert
            Assert.True(buildResult.IsSuccess);
            
            // After building, attempting to build again will throw because the builder is disposed
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        #endregion

        #region ConfigurationBuilder<T> - Integration Tests

        [Fact]
        public void Generic_FluentBuilder_ShouldSupportFullChaining()
        {
            // Arrange
            var validator = new ConditionalValidator(
                cfg => !string.IsNullOrEmpty(cfg.TestValue),
                "TestValue cannot be empty"
            );
            var json = "{\"TestValue\":\"FluentTest\",\"TestNumber\":99}";

            // Act
            var result = new Configuration.ConfigurationBuilder<TestConfiguration>()
                .WithRawJsonStrings(json)
                .Bind(b => ((Configuration.ConfigurationBuilder<TestConfiguration>)b).WithValidators(validator))
                .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("FluentTest", result.Value.TestValue);
            Assert.Equal(99, result.Value.TestNumber);
        }

        [Fact]
        public void Generic_Build_WithComplexConfiguration_ShouldHandleAllFeatures()
        {
            // Arrange
            var additionalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "TestNumber", "100" }
                })
                .Build();

            var validator = new ConditionalValidator(
                cfg => cfg.TestNumber >= 100,
                "TestNumber must be at least 100"
            );

            var json = "{\"TestValue\":\"ComplexTest\"}";

            // Act
            var result = new Configuration.ConfigurationBuilder<TestConfiguration>()
                .WithRawJsonStrings(json)
                .Bind(b => b.WithAdditionalConfigurations(additionalConfig))
                .Bind(b => ((Configuration.ConfigurationBuilder<TestConfiguration>)b).WithValidators(validator))
                .Bind(b => b.Build());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("ComplexTest", result.Value.TestValue);
            Assert.Equal(100, result.Value.TestNumber);
        }

        #endregion
    }
}
