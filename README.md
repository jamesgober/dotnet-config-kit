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

A high-performance, multi-source configuration library for .NET. Loads from JSON files, environment variables, memory, and custom sources in registration order. Binds to strongly-typed options with validation, zero-copy reads, and optional hot-reload. Built for production: fast, reliable, and secure.

## Features

- **Multi-Source Loading** — JSON, environment variables, in-memory dictionaries, and custom sources with override priority by registration order
- **Strongly-Typed Binding** — Bind flat configuration to POCO classes with nested object and collection support
- **Validation** — Type checking and error reporting during binding with clear paths to configuration errors
- **Async-First Design** — Async load paths with `ValueTask<T>` for performance; sync paths also available
- **Zero-Copy Reads** — Configuration cached after loading; reads are lock-free and allocation-free
- **Extensible** — Custom sources and parsers via simple interfaces

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

File format:
```json
{
  "database": {
    "host": "localhost",
    "port": 5432,
    "username": "admin"
  },
  "debug": true
}
```

All keys are flattened to dot-notation internally: `database.host`, `database.port`, etc.

### Environment Variables

```csharp
.AddEnvironmentVariables("MYAPP")
```

Environment variables with the prefix (followed by underscore) are included. Underscores become dots:

```bash
MYAPP_DATABASE_HOST=localhost      # → database.host
MYAPP_DEBUG=true                   # → debug
```

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
- Collections (arrays, lists) via array indexing

Example with nested objects:

```csharp
public class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public LogSettings Log { get; set; } = new();
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

Configuration file:
```json
{
  "database": {
    "host": "prod.db.example.com",
    "port": 5433
  },
  "log": {
    "level": "Warning"
  }
}
```

Binding:
```csharp
services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    .Build<AppSettings>();
```

## Error Handling

Binding errors include the configuration path and actual value:

```
InvalidOperationException: Configuration binding failed with 1 error(s):
  database.port: Failed to bind to type Int32: Cannot convert 'not_a_number' to int
```

Use `IConfigBinder<T>.GetValidationErrors()` to check for errors without throwing:

```csharp
var binder = serviceProvider.GetRequiredService<IConfigBinder<AppSettings>>();
var errors = binder.GetValidationErrors(configuration);

if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"{error.Path}: {error.Message}");
    }
}
```

## Performance

Configuration is loaded once during application startup and cached. All subsequent reads are lock-free and allocation-free.

To benchmark your application:

```bash
dotnet run --project tests/dotnet-config-kit.Benchmarks -c Release
```

## Architecture

- **`Abstractions/`** — `IConfigParser`, `IConfigSource`, `IConfigBinder<T>`, `IConfigBuilder`
- **`Internal/`** — Implementation: `ConfigBuilder`, parsers, sources, binders
- **`Extensions/`** — `AddConfiguration()` and fluent builder API

All internal types are sealed and implementation-hidden. Depend on abstractions.

## Thread Safety

Configuration is immutable after loading. The `IConfigBuilder.Configuration` dictionary is thread-safe for reading.

Custom sources should be thread-safe if they may be called concurrently.

## Supported .NET Versions

- .NET 8.0 and later

## License

Apache 2.0 — See [LICENSE](./LICENSE) for details.

---

<!--
:: COPYRIGHT
=========================== -->
<div align="center">

  <h2></h2>
  <sup>COPYRIGHT <small>&copy;</small> 2026 <strong>JAMES GOBER.</strong></sup>
</div>
