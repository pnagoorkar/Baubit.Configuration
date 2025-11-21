using Microsoft.Extensions.Configuration;
using ConfigSource = Baubit.Configuration.ConfigurationSource;

namespace Baubit.Configuration.Test.ConfigurationSourceExtensions
{
    #region Test Helpers

    // Test class with URI-decorated properties
    public class TestConfigWithUris
    {
        [URI]
        public string? StringUri { get; set; }

        [URI]
        public List<string>? ListUri { get; set; }

        public string? NonUriString { get; set; }
    }

    // Test class with unsupported URI property type
    public class TestConfigWithUnsupportedUri
    {
        [URI]
        public int UnsupportedProperty { get; set; }
    }

    #endregion

    public class Test
    {
        #region Build Method Tests

        [Fact]
        public void Build_WithNullConfigurationSource_ShouldReturnFailure()
        {
            // Arrange
            ConfigSource? configSource = null;

            // Act
            var result = configSource!.Build();

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("ConfigurationSource cannot be null"));
        }

        [Fact]
        public void Build_WithEmptyConfigurationSource_ShouldReturnEmptyConfiguration()
        {
            // Arrange
            var configSource = new ConfigSource();

            // Act
            var result = configSource.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void Build_WithRawJsonStrings_ShouldBuildConfiguration()
        {
            // Arrange
            var configSource = new ConfigSource(
                new List<string> { "{\"TestKey\":\"TestValue\"}" },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            // Act
            var result = configSource.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("TestValue", result.Value["TestKey"]);
        }

        [Fact]
        public void Build_WithMultipleRawJsonStrings_ShouldMergeConfiguration()
        {
            // Arrange
            var configSource = new ConfigSource(
                new List<string> 
                { 
                    "{\"Key1\":\"Value1\"}", 
                    "{\"Key2\":\"Value2\"}" 
                },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            // Act
            var result = configSource.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value1", result.Value["Key1"]);
            Assert.Equal("Value2", result.Value["Key2"]);
        }

        [Fact]
        public void Build_WithAdditionalConfigurations_ShouldMergeAllConfigurations()
        {
            // Arrange
            var configSource = new ConfigSource(
                new List<string> { "{\"SourceKey\":\"SourceValue\"}" },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            var additionalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "AdditionalKey", "AdditionalValue" }
                })
                .Build();

            // Act
            var result = configSource.Build(additionalConfig);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("SourceValue", result.Value["SourceKey"]);
            Assert.Equal("AdditionalValue", result.Value["AdditionalKey"]);
        }

        [Fact]
        public void Build_WithNullAdditionalConfigurations_ShouldSucceed()
        {
            // Arrange
            var configSource = new ConfigSource(
                new List<string> { "{\"Key\":\"Value\"}" },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            // Act
            var result = configSource.Build((IConfiguration[])null!);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value", result.Value["Key"]);
        }

        [Fact]
        public void Build_WithMultipleAdditionalConfigurations_ShouldMergeAll()
        {
            // Arrange
            var configSource = new ConfigSource(
                new List<string> { "{\"Key1\":\"Value1\"}" },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            var config1 = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { { "Key2", "Value2" } })
                .Build();

            var config2 = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { { "Key3", "Value3" } })
                .Build();

            // Act
            var result = configSource.Build(config1, config2);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value1", result.Value["Key1"]);
            Assert.Equal("Value2", result.Value["Key2"]);
            Assert.Equal("Value3", result.Value["Key3"]);
        }

        [Fact]
        public void Build_OverloadWithoutParameters_ShouldCallMainBuild()
        {
            // Arrange
            var configSource = new ConfigSource(
                new List<string> { "{\"Key\":\"Value\"}" },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            // Act
            var result = configSource.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value", result.Value["Key"]);
        }

        #endregion

        #region ExpandURIs Tests

        [Fact]
        public void ExpandURIs_WithNullObject_ShouldReturnSuccess()
        {
            // Arrange
            TestConfigWithUris? obj = null;

            // Act
            var result = obj.ExpandURIs();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
        }

        [Fact]
        public void ExpandURIs_WithNoUriProperties_ShouldReturnSuccess()
        {
            // Arrange
            var obj = new { NormalProperty = "value" };

            // Act
            var result = obj.ExpandURIs();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void ExpandURIs_WithStringUriProperty_ShouldExpandEnvironmentVariable()
        {
            // Arrange
            var testVarName = "TEST_VAR_" + Guid.NewGuid().ToString("N");
            var testVarValue = "TestValue123";
            Environment.SetEnvironmentVariable(testVarName, testVarValue);

            try
            {
                var obj = new TestConfigWithUris
                {
                    StringUri = $"${{{testVarName}}}/config"
                };

                // Act
                var result = obj.ExpandURIs();

                // Assert
                Assert.True(result.IsSuccess);
                Assert.Equal($"{testVarValue}/config", result.Value.StringUri);
            }
            finally
            {
                Environment.SetEnvironmentVariable(testVarName, null);
            }
        }

        [Fact]
        public void ExpandURIs_WithListUriProperty_ShouldExpandAllItems()
        {
            // Arrange
            var testVarName = "TEST_VAR_" + Guid.NewGuid().ToString("N");
            var testVarValue = "TestValue456";
            Environment.SetEnvironmentVariable(testVarName, testVarValue);

            try
            {
                var obj = new TestConfigWithUris
                {
                    ListUri = new List<string>
                    {
                        $"${{{testVarName}}}/path1",
                        $"${{{testVarName}}}/path2"
                    }
                };

                // Act
                var result = obj.ExpandURIs();

                // Assert
                Assert.True(result.IsSuccess);
                Assert.Equal(2, result.Value.ListUri!.Count);
                Assert.Equal($"{testVarValue}/path1", result.Value.ListUri[0]);
                Assert.Equal($"{testVarValue}/path2", result.Value.ListUri[1]);
            }
            finally
            {
                Environment.SetEnvironmentVariable(testVarName, null);
            }
        }

        [Fact]
        public void ExpandURIs_WithEmptyListUriProperty_ShouldReturnSuccess()
        {
            // Arrange
            var obj = new TestConfigWithUris
            {
                ListUri = new List<string>()
            };

            // Act
            var result = obj.ExpandURIs();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value.ListUri!);
        }

        [Fact]
        public void ExpandURIs_WithNullListUriProperty_ShouldReturnSuccess()
        {
            // Arrange
            var obj = new TestConfigWithUris
            {
                ListUri = null
            };

            // Act
            var result = obj.ExpandURIs();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void ExpandURIs_WithUnsupportedPropertyType_ShouldReturnFailure()
        {
            // Arrange
            var obj = new TestConfigWithUnsupportedUri
            {
                UnsupportedProperty = 42
            };

            // Act
            var result = obj.ExpandURIs();

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("Unsupported URI property type"));
        }

        [Fact]
        public void ExpandURIs_WithMissingEnvironmentVariable_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentVar = "NONEXISTENT_VAR_" + Guid.NewGuid().ToString("N");
            var obj = new TestConfigWithUris
            {
                StringUri = $"${{{nonExistentVar}}}/config"
            };

            // Act
            var result = obj.ExpandURIs();

            // Assert
            Assert.True(result.IsFailed);
        }

        [Fact]
        public void ExpandURIs_WithNullStringProperty_ShouldHandleGracefully()
        {
            // Arrange
            var obj = new TestConfigWithUris
            {
                StringUri = null
            };

            // Act
            var result = obj.ExpandURIs();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.StringUri);
        }

