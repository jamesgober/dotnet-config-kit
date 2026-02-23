# Getting Started — dotnet-config-kit

## Installation

```bash
dotnet add package JG.ConfigKit
```

Or via Package Manager:
```
Install-Package JG.ConfigKit
```

## 5-Minute Quick Start

### 1. Define Your Configuration Class

```csharp
public class AppConfig
{
    [Required(ErrorMessage = "Database URL is required")]
    public string? DatabaseUrl { get; set; }
    
    [Range(1, 1000)]
    public int MaxConnections { get; set; } = 10;
    
    public bool EnableLogging { get; set; } = true;
}
```

### 2. Create appsettings.json

```json
{
  "databaseUrl": "Server=localhost;Database=mydb",
  "maxConnections": 25,
  "enableLogging": false
}
```

### 3. Register in Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using dotnet_config_kit.Extensions;
using System.ComponentModel.DataAnnotations;

var services = new ServiceCollection();

services
    .AddConfiguration()
    .WithAutoProfile()
    .AddDefaults(new Dictionary<string, string>
    {
        { "maxConnections", "10" },
        { "enableLogging", "true" }
    })
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.{profile}.json", isOptional: true)
    .AddEnvironmentVariables("MYAPP")
    .AddCommandLineArguments(args)
    .Build<AppConfig>();

var provider = services.BuildServiceProvider();
```

### 4. Use Configuration

```csharp
var config = provider.GetRequiredService<AppConfig>();

Console.WriteLine($"Database: {config.DatabaseUrl}");
Console.WriteLine($"Max Connections: {config.MaxConnections}");
Console.WriteLine($"Logging: {config.EnableLogging}");
```

**Output:**
```
Database: Server=localhost;Database=mydb
Max Connections: 25
Logging: False
```

---

## Common Configuration Patterns

### Using Default Values

Default values are useful for optional settings that should have sensible fallbacks:

```csharp
services
    .AddConfiguration()
    .AddDefaults(new Dictionary<string, string>
    {
        { "api.timeout", "30" },
        { "api.retries", "3" },
        { "cache.enabled", "true" }
    })
    .AddJsonFile("appsettings.json", isOptional: true)
    .AddEnvironmentVariables("APP")
    .Build<AppSettings>();
```

Defaults are applied first, so environment variables or files can override them.

### Using Configuration Presets

Presets are perfect for environment-specific configurations. Define them once and reuse across your app:

```csharp
services
    .AddConfiguration()
    .RegisterPreset("development", new Dictionary<string, string>
    {
        { "database.host", "localhost" },
        { "database.port", "5432" },
        { "logging.level", "Debug" },
        { "features.strictValidation", "true" }
    })
    .RegisterPreset("production", new Dictionary<string, string>
    {
        { "database.host", "prod.db.example.com" },
        { "database.port", "5432" },
        { "logging.level", "Error" },
        { "features.strictValidation", "false" }
    })
    .UsePreset(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development")
    .AddJsonFile("appsettings.json", isOptional: true)
    .Build<AppSettings>();
```

### Optional Configuration Files

Load environment-specific files without errors if they're missing:

```csharp
services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")  // Required - will error if missing
    .AddJsonFile("appsettings.local.json", isOptional: true)  // Optional
    .AddJsonFile("appsettings.{profile}.json", isOptional: true, enableHotReload: true)
    .Build<AppSettings>();
```

### Hot-Reload Configuration

Watch for configuration file changes and reload automatically:

```csharp
services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.local.json", isOptional: true, enableHotReload: true)
    .AddYamlFile("features.yaml", enableHotReload: true)
    .Build<AppSettings>();

// Subscribe to changes
var hotReload = provider.GetRequiredService<HotReloadFileSource>();
hotReload.OnChange(newConfig =>
{
    Console.WriteLine("Configuration reloaded!");
    var updatedSettings = provider.GetRequiredService<AppSettings>();
});
```

---

## What's New in v0.5.0

**Launch Polish Release** includes:

- ✅ **Default values** via `AddDefaults()` — Set fallback configuration
- ✅ **Configuration presets** — Register and reuse common configurations
- ✅ **Consistent hot-reload** — All file formats support `enableHotReload`
- ✅ **Optional files** — All file sources support `isOptional` parameter
- ✅ **API consistency** — Unified API across all configuration sources

See [Changelog](../CHANGELOG.md) for complete release notes.

---

## Next Steps

- [**API Reference**](./API.md) — Complete API documentation
- [**Configuration Sources**](./API.md#configuration-sources) — Detailed source documentation
- [**Advanced Usage**](./ADVANCED.md) — Complex scenarios and patterns
