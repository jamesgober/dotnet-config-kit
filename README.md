<div align="center">
    <img width="120px" height="auto" src="https://raw.githubusercontent.com/jamesgober/jamesgober/main/media/icons/hexagon-3.svg" alt="Triple Hexagon">
    <h1>
        <strong>dotnet-config-kit</strong>
        <sup><br><sub>CONFIGURATION MANAGEMENT</sub></sup>
    </h1>
    <div>
        <a href="https://www.nuget.org/packages/JG.ConfigKit"><img alt="NuGet" src="https://img.shields.io/nuget/v/JG.ConfigKit"></a>
        <span>&nbsp;</span>
        <a href="https://www.nuget.org/packages/JG.ConfigKit"><img alt="NuGet Downloads" src="https://img.shields.io/nuget/dt/JG.ConfigKit?color=%230099ff"></a>
        <span>&nbsp;</span>
        <a href="./LICENSE" title="License"><img alt="License" src="https://img.shields.io/badge/license-Apache--2.0-blue.svg"></a>
        <span>&nbsp;</span>
        <a href="https://github.com/jamesgober/dotnet-config-kit/actions"><img alt="GitHub CI" src="https://github.com/jamesgober/dotnet-config-kit/actions/workflows/ci.yml/badge.svg"></a>
    </div>
</div>

A high-performance, multi-source configuration library for .NET. Supports JSON, YAML, TOML, INI, XML, environment variables, command-line arguments, user secrets, and custom sources. Binds to strongly-typed options with DataAnnotations validation, custom type converters, configuration profiles, hot-reload capability, zero-copy reads, and optional hot-reload. Built for production: fast, reliable, and secure.

## Features

- **Multiple Formats** — JSON, YAML, TOML, INI, XML with automatic format detection
- **Multi-Source Loading** — Environment variables, command-line arguments, user secrets, in-memory dictionaries, files, and custom sources with override priority by registration order
- **Strongly-Typed Binding** — Bind flat configuration to POCO classes with nested object and collection support
- **Validation** — Type checking, DataAnnotations support, and error reporting during binding with clear paths to configuration errors
- **Custom Type Converters** — Extend type conversion for custom types (Uri, IPAddress, etc.)
- **Configuration Profiles** — Environment-specific settings (development, staging, production) with auto-detection
- **Hot-Reload** — FileSystemWatcher integration for automatic configuration reloading with change notifications
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
```

### YAML Files

```csharp
.AddYamlFile("config.yaml", enableHotReload: true)
```

### TOML Files

```csharp
.AddTomlFile("config.toml")
```

### INI Files

```csharp
.AddIniFile("config.ini")
```

### XML Files

```csharp
.AddXmlFile("config.xml")
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