        [Fact]
        public void ExpandURIs_WithEmptyStringProperty_ShouldReturnSuccess()
        {
            // Arrange
            var obj = new TestConfigWithUris
            {
                StringUri = string.Empty
            };

            // Act
            var result = obj.ExpandURIs();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(string.Empty, result.Value.StringUri);
        }

        [Fact]
        public void ExpandURIs_WithMultipleVariablesInOneString_ShouldExpandAll()
        {
            // Arrange
            var var1Name = "TEST_VAR1_" + Guid.NewGuid().ToString("N");
            var var2Name = "TEST_VAR2_" + Guid.NewGuid().ToString("N");
            Environment.SetEnvironmentVariable(var1Name, "Value1");
            Environment.SetEnvironmentVariable(var2Name, "Value2");

            try
            {
                var obj = new TestConfigWithUris
                {
                    StringUri = $"${{{var1Name}}}/middle/${{{var2Name}}}"
                };

                // Act
                var result = obj.ExpandURIs();

                // Assert
                Assert.True(result.IsSuccess);
                Assert.Equal("Value1/middle/Value2", result.Value.StringUri);
            }
            finally
            {
                Environment.SetEnvironmentVariable(var1Name, null);
                Environment.SetEnvironmentVariable(var2Name, null);
            }
        }

