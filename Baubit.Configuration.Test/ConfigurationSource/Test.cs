namespace Baubit.Configuration.Test.ConfigurationSource
{
    public class Test
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithAllParameters_ShouldInitializeAllProperties()
        {
            // Arrange
            var rawJsonStrings = new List<string> { "{\"key1\":\"value1\"}" };
            var jsonUriStrings = new List<string> { "https://example.com/config.json" };
            var embeddedJsonResources = new List<string> { "MyApp;Config.appsettings.json" };
            var localSecrets = new List<string> { "MyApp.Secrets" };

            // Act
            var configSource = new Configuration.ConfigurationSource(
                rawJsonStrings,
                jsonUriStrings,
                embeddedJsonResources,
                localSecrets
            );

            // Assert
            Assert.NotNull(configSource.RawJsonStrings);
            Assert.NotNull(configSource.JsonUriStrings);
            Assert.NotNull(configSource.EmbeddedJsonResources);
            Assert.NotNull(configSource.LocalSecrets);
            Assert.Single(configSource.RawJsonStrings);
            Assert.Single(configSource.JsonUriStrings);
            Assert.Single(configSource.EmbeddedJsonResources);
            Assert.Single(configSource.LocalSecrets);
            Assert.Equal("{\"key1\":\"value1\"}", configSource.RawJsonStrings[0]);
            Assert.Equal("https://example.com/config.json", configSource.JsonUriStrings[0]);
            Assert.Equal("MyApp;Config.appsettings.json", configSource.EmbeddedJsonResources[0]);
            Assert.Equal("MyApp.Secrets", configSource.LocalSecrets[0]);
        }

        [Fact]
        public void Constructor_WithNullRawJsonStrings_ShouldInitializeEmptyList()
        {
            // Arrange & Act
            var configSource = new Configuration.ConfigurationSource(
                null,
                new List<string>(),
                new List<string>(),
                new List<string>()
            );

            // Assert
            Assert.NotNull(configSource.RawJsonStrings);
            Assert.Empty(configSource.RawJsonStrings);
        }

        [Fact]
        public void Constructor_WithNullJsonUriStrings_ShouldInitializeEmptyList()
        {
            // Arrange & Act
            var configSource = new Configuration.ConfigurationSource(
                new List<string>(),
                null,
                new List<string>(),
                new List<string>()
            );

            // Assert
            Assert.NotNull(configSource.JsonUriStrings);
            Assert.Empty(configSource.JsonUriStrings);
        }

        [Fact]
        public void Constructor_WithNullEmbeddedJsonResources_ShouldInitializeEmptyList()
        {
            // Arrange & Act
            var configSource = new Configuration.ConfigurationSource(
                new List<string>(),
                new List<string>(),
                null,
                new List<string>()
            );

            // Assert
            Assert.NotNull(configSource.EmbeddedJsonResources);
            Assert.Empty(configSource.EmbeddedJsonResources);
        }

        [Fact]
        public void Constructor_WithNullLocalSecrets_ShouldInitializeEmptyList()
        {
            // Arrange & Act
            var configSource = new Configuration.ConfigurationSource(
                new List<string>(),
                new List<string>(),
                new List<string>(),
                null
            );

            // Assert
            Assert.NotNull(configSource.LocalSecrets);
            Assert.Empty(configSource.LocalSecrets);
        }

        [Fact]
        public void Constructor_WithAllNullParameters_ShouldInitializeAllEmptyLists()
        {
            // Arrange & Act
            var configSource = new Configuration.ConfigurationSource(null, null, null, null);

            // Assert
            Assert.NotNull(configSource.RawJsonStrings);
            Assert.NotNull(configSource.JsonUriStrings);
            Assert.NotNull(configSource.EmbeddedJsonResources);
            Assert.NotNull(configSource.LocalSecrets);
            Assert.Empty(configSource.RawJsonStrings);
            Assert.Empty(configSource.JsonUriStrings);
            Assert.Empty(configSource.EmbeddedJsonResources);
            Assert.Empty(configSource.LocalSecrets);
        }

        [Fact]
        public void DefaultConstructor_ShouldInitializeAllEmptyLists()
        {
            // Arrange & Act
            var configSource = new Configuration.ConfigurationSource();

            // Assert
            Assert.NotNull(configSource.RawJsonStrings);
            Assert.NotNull(configSource.JsonUriStrings);
            Assert.NotNull(configSource.EmbeddedJsonResources);
            Assert.NotNull(configSource.LocalSecrets);
            Assert.Empty(configSource.RawJsonStrings);
            Assert.Empty(configSource.JsonUriStrings);
            Assert.Empty(configSource.EmbeddedJsonResources);
            Assert.Empty(configSource.LocalSecrets);
        }

        #endregion

        #region Empty Static Property Tests

        [Fact]
        public void Empty_ShouldReturnConfigurationSourceWithEmptyLists()
        {
            // Act
            var emptySource = Configuration.ConfigurationSource.Empty;

            // Assert
            Assert.NotNull(emptySource);
            Assert.NotNull(emptySource.RawJsonStrings);
            Assert.NotNull(emptySource.JsonUriStrings);
            Assert.NotNull(emptySource.EmbeddedJsonResources);
            Assert.NotNull(emptySource.LocalSecrets);
            Assert.Empty(emptySource.RawJsonStrings);
            Assert.Empty(emptySource.JsonUriStrings);
            Assert.Empty(emptySource.EmbeddedJsonResources);
            Assert.Empty(emptySource.LocalSecrets);
        }

        [Fact]
        public void Empty_ShouldReturnNewInstanceEachTime()
        {
            // Act
            var empty1 = Configuration.ConfigurationSource.Empty;
            var empty2 = Configuration.ConfigurationSource.Empty;

            // Assert
            Assert.NotSame(empty1, empty2);
        }

        #endregion

        #region Property Initialization Tests

        [Fact]
        public void Properties_ShouldSupportInitOnlySetters()
        {
            // Arrange
            var rawJsonStrings = new List<string> { "{\"test\":\"value\"}" };
            var jsonUriStrings = new List<string> { "file:///config.json" };
            var embeddedJsonResources = new List<string> { "App;config.json" };
            var localSecrets = new List<string> { "SecretId" };

            // Act

            var configSource = new Configuration.ConfigurationSource(rawJsonStrings, jsonUriStrings, embeddedJsonResources, localSecrets);

            // Assert
            Assert.Same(rawJsonStrings, configSource.RawJsonStrings);
            Assert.Same(jsonUriStrings, configSource.JsonUriStrings);
            Assert.Same(embeddedJsonResources, configSource.EmbeddedJsonResources);
            Assert.Same(localSecrets, configSource.LocalSecrets);
        }

        #endregion

        #region URI Attribute Tests

        [Fact]
        public void JsonUriStrings_ShouldHaveURIAttribute()
        {
            // Arrange
            var propertyInfo = typeof(Configuration.ConfigurationSource)
                .GetProperty(nameof(Configuration.ConfigurationSource.JsonUriStrings));

            // Act
            var hasUriAttribute = propertyInfo?.GetCustomAttributes(typeof(URIAttribute), false).Length > 0;

            // Assert
            Assert.True(hasUriAttribute, "JsonUriStrings property should have URI attribute");
        }

        [Fact]
        public void EmbeddedJsonResources_ShouldHaveURIAttribute()
        {
            // Arrange
            var propertyInfo = typeof(Configuration.ConfigurationSource)
                .GetProperty(nameof(Configuration.ConfigurationSource.EmbeddedJsonResources));

            // Act
            var hasUriAttribute = propertyInfo?.GetCustomAttributes(typeof(URIAttribute), false).Length > 0;

            // Assert
            Assert.True(hasUriAttribute, "EmbeddedJsonResources property should have URI attribute");
        }

        [Fact]
        public void LocalSecrets_ShouldHaveURIAttribute()
        {
            // Arrange
            var propertyInfo = typeof(Configuration.ConfigurationSource)
                .GetProperty(nameof(Configuration.ConfigurationSource.LocalSecrets));

            // Act
            var hasUriAttribute = propertyInfo?.GetCustomAttributes(typeof(URIAttribute), false).Length > 0;

            // Assert
            Assert.True(hasUriAttribute, "LocalSecrets property should have URI attribute");
        }

        [Fact]
        public void RawJsonStrings_ShouldNotHaveURIAttribute()
        {
            // Arrange
            var propertyInfo = typeof(Configuration.ConfigurationSource)
                .GetProperty(nameof(Configuration.ConfigurationSource.RawJsonStrings));

            // Act
            var hasUriAttribute = propertyInfo?.GetCustomAttributes(typeof(URIAttribute), false).Length > 0;

            // Assert
            Assert.False(hasUriAttribute, "RawJsonStrings property should not have URI attribute");
        }

        #endregion

        #region Multiple Items Tests

        [Fact]
        public void Constructor_WithMultipleItemsInEachList_ShouldPreserveAllItems()
        {
            // Arrange
            var rawJsonStrings = new List<string> 
            { 
                "{\"key1\":\"value1\"}", 
                "{\"key2\":\"value2\"}", 
                "{\"key3\":\"value3\"}" 
            };
            var jsonUriStrings = new List<string> 
            { 
                "https://example.com/config1.json",
                "https://example.com/config2.json"
            };
            var embeddedJsonResources = new List<string> 
            { 
                "MyApp;Config.appsettings.json",
                "MyApp;Config.appsettings.dev.json",
                "MyApp;Config.logging.json"
            };
            var localSecrets = new List<string> 
            { 
                "MyApp.Secrets.Dev",
                "MyApp.Secrets.Prod"
            };

            // Act
            var configSource = new Configuration.ConfigurationSource(
                rawJsonStrings,
                jsonUriStrings,
                embeddedJsonResources,
                localSecrets
            );

            // Assert
            Assert.Equal(3, configSource.RawJsonStrings.Count);
            Assert.Equal(2, configSource.JsonUriStrings.Count);
            Assert.Equal(3, configSource.EmbeddedJsonResources.Count);
            Assert.Equal(2, configSource.LocalSecrets.Count);
            Assert.Equal(rawJsonStrings, configSource.RawJsonStrings);
            Assert.Equal(jsonUriStrings, configSource.JsonUriStrings);
            Assert.Equal(embeddedJsonResources, configSource.EmbeddedJsonResources);
            Assert.Equal(localSecrets, configSource.LocalSecrets);
        }

        #endregion

        #region List Independence Tests

        [Fact]
        public void Constructor_ShouldUseProvidedListsDirectly()
        {
            // Arrange
            var originalRawJsonStrings = new List<string> { "{\"key\":\"value\"}" };
            
            // Act
            var configSource = new Configuration.ConfigurationSource(
                originalRawJsonStrings,
                null,
                null,
                null
            );
            
            // The constructor doesn't create copies, it uses the same reference
            originalRawJsonStrings.Add("{\"another\":\"json\"}");

            // Assert - Both lists should have the same items since they share the reference
            Assert.Equal(2, configSource.RawJsonStrings.Count);
            Assert.Equal(2, originalRawJsonStrings.Count);
            Assert.Same(originalRawJsonStrings, configSource.RawJsonStrings);
        }

        [Fact]
        public void Properties_WithInitializers_ShouldAllowModification()
        {
            // Arrange
            var configSource = new Configuration.ConfigurationSource(rawJsonStrings: new List<string> { "{\"initial\":\"value\"}" }, new List<string>(), new List<string>(), new List<string>());

            // Act
            configSource.RawJsonStrings.Add("{\"added\":\"value\"}");

            // Assert
            Assert.Equal(2, configSource.RawJsonStrings.Count);
        }

        [Fact]
        public void Constructor_WithNullParameter_CreatesNewList()
        {
            // Arrange & Act
            var configSource = new Configuration.ConfigurationSource(
                null,
                null,
                null,
                null
            );

            // Act - Should be able to add to the newly created lists
            configSource.RawJsonStrings.Add("{\"test\":\"value\"}");
            configSource.JsonUriStrings.Add("https://example.com/config.json");
            configSource.EmbeddedJsonResources.Add("MyApp;config.json");
            configSource.LocalSecrets.Add("SecretId");

            // Assert
            Assert.Single(configSource.RawJsonStrings);
            Assert.Single(configSource.JsonUriStrings);
            Assert.Single(configSource.EmbeddedJsonResources);
            Assert.Single(configSource.LocalSecrets);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void Constructor_WithEmptyLists_ShouldPreserveEmptyLists()
        {
            // Arrange
            var emptyList = new List<string>();

            // Act
            var configSource = new Configuration.ConfigurationSource(
                emptyList,
                emptyList,
                emptyList,
                emptyList
            );

            // Assert
            Assert.Empty(configSource.RawJsonStrings);
            Assert.Empty(configSource.JsonUriStrings);
            Assert.Empty(configSource.EmbeddedJsonResources);
            Assert.Empty(configSource.LocalSecrets);
        }

        [Fact]
        public void Constructor_WithSpecialCharacters_ShouldPreserveValues()
        {
            // Arrange
            var rawJsonStrings = new List<string> { "{\"key\":\"value with spaces and 特殊字符\"}" };
            var jsonUriStrings = new List<string> { "https://example.com/config%20with%20spaces.json" };
            var embeddedJsonResources = new List<string> { "MyApp;Config.Special-Resource_123.json" };
            var localSecrets = new List<string> { "MyApp-Secrets_2024" };

            // Act
            var configSource = new Configuration.ConfigurationSource(
                rawJsonStrings,
                jsonUriStrings,
                embeddedJsonResources,
                localSecrets
            );

            // Assert
            Assert.Equal("{\"key\":\"value with spaces and 特殊字符\"}", configSource.RawJsonStrings[0]);
            Assert.Equal("https://example.com/config%20with%20spaces.json", configSource.JsonUriStrings[0]);
            Assert.Equal("MyApp;Config.Special-Resource_123.json", configSource.EmbeddedJsonResources[0]);
            Assert.Equal("MyApp-Secrets_2024", configSource.LocalSecrets[0]);
        }

        [Fact]
        public void Constructor_WithVeryLargeLists_ShouldHandleCorrectly()
        {
            // Arrange
            var largeList = Enumerable.Range(0, 1000)
                .Select(i => $"{{\"key{i}\":\"value{i}\"}}")
                .ToList();

            // Act
            var configSource = new Configuration.ConfigurationSource(
                largeList,
                null,
                null,
                null
            );

            // Assert
            Assert.Equal(1000, configSource.RawJsonStrings.Count);
            Assert.Equal("{\"key0\":\"value0\"}", configSource.RawJsonStrings[0]);
            Assert.Equal("{\"key999\":\"value999\"}", configSource.RawJsonStrings[999]);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ConfigurationSource_ShouldWorkWithAllPropertiesPopulated()
        {
            // Arrange & Act
            var configSource = new Configuration.ConfigurationSource(
                new List<string> 
                { 
                    "{\"database\":\"localhost\"}",
                    "{\"logging\":{\"level\":\"info\"}}"
                },
                new List<string> 
                { 
                    "https://config-server.com/app.json",
                    "file:///etc/config/settings.json"
                },
                new List<string> 
                { 
                    "MyApp;Resources.appsettings.json",
                    "MyApp;Resources.database.json"
                },
                new List<string> 
                { 
                    "MyApp-Dev-Secrets",
                    "MyApp-Prod-Secrets"
                }
            );

            // Assert
            Assert.Equal(2, configSource.RawJsonStrings.Count);
            Assert.Equal(2, configSource.JsonUriStrings.Count);
            Assert.Equal(2, configSource.EmbeddedJsonResources.Count);
            Assert.Equal(2, configSource.LocalSecrets.Count);
            
            // Verify all values are accessible
            Assert.Contains("database", configSource.RawJsonStrings[0]);
            Assert.Contains("config-server.com", configSource.JsonUriStrings[0]);
            Assert.Contains("appsettings.json", configSource.EmbeddedJsonResources[0]);
            Assert.Contains("Dev", configSource.LocalSecrets[0]);
        }

        [Fact]
        public void DefaultConstructor_Equals_EmptyProperty()
        {
            // Arrange
            var defaultConstructed = new Configuration.ConfigurationSource();
            var emptyProperty = Configuration.ConfigurationSource.Empty;

            // Assert - Verify they have the same structure (not same instance)
            Assert.NotSame(defaultConstructed, emptyProperty);
            Assert.Equal(defaultConstructed.RawJsonStrings.Count, emptyProperty.RawJsonStrings.Count);
            Assert.Equal(defaultConstructed.JsonUriStrings.Count, emptyProperty.JsonUriStrings.Count);
            Assert.Equal(defaultConstructed.EmbeddedJsonResources.Count, emptyProperty.EmbeddedJsonResources.Count);
            Assert.Equal(defaultConstructed.LocalSecrets.Count, emptyProperty.LocalSecrets.Count);
        }

        #endregion
    }
}
