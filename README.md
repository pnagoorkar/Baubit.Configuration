# Baubit.Configuration

[![CircleCI](https://dl.circleci.com/status-badge/img/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master)
[![codecov](https://codecov.io/gh/pnagoorkar/Baubit.Configuration/branch/master/graph/badge.svg)](https://codecov.io/gh/pnagoorkar/Baubit.Configuration)
[![NuGet](https://img.shields.io/nuget/v/Baubit.Configuration.svg)](https://www.nuget.org/packages/Baubit.Configuration)

Type-safe, fluent configuration builder for .NET with Result pattern error handling and environment variable expansion.

## Features

- **Fluent API** - Chain configuration sources naturally
- **Type-Safe** - Bind configuration to strongly-typed classes
- **Result Pattern** - No exceptions, explicit error handling
- **URI Expansion** - `${VAR}` placeholders with environment variables
- **Multiple Sources** - JSON files, embedded resources, raw strings, user secrets
- **Validation** - Built-in validator pipeline
- **Disposal Safety** - Single-use builders with automatic cleanup

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
public record AppConfig : AConfiguration
{
    public string ConnectionString { get; init; }
    public int MaxRetries { get; init; }
}

var result = new ConfigurationBuilder<AppConfig>()
    .WithRawJsonStrings("{\"ConnectionString\":\"...\",\"MaxRetries\":3}")
    .Bind(b => b.Build());

if (result.IsSuccess)
{
    var config = result.Value;
    Console.WriteLine(config.ConnectionString);
}
```

### Environment Variable Expansion

```csharp
public record PathConfig : AConfiguration
{
    [URI]
    public string LogPath { get; init; }
}

// With LogPath = "${HOME}/logs" in JSON
// Automatically expands to "/home/user/logs"
var result = new ConfigurationBuilder<PathConfig>()
    .WithRawJsonStrings("{\"LogPath\":\"${HOME}/logs\"}")
    .Bind(b => b.Build());
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

var result = new ConfigurationBuilder<AppConfig>()
    .WithRawJsonStrings(json)
    .Bind(b => b.WithValidators(new AppConfigValidator()))
    .Bind(b => b.Build());
```

## Configuration Sources

| Source | Method | Format |
|--------|--------|--------|
| JSON Files | `WithJsonUriStrings("file:///path")` | File URIs |
| Embedded Resources | `WithEmbeddedJsonResources("Assembly;Path")` | Assembly;Resource |
| Raw JSON | `WithRawJsonStrings("{...}")` | JSON strings |
| User Secrets | `WithLocalSecrets("SecretId")` | Secret ID |
| Additional Config | `WithAdditionalConfigurations(config)` | IConfiguration |

## URI Expansion

Mark properties with `[URI]` attribute for automatic environment variable expansion:

```csharp
public record Config : AConfiguration
{
    [URI]
    public string DatabasePath { get; init; }  // Expands ${DB_PATH}
    
    [URI]
    public List<string> SearchPaths { get; init; }  // Expands all items
    
    public string NormalPath { get; init; }  // No expansion
}
```

**Syntax:** `${VARIABLE_NAME}` - Fails if variable doesn't exist

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

// Or throw on failure
var config = builder.Build().ThrowIfFailed().Value;
```

## Best Practices

✅ **Do:**
- Use typed configuration classes inheriting `AConfiguration`
- Validate configuration with `IValidator<T>`
- Handle Result failures explicitly
- Use URI expansion for environment-specific values

❌ **Don't:**
- Reuse builder instances (auto-disposed after Build)
- Ignore Result failures
- Store sensitive data in code

## Examples

### Production Configuration

```csharp
public record ProductionConfig : AConfiguration
{
    [URI]
    public string DatabaseConnection { get; init; }
    
    [URI]
    public string LogDirectory { get; init; }
    
    public int PoolSize { get; init; }
}

var result = ConfigurationBuilder<ProductionConfig>().CreateNew()
    .WithJsonUriStrings("file:///etc/app/config.json")
    .Bind(b => b.WithLocalSecrets("ProductionSecrets"))
    .Bind(b => b.WithValidators(new ProductionValidator()))
    .Bind(b => b.Build());
```

### Testing Configuration

```csharp
var testConfig = ConfigurationBuilder<AppConfig>().CreateNew()
    .WithRawJsonStrings(@"{
        ""ConnectionString"": ""Server=localhost;Database=test"",
        ""MaxRetries"": 3
    }")
    .Bind(b => b.Build())
    .ThrowIfFailed()
    .Value;
```

## License

Licensed under the terms specified in [LICENSE](LICENSE).

## Contributing

Contributions welcome! This project maintains:
- 80%+ test coverage
- Comprehensive XML documentation
- Result pattern for error handling
- .NET 9.0 target

---

**Author:** Prashant Nagoorkar  
**Repository:** https://github.com/pnagoorkar/Baubit.Configuration