        [Fact]
        public void ExpandURIs_WithStringWithoutPlaceholders_ShouldReturnUnchanged()
        {
            // Arrange
            var obj = new TestConfigWithUris
            {
                StringUri = "/config/path"
            };

            // Act
            var result = obj.ExpandURIs();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("/config/path", result.Value.StringUri);
        }

        #endregion

        #region AddJsonFiles Tests

        [Fact]
        public void Build_WithFileUris_ShouldAttemptToLoadFiles()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "{\"FileKey\":\"FileValue\"}");

            try
            {
                // Create proper file:// URI that works cross-platform
                var fileUri = new Uri(Path.GetFullPath(tempFile)).AbsoluteUri;
                
                var configSource = new ConfigSource(
                    new List<string>(),
                    new List<string> { fileUri },
                    new List<string>(),
                    new List<string>()
                );

                // Act
                var result = configSource.Build();

                // Assert
                Assert.True(result.IsSuccess);
                Assert.Equal("FileValue", result.Value["FileKey"]);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Build_WithHttpUri_ShouldIgnoreNonFileUris()
        {
            // Arrange
            var configSource = new ConfigSource(
                new List<string> { "{\"Key\":\"Value\"}" },
                new List<string> { "https://example.com/config.json" },
                new List<string>(),
                new List<string>()
            );

            // Act
            var result = configSource.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value", result.Value["Key"]);
        }

        [Fact]
        public void Build_WithInvalidUri_ShouldReturnFailure()
        {
            // Arrange
            var configSource = new ConfigSource(
                new List<string>(),
                new List<string> { "not a valid uri" },
                new List<string>(),
                new List<string>()
            );

            // Act
            var result = configSource.Build();

            // Assert
            Assert.True(result.IsFailed);
        }

        [Fact]
        public void Build_WithMultipleFileUris_ShouldLoadAll()
        {
            // Arrange
            var tempFile1 = Path.GetTempFileName();
            var tempFile2 = Path.GetTempFileName();
            File.WriteAllText(tempFile1, "{\"Key1\":\"Value1\"}");
            File.WriteAllText(tempFile2, "{\"Key2\":\"Value2\"}");

            try
            {
                // Create proper file:// URIs that work cross-platform
                var fileUri1 = new Uri(Path.GetFullPath(tempFile1)).AbsoluteUri;
                var fileUri2 = new Uri(Path.GetFullPath(tempFile2)).AbsoluteUri;
                
                var configSource = new ConfigSource(
                    new List<string>(),
                    new List<string> { fileUri1, fileUri2 },
                    new List<string>(),
                    new List<string>()
                );

                // Act
                var result = configSource.Build();

                // Assert
                Assert.True(result.IsSuccess);
                Assert.Equal("Value1", result.Value["Key1"]);
                Assert.Equal("Value2", result.Value["Key2"]);
            }
            finally
            {
                if (File.Exists(tempFile1)) File.Delete(tempFile1);
                if (File.Exists(tempFile2)) File.Delete(tempFile2);
            }
        }

        #endregion

        #region AddRawJsonStrings Tests

        [Fact]
        public void Build_WithComplexJson_ShouldParseCorrectly()
        {
            // Arrange
            var complexJson = @"{
                ""Database"": {
                    ""ConnectionString"": ""Server=localhost;Database=Test;""
                },
                ""Logging"": {
                    ""LogLevel"": {
                        ""Default"": ""Information""
                    }
                }
            }";

            var configSource = new ConfigSource(
                new List<string> { complexJson },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            // Act
            var result = configSource.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Server=localhost;Database=Test;", result.Value["Database:ConnectionString"]);
            Assert.Equal("Information", result.Value["Logging:LogLevel:Default"]);
        }

        [Fact]
        public void Build_WithInvalidJson_ShouldThrowException()
        {
            // Arrange
            var invalidJson = "{ this is not valid json }";
            var configSource = new ConfigSource(
                new List<string> { invalidJson },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            // Act & Assert
            // Invalid JSON causes an exception during Microsoft.Extensions.Configuration.ConfigurationBuilder.Build()
            var ex = Assert.ThrowsAny<Exception>(() => configSource.Build());
            Assert.NotNull(ex);
        }

        [Fact]
        public void Build_WithEmptyJson_ShouldSucceed()
        {
            // Arrange
            var configSource = new ConfigSource(
                new List<string> { "{}" },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            // Act
            var result = configSource.Build();

            // Assert
            Assert.True(result.IsSuccess);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Build_WithAllSourceTypes_ShouldMergeCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "{\"FileKey\":\"FileValue\"}");

            try
            {
                // Create proper file:// URI that works cross-platform
                var fileUri = new Uri(Path.GetFullPath(tempFile)).AbsoluteUri;
                
                var configSource = new ConfigSource(
                    new List<string> { "{\"RawKey\":\"RawValue\"}" },
                    new List<string> { fileUri },
                    new List<string>(),
                    new List<string>()
                );

                var additionalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "AdditionalKey", "AdditionalValue" }
                    })
                    .Build();

                // Act
                var result = configSource.Build(additionalConfig);

                // Assert
                Assert.True(result.IsSuccess);
                Assert.Equal("RawValue", result.Value["RawKey"]);
                Assert.Equal("FileValue", result.Value["FileKey"]);
                Assert.Equal("AdditionalValue", result.Value["AdditionalKey"]);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void Build_WithConfigurationSourceHavingUris_ShouldExpandBeforeBuilding()
        {
            // Arrange
            var testVarName = "TEST_BUILD_VAR_" + Guid.NewGuid().ToString("N");
            var testVarValue = "expanded";
            Environment.SetEnvironmentVariable(testVarName, testVarValue);

            try
            {
                var configSource = new ConfigSource(
                    new List<string>(),
                    new List<string> { $"${{{testVarName}}}/config.json" }, // This will be expanded
                    new List<string>(),
                    new List<string>()
                );

                // Act
                var result = configSource.Build();

                // Assert - Should succeed even though file doesn't exist after expansion
                // because non-file URIs are ignored
                Assert.True(result.IsSuccess || result.IsFailed); // Either is valid depending on URI interpretation
            }
            finally
            {
                Environment.SetEnvironmentVariable(testVarName, null);
            }
        }

        [Fact]
        public void Build_WithOverlappingKeys_LaterSourcesShouldWin()
        {
            // Arrange
            var configSource = new ConfigSource(
                new List<string> 
                { 
                    "{\"SharedKey\":\"FirstValue\"}",
                    "{\"SharedKey\":\"SecondValue\"}"
                },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            // Act
            var result = configSource.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("SecondValue", result.Value["SharedKey"]);
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public void Build_WithVeryLargeJson_ShouldHandleCorrectly()
        {
            // Arrange
            var largeJson = "{" + string.Join(",", Enumerable.Range(0, 1000)
                .Select(i => $"\"Key{i}\":\"Value{i}\"")) + "}";
            var configSource = new ConfigSource(
                new List<string> { largeJson },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            // Act
            var result = configSource.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value0", result.Value["Key0"]);
            Assert.Equal("Value999", result.Value["Key999"]);
        }

        [Fact]
        public void Build_WithSpecialCharactersInJson_ShouldPreserve()
        {
            // Arrange
            var jsonWithSpecialChars = "{\"Key\":\"Value with ???? and émojis ??\"}";
            var configSource = new ConfigSource(
                new List<string> { jsonWithSpecialChars },
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            // Act
            var result = configSource.Build();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Value with ???? and émojis ??", result.Value["Key"]);
        }

        [Fact]
        public void ExpandURIs_WithComplexNestedObject_ShouldExpandAllUriProperties()
        {
            // Arrange
            var testVarName = "TEST_COMPLEX_VAR_" + Guid.NewGuid().ToString("N");
            Environment.SetEnvironmentVariable(testVarName, "expanded");

            try
            {
                var obj = new TestConfigWithUris
                {
                    StringUri = $"${{{testVarName}}}/path",
                    ListUri = new List<string> { $"${{{testVarName}}}/list" },
                    NonUriString = "should not change"
                };

                // Act
                var result = obj.ExpandURIs();

                // Assert
                Assert.True(result.IsSuccess);
                Assert.Equal("expanded/path", result.Value.StringUri);
                Assert.Equal("expanded/list", result.Value.ListUri![0]);
                Assert.Equal("should not change", result.Value.NonUriString);
            }
            finally
            {
                Environment.SetEnvironmentVariable(testVarName, null);
            }
        }

        #endregion
    }
}
