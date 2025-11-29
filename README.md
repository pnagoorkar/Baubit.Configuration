# Baubit.Configuration


[![CircleCI](https://dl.circleci.com/status-badge/img/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master)
[![codecov](https://codecov.io/gh/pnagoorkar/Baubit.Configuration/branch/master/graph/badge.svg)](https://codecov.io/gh/pnagoorkar/Baubit.Configuration)<br/>
[![NuGet](https://img.shields.io/nuget/v/Baubit.Configuration.svg)](https://www.nuget.org/packages/Baubit.Configuration/)
![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4?logo=dotnet&logoColor=white)<br/>
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Known Vulnerabilities](https://snyk.io/test/github/pnagoorkar/Baubit.Configuration/badge.svg)](https://snyk.io/test/github/pnagoorkar/Baubit.Configuration)

Type-safe configuration builder for .NET with Result pattern error handling and environment variable expansion.

## Features

- **Type-Safe** - Bind configuration to strongly-typed classes
- **Result Pattern** - Explicit error handling without exceptions
- **Environment Variable Expansion** - `${VAR}` syntax in configuration values
- **Multiple Sources** - JSON files, embedded resources, raw strings, user secrets
- **Validation** - Custom validator pipeline support
- **Fluent API** - Chainable configuration methods

## Installation

```bash
dotnet add package Baubit.Configuration
```

## Quick Start

### Basic Configuration

```csharp
using Baubit.Configuration;

var result = ConfigurationBuilder.CreateNew()
    .Bind(b => b.WithRawJsonStrings("{\"Database\":\"Server=localhost\"}"))
    .Bind(b => b.Build());

if (result.IsSuccess)
{
    var value = result.Value["Database"]; // "Server=localhost"
}
```

### Type-Safe Configuration

```csharp
public class AppConfig : AConfiguration
{
    public string ConnectionString { get; init; }
    public int MaxRetries { get; init; }
}

var builder = ConfigurationBuilder<AppConfig>.CreateNew();
builder.WithRawJsonStrings("{\"ConnectionString\":\"...\",\"MaxRetries\":3}");
var result = builder.Build();

if (result.IsSuccess)
{
    var config = result.Value;
    Console.WriteLine(config.ConnectionString);
}
```

### Environment Variable Expansion

```csharp
public class PathConfig : AConfiguration
{
    [URI]
    public string LogPath { get; init; }
}

// With LogPath = "${HOME}/logs" in JSON
// Automatically expands to "/home/user/logs"
var builder = ConfigurationBuilder<PathConfig>.CreateNew();
builder.WithRawJsonStrings("{\"LogPath\":\"${HOME}/logs\"}");
var result = builder.Build();
```

### Multiple Configuration Sources

```csharp
var result = ConfigurationBuilder.CreateNew()
    .Bind(b => b.WithJsonUriStrings("file:///app/config.json"))
    .Bind(b => b.WithEmbeddedJsonResources("MyApp;Config.appsettings.json"))
    .Bind(b => b.WithLocalSecrets("MyApp-Secrets"))
    .Bind(b => b.WithRawJsonStrings("{\"Override\":\"Value\"}"))
    .Bind(b => b.Build());
```

### Validation

```csharp
using Baubit.Configuration.Validation;

public class AppConfigValidator : AValidator<AppConfig>
{
    public override Result Run(AppConfig config)
    {
        return Result.OkIf(
            config.MaxRetries > 0, 
            "MaxRetries must be positive"
        );
    }
}

var builder = ConfigurationBuilder<AppConfig>.CreateNew();
builder.WithRawJsonStrings("{\"ConnectionString\":\"test\",\"MaxRetries\":3}");
builder.WithValidators(new AppConfigValidator());
var result = builder.Build();
```

## Configuration Sources

| Source | Method | Format |
|--------|--------|--------|
| JSON Files | `WithJsonUriStrings("file:///path")` | File URIs |
| Embedded Resources | `WithEmbeddedJsonResources("Assembly;Path")` | Assembly;Resource |
| Raw JSON | `WithRawJsonStrings("{...}")` | JSON strings |
| User Secrets | `WithLocalSecrets("SecretId")` | Secret ID |
| Additional Config | `WithAdditionalConfigurations(config)` | IConfiguration |
| Configuration Sources | `WithAdditionaConfigurationSourcesFrom(config)` | IConfiguration with "configurationSource" section |
| Configuration Data | `WithAdditionaConfigurationsFrom(config)` | IConfiguration with "configuration" section |

## Environment Variable Expansion

Properties marked with `[URI]` attribute support automatic environment variable expansion using `${VAR}` syntax:

```csharp
public class Config : AConfiguration
{
    [URI]
    public string DatabasePath { get; init; }  // Expands ${DB_PATH}
    
    [URI]
    public List<string> SearchPaths { get; init; }  // Expands all items
    
    public string NormalPath { get; init; }  // No expansion
}
```

**Syntax:** `${VARIABLE_NAME}` - Build fails with error if variable doesn't exist

## API Reference

### ConfigurationBuilder

```csharp
// Factory
ConfigurationBuilder.CreateNew() : Result<ConfigurationBuilder>

// Methods
WithJsonUriStrings(params string[] uris) : Result<ConfigurationBuilder>
WithEmbeddedJsonResources(params string[] resources) : Result<ConfigurationBuilder>
WithLocalSecrets(params string[] secrets) : Result<ConfigurationBuilder>
WithRawJsonStrings(params string[] json) : Result<ConfigurationBuilder>
WithAdditionalConfigurations(params IConfiguration[] configs) : Result<ConfigurationBuilder>
WithAdditionaConfigurationSourcesFrom(params IConfiguration[] configs) : Result<ConfigurationBuilder>
WithAdditionaConfigurationsFrom(params IConfiguration[] configs) : Result<ConfigurationBuilder>
Build() : Result<IConfiguration>
```

### ConfigurationBuilder\<TConfiguration>

```csharp
// Inherits all ConfigurationBuilder methods plus:
WithValidators(params IValidator<TConfiguration>[] validators) : Result<ConfigurationBuilder<TConfiguration>>
Build() : Result<TConfiguration>  // Returns typed config
```

## Error Handling

All operations return `Result<T>` from [FluentResults](https://github.com/altmann/FluentResults):

```csharp
var result = builder.Build();

if (result.IsFailed)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine(error.Message);
    }
}
```

## Best Practices

- Use typed configuration classes inheriting `AConfiguration`
- Validate configuration with `IValidator<T>`
- Handle `Result` failures explicitly
- Use `[URI]` attribute for environment-specific values
- Avoid storing sensitive data in code
- Use `WithAdditionaConfigurationSourcesFrom` to load configuration sources from external configurations
- Use `WithAdditionaConfigurationsFrom` to load configuration data from external configurations

## Examples

### Production Configuration

```csharp
public class ProductionConfig : AConfiguration
{
    [URI]
    public string DatabaseConnection { get; init; }
    
    [URI]
    public string LogDirectory { get; init; }
    
    public int PoolSize { get; init; }
}

var builder = ConfigurationBuilder<ProductionConfig>.CreateNew();
builder.WithJsonUriStrings("file:///etc/app/config.json");
builder.WithLocalSecrets("ProductionSecrets");
builder.WithValidators(new ProductionValidator());
var result = builder.Build();
```

### Testing Configuration

```csharp
var builder = ConfigurationBuilder<AppConfig>.CreateNew();
builder.WithRawJsonStrings(@"{
    ""ConnectionString"": ""Server=localhost;Database=test"",
    ""MaxRetries"": 3
}");
var result = builder.Build();

if (result.IsSuccess)
{
    var testConfig = result.Value;
}
```

### Loading Configuration from External Sources

```csharp
// Load configuration sources from another configuration
var externalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string> 
    {
        { "configurationSource:RawJsonStrings:0", "{\"Key\":\"Value\"}" }
    })
    .Build();

var builder = ConfigurationBuilder.CreateNew();
builder.WithAdditionaConfigurationSourcesFrom(externalConfig);
var result = builder.Build();
```

### Loading Configuration Data from External Sources

```csharp
// Load configuration data from another configuration
var externalConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string> 
    {
        { "configuration:Database", "Server=localhost" }
    })
    .Build();

var builder = ConfigurationBuilder.CreateNew();
builder.WithAdditionaConfigurationsFrom(externalConfig);
var result = builder.Build();

if (result.IsSuccess)
{
    var value = result.Value["Database"]; // "Server=localhost"
}
```

## License

Licensed under the terms specified in [LICENSE](LICENSE).

## Contributing

Contributions welcome! This project uses:
- Result pattern for error handling
- Comprehensive XML documentation

---

**Author:** Prashant Nagoorkar  
**Repository:** https://github.com/pnagoorkar/Baubit.Configuration