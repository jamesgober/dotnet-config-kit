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
    public string? DatabaseUrl { get; set; }
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

var services = new ServiceCollection();

services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
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

## Common Patterns

### Multi-Environment Configuration

```csharp
var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "development";

services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json")
    .Build<AppConfig>();
```

**Files:**
- `appsettings.json` — Defaults
- `appsettings.development.json` — Dev overrides
- `appsettings.production.json` — Prod overrides

Later files override earlier ones. Production secrets can be in `appsettings.production.json` (gitignored).

### Environment Variables Override

```csharp
services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("MYAPP")
    .Build<AppConfig>();
```

Environment variables are loaded last, so they override JSON:

```bash
export MYAPP_DATABASEURL="Server=prod-db.com"
export MYAPP_MAXCONNECTIONS=100
```

### Testing with In-Memory Configuration

```csharp
[Fact]
public void TestWithConfiguration()
{
    var testConfig = new Dictionary<string, string>
    {
        { "databaseUrl", "Server=test-db" },
        { "maxConnections", "5" }
    };

    var services = new ServiceCollection();
    services
        .AddConfiguration()
        .AddMemory(testConfig)
        .Build<AppConfig>();

    var config = services.BuildServiceProvider().GetRequiredService<AppConfig>();
    
    Assert.Equal("Server=test-db", config.DatabaseUrl);
    Assert.Equal(5, config.MaxConnections);
}
```

### Nested Configuration

Define nested classes:

```csharp
public class AppConfig
{
    public DatabaseConfig? Database { get; set; }
    public LoggingConfig? Logging { get; set; }
}

public class DatabaseConfig
{
    public string? Host { get; set; }
    public int Port { get; set; } = 5432;
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class LoggingConfig
{
    public string? Level { get; set; } = "Info";
    public bool Console { get; set; } = true;
    public bool File { get; set; } = false;
}
```

**appsettings.json:**
```json
{
  "database": {
    "host": "localhost",
    "port": 5432,
    "username": "sa",
    "password": "password"
  },
  "logging": {
    "level": "Debug",
    "console": true,
    "file": true
  }
}
```

Configuration is bound recursively to nested properties.

### Enum Configuration

```csharp
public enum LogLevel { Debug, Info, Warning, Error, Critical }

public class AppConfig
{
    public LogLevel LogLevel { get; set; } = LogLevel.Info;
}
```

**appsettings.json:**
```json
{
  "logLevel": "Warning"
}
```

Enums are matched case-insensitively.

### Asynchronous Loading

For loading from async sources (remote config servers, databases):

```csharp
var services = new ServiceCollection();

await services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    .AddSource(new RemoteConfigSource("https://config.example.com"))
    .BuildAsync<AppConfig>();
```

### Custom Configuration Source

Implement `IConfigSource`:

```csharp
public class CustomSource : IConfigSource
{
    public string Name => "CustomSource";

    public IReadOnlyDictionary<string, string> Load()
    {
        return new Dictionary<string, string>
        {
            { "custom.setting", "value" },
            { "another.key", "another.value" }
        };
    }

    public ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken ct)
    {
        return new ValueTask<IReadOnlyDictionary<string, string>>(Load());
    }
}
```

Register it:
```csharp
services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    .AddSource(new CustomSource())
    .Build<AppConfig>();
```

### Error Handling

Catch binding errors at startup:

```csharp
try
{
    services
        .AddConfiguration()
        .AddJsonFile("appsettings.json")
        .Build<AppConfig>();
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Configuration error: {ex.Message}");
    Environment.Exit(1);
}
```

Or check for errors without binding:

```csharp
var config = new Dictionary<string, string>
{
    { "maxConnections", "not_a_number" }
};

var binder = new ReflectionConfigBinder<AppConfig>();
var errors = binder.GetValidationErrors(config);

if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Error: {error.Path} — {error.Message}");
        Console.WriteLine($"  Value: {error.AttemptedValue}");
    }
}
```

---

## Advanced Usage

### Key Naming Conventions

Configuration keys are **case-insensitive** and use **dot notation**:

```json
{
  "App": {
    "Name": "MyApp"
  }
}
```

