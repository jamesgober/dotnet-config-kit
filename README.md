# dotnet-config-kit

[![NuGet](https://img.shields.io/nuget/v/JG.ConfigKit?logo=nuget)](https://www.nuget.org/packages/JG.ConfigKit)
[![Downloads](https://img.shields.io/nuget/dt/JG.ConfigKit?color=%230099ff&logo=nuget)](https://www.nuget.org/packages/JG.ConfigKit)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](./LICENSE)
[![CI](https://github.com/jamesgober/dotnet-config-kit/actions/workflows/ci.yml/badge.svg)](https://github.com/jamesgober/dotnet-config-kit/actions)

---

## Quick Links

[![Install NuGet](https://img.shields.io/badge/install-nuget-green?logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/JG.ConfigKit/1.0.0)
[![Get Started](https://img.shields.io/badge/get%20started-docs-blue?style=for-the-badge)](./docs/GETTING_STARTED.md)
[![View on GitHub](https://img.shields.io/badge/view%20on-github-black?logo=github&style=for-the-badge)](https://github.com/jamesgober/dotnet-config-kit)

---

A high-performance, multi-source configuration library for .NET. Supports JSON, YAML, TOML, INI, XML, environment variables, command-line arguments, user secrets, and custom sources. Binds to strongly-typed options with DataAnnotations validation, custom type converters, configuration profiles, hot-reload capability, zero-copy reads, and optional hot-reload. Built for production: fast, reliable, and secure.

## Features

- **Multiple Formats** — JSON, YAML, TOML, INI, XML with automatic format detection
- **Multi-Source Loading** — Environment variables, command-line arguments, user secrets, in-memory dictionaries, files, HTTP endpoints, and custom sources with override priority by registration order
- **Strongly-Typed Binding** — Bind flat configuration to POCO classes with nested object and collection support
- **Validation** — Type checking, DataAnnotations support, and error reporting during binding with clear paths to configuration errors
- **Custom Type Converters** — Extend type conversion for custom types (Uri, IPAddress, etc.)
- **Configuration Profiles** — Environment-specific settings (development, staging, production) with auto-detection
- **Merging Strategies** — Control how multiple sources combine: last-wins, first-wins, merge, or strict
- **Hot-Reload** — FileSystemWatcher integration for automatic configuration reloading with change notifications
- **Remote Configuration** — Load configuration from HTTP endpoints with optional polling for cloud-native architectures
- **Configuration Export** — Serialize configuration to JSON or YAML for debugging and auditing
- **Lazy Loading** — Defer source initialization until first access with optional timeouts for performance optimization
- **Async-First Design** — Async load paths with `ValueTask<T>` for performance; sync paths also available
- **Zero-Copy Reads** — Configuration cached after loading; reads are lock-free and allocation-free
- **Extensible** — Custom sources and parsers via simple interfaces
- **Cross-Platform** — Works on Windows, Linux, and macOS

## Installation

```bash
dotnet add package JG.ConfigKit
```

## Quick Start

Define a configuration class:

```csharp
public class DatabaseSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
```

Register and use in dependency injection:

```csharp
var services = new ServiceCollection();

services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("MYAPP")
    .Build<DatabaseSettings>();

var serviceProvider = services.BuildServiceProvider();
var dbSettings = serviceProvider.GetRequiredService<DatabaseSettings>();

Console.WriteLine($"Connecting to {dbSettings.Host}:{dbSettings.Port}");
```

Environment variables are matched case-insensitively with underscores converted to dots:

```bash
export MYAPP_HOST=prod.db.example.com
export MYAPP_PORT=5433
# Binds to Host="prod.db.example.com", Port=5433
```

## Configuration Sources

### JSON Files

```csharp
.AddJsonFile("appsettings.json")
.AddJsonFile("appsettings.local.json", isOptional: true)  // Skip if missing
.AddJsonFile("appsettings.prod.json", isOptional: true, enableHotReload: true)
```

### YAML Files

```csharp
.AddYamlFile("config.yaml", enableHotReload: true)
.AddYamlFile("config.local.yaml", isOptional: true)
```

### TOML Files

```csharp
.AddTomlFile("config.toml")
.AddTomlFile("config.local.toml", isOptional: true)
```

### INI Files

```csharp
.AddIniFile("config.ini")
.AddIniFile("config.local.ini", isOptional: true, enableHotReload: true)
```

### XML Files

```csharp
.AddXmlFile("config.xml")
.AddXmlFile("config.local.xml", isOptional: true)
```

### Environment Variables

```csharp
.AddEnvironmentVariables("MYAPP")
```

Environment variables with the prefix (followed by underscore) are included. Underscores become dots:

```bash
MYAPP_DATABASE_HOST=localhost      # → database.host
MYAPP_DEBUG=true                   # → debug
```

### Command-Line Arguments

```csharp
.AddCommandLineArguments(args)
```

Arguments support three formats:

```bash
--key=value                        # Direct assignment
--key value                        # Space-separated
-k value                           # Short form
--flag                             # Boolean flags (→ "true")
```

### User Secrets

```csharp
.AddUserSecrets<Program>()
```

Loads from `~/.microsoft/usersecrets/{UserSecretsId}/secrets.json`. Perfect for local development secrets without committing to source control.

### In-Memory Dictionaries

```csharp
.AddMemory(new Dictionary<string, string>
{
    { "database.host", "localhost" },
    { "database.port", "5432" }
})
```

### Custom Sources

Implement `IConfigSource`:

```csharp
public class CustomSource : IConfigSource
{
    public string Name => "Custom";
    
    public IReadOnlyDictionary<string, string> Load()
    {
        return new Dictionary<string, string>
        {
            { "key", "value" }
        };
    }
    
    public ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<IReadOnlyDictionary<string, string>>(Load());
    }
}

// Register
.AddSource(new CustomSource())
```

## Type Binding

Configuration is bound to public properties. Supported types:

- String, bool, int, long, float, double, decimal
- Guid, DateTime, TimeSpan
- Enums (case-insensitive)
- Nullable types
- Custom types with `IConfigValueConverter<T>`
- Collections (arrays, lists) via array indexing

### Example with Nested Objects

```csharp
public class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public LogSettings Log { get; set; } = new();
    public Uri ApiEndpoint { get; set; } = null!;
}

public class DatabaseSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
}

public class LogSettings
{
    public LogLevel Level { get; set; } = LogLevel.Info;
}

public enum LogLevel { Debug, Info, Warning, Error }
```

### Custom Type Converters

Convert custom types like `Uri`, `IPAddress`, or domain-specific types:

```csharp
public class UriConverter : IConfigValueConverter<Uri>
{
    public Uri Convert(string? value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("URI cannot be empty");
        return new Uri(value);
    }
}

// Register
var binder = new EnhancedReflectionConfigBinder<AppSettings>();
binder.RegisterConverter<Uri>(new UriConverter());
```

### Validation with DataAnnotations

Validate configuration using standard .NET attributes:

```csharp
public class AppSettings
{
    [Required(ErrorMessage = "API key is required")]
    public string? ApiKey { get; set; }

    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
    public int Port { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? AdminEmail { get; set; }

    [Url(ErrorMessage = "Invalid URL format")]
    public string? ApiUrl { get; set; }
}
```

Validation errors are reported during binding with clear messages and configuration paths.

### Configuration Profiles

Use environment-specific settings:

```csharp
services
    .AddConfiguration()
    .WithAutoProfile()  // Auto-detect from ASPNETCORE_ENVIRONMENT
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.{profile}.json")  // development, staging, production
    .AddEnvironmentVariables("MYAPP")
    .Build<AppSettings>();
```

Or set profile explicitly:

```csharp
.WithProfile("production")
```

### Hot-Reload Capability

Automatically reload configuration when files change:

```csharp
services
    .AddConfiguration()
    .AddJsonFile("config.json", enableHotReload: true)
    .AddYamlFile("settings.yaml", enableHotReload: true)
    .Build<AppSettings>();

// Subscribe to changes
var hotReload = serviceProvider.GetRequiredService<HotReloadFileSource>();
hotReload.OnChange(newConfig => 
{
    Console.WriteLine("Configuration reloaded!");
});
```

### Remote Configuration

Load configuration from HTTP endpoints with optional automatic polling:

```csharp
services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    // Load from remote server, polling every 60 seconds
    .AddHttpSource(
        new Uri("https://config.example.com/api/settings"),
        new JsonConfigParser(),
        pollIntervalSeconds: 60)
    .Build<AppSettings>();
```

### Merging Strategies

Control how multiple sources are combined:

```csharp
services
    .AddConfiguration()
    .WithMergeStrategy(MergeStrategy.FirstWins)  // First source wins
    .AddJsonFile("defaults.json")
    .AddJsonFile("overrides.json")
    .Build<AppSettings>();
```

Available strategies:
- `LastWins` (default) — Later sources override earlier ones
- `FirstWins` — First source to define a key wins
- `Merge` — Combine all sources; error on conflicts with different values
- `Throw` — Error if any key appears in multiple sources

### Lazy Configuration Loading

Defer source loading until first access for improved startup performance:

```csharp
services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    // Lazy-load from HTTP with 30-second timeout
    .AddLazySource(
        new HttpConfigSource(new Uri("https://config.example.com/settings"), new JsonConfigParser()),
        timeoutSeconds: 30)
    .Build<AppSettings>();
```

### Configuration Export

Export loaded configuration to JSON or YAML for debugging:

```csharp
var configBuilder = services.AddConfiguration()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("MYAPP");

// Export for inspection
var jsonConfig = configBuilder.ExportAsJson(indent: true);
var yamlConfig = configBuilder.ExportAsYaml();

Console.WriteLine(jsonConfig);
File.WriteAllText("exported-config.json", jsonConfig);
```

### Default Configuration Values

Provide fallback values that can be overridden by other sources:

```csharp
services
    .AddConfiguration()
    .AddDefaults(new Dictionary<string, string>
    {
        { "database.host", "localhost" },
        { "database.port", "5432" },
        { "logging.level", "Info" }
    })
    .AddJsonFile("appsettings.json", isOptional: true)
    .AddEnvironmentVariables("MYAPP")
    .Build<AppSettings>();
```

Defaults are applied first, so any later source can override them.

### Configuration Presets

Register and reuse common configurations across your application:

```csharp
// Register presets during setup
services
    .AddConfiguration()
    .RegisterPreset("development", new Dictionary<string, string>
    {
        { "database.host", "localhost" },
        { "logging.level", "Debug" },
        { "features.strict-validation", "true" }
    })
    .RegisterPreset("production", new Dictionary<string, string>
    {
        { "database.host", "prod-db.example.com" },
        { "logging.level", "Error" },
        { "features.strict-validation", "false" }
    })
    // Load the appropriate preset based on environment
    .UsePreset(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development")
    .AddJsonFile("appsettings.json", isOptional: true)
    .Build<AppSettings>();
```

Or use presets with profiles:

```csharp
services
    .AddConfiguration()
    .RegisterPreset("dev", devDefaults)
    .RegisterPreset("prod", prodDefaults)
    .WithAutoProfile()
    .UsePreset(builder.CurrentProfile ?? "dev")  // Load preset matching profile
    .AddJsonFile($"appsettings.{profile}.json", isOptional: true)
    .Build<AppSettings>();
```

---

## Documentation

For more information, see:

- **[Getting Started Guide](./docs/GETTING_STARTED.md)** — Quick start and common patterns
- **[API Reference](./docs/API.md)** — Complete API documentation
- **[Advanced Usage](./docs/ADVANCED.md)** — Complex scenarios and best practices

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

Licensed under the Apache License 2.0. See [LICENSE](./LICENSE) for details.

---

**Ready to get started?** Install via NuGet and check out the [getting started guide](./docs/GETTING_STARTED.md).