Maps to all of these (same property):
- `app.name`
- `APP.NAME`
- `App.Name`
- `AppName` (if property doesn't nest)

### Environment Variable Mapping

Underscores in environment variables become dots, enabling hierarchical configuration:

```bash
# Maps to: database.connection.host
DATABASE_CONNECTION_HOST=localhost

# Maps to: database.pool.max_size (double underscore → single dot)
DATABASE_POOL__MAX_SIZE=10
```

### Configuration Priority

Sources are loaded in registration order. **Last wins:**

```csharp
.AddJsonFile("defaults.json")        // Loaded first
.AddJsonFile("overrides.json")       // Overrides defaults
.AddEnvironmentVariables()           // Overrides both
```

### Supported Types

- **Primitives:** `string`, `bool`, `byte`, `short`, `int`, `long`, `float`, `double`, `decimal`
- **Special Types:** `Guid`, `DateTime`, `TimeSpan`
- **Enums:** Case-insensitive matching
- **Nullable:** `int?`, `bool?`, etc.
- **Nested Types:** Other classes with parameterless constructors
- **Collections:** Limited to arrays and lists (via indexing)

### Validation at Startup

Errors are caught during binding, not at runtime:

```csharp
var config = new Dictionary<string, string>
{
    { "port", "invalid" }  // This will throw during Build<AppConfig>()
};

try
{
    services
        .AddConfiguration()
        .AddMemory(config)
        .Build<AppConfig>();  // ← Throws here, not later
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Configuration invalid: {ex.Message}");
}
```

---

## Best Practices

### ✅ DO

- **Load configuration once** at application startup
- **Use environment-specific files** for secrets and overrides
- **Validate configuration early** (during startup, not at runtime)
- **Use enums** for restricted values
- **Organize configuration** into nested classes
- **Document configuration keys** in comments or documentation
- **Use HTTPS** for remote configuration sources
- **Test configuration** with in-memory sources

### ❌ DON'T

- **Don't load configuration multiple times** — Cache it in DI
- **Don't commit secrets** to source control — Use environment variables
- **Don't trust untrusted configuration sources** — Validate all input
- **Don't reload configuration at runtime** without careful synchronization
- **Don't pass configuration files as constructor arguments** — Use DI
- **Don't parse configuration manually** — Use the binder
- **Don't block on async configuration loading** — Use `BuildAsync<T>()`

---

## Troubleshooting

### Configuration Not Binding

**Issue:** Properties remain at default values.

**Cause:** Key names don't match property names.

**Solution:** Check key casing and nesting. Use dots for nested objects:
```json
{
  "database": {
    "host": "localhost"  // Becomes "database.host"
  }
}
```

### Type Conversion Error

**Error:** `InvalidOperationException: Failed to bind to type Int32`

**Cause:** Value cannot be converted to target type.

**Solution:** Check configuration values are valid for their types:
```json
{
  "port": 5432,        // ✅ Valid int
  "port": "5432",      // ✅ String converted to int
  "port": "invalid"    // ❌ Cannot convert
}
```

### Environment Variable Not Loaded

**Issue:** Environment variable doesn't affect configuration.

**Cause:** 
- Wrong prefix used
- Variable name format incorrect
- Loaded before environment variable source

**Solution:**
```csharp
// With prefix "MYAPP", these are matched:
// MYAPP_DATABASE_HOST
// MYAPP_DEBUG

// These are NOT matched:
// DATABASE_HOST (no prefix)
// MYAPPDATABASE_HOST (no underscore after prefix)
// Other_HOST (different prefix)

.AddEnvironmentVariables("MYAPP")  // ✅ Correct
```

### File Not Found

**Issue:** Configuration from file is ignored.

**Cause:** File doesn't exist in the expected location.

**Solution:** Use absolute paths or check working directory:
```csharp
var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
Console.WriteLine($"Looking for: {configPath}");
Console.WriteLine($"Exists: {File.Exists(configPath)}");

.AddJsonFile(configPath)
```

---

## Next Steps

- **Read the [API Reference](API.md)** for complete API documentation
- **Explore [Project Structure](PROJECT_STRUCTURE.md)** for architecture details
- **Check [CHANGELOG](../CHANGELOG.md)** for version history
- **View [README](../README.md)** for feature overview

---

**Happy configuring! 🚀**
