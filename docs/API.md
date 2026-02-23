# API Reference — dotnet-config-kit

## Core Interfaces

### `IConfigParser`

**Namespace:** `dotnet_config_kit.Abstractions`

Parses configuration from a specific format into flat key-value pairs.

#### Members

```csharp
string Format { get; }
```
Gets the format this parser handles (e.g., "json", "yaml", "toml").

```csharp
IReadOnlyDictionary<string, string> Parse(string content)
```
Parses configuration content into a flat key-value dictionary.

**Parameters:**
- `content` (string): Raw configuration content. Must not be null or empty.

**Returns:** Dictionary with keys in dot-notation format (e.g., "database.host").

**Exceptions:**
- `ArgumentNullException`: When content is null
- `ArgumentException`: When content is invalid for the format
- `InvalidOperationException`: For other parsing failures

**Example:**
```csharp
var parser = new JsonConfigParser();
var config = parser.Parse("""
    {
      "database": {
        "host": "localhost",
        "port": "5432"
      }
    }
""");

Console.WriteLine(config["database.host"]); // "localhost"
```

```csharp
ValueTask<IReadOnlyDictionary<string, string>> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
```
Asynchronously parses configuration from a stream.

**Parameters:**
- `stream` (Stream): Stream to read from. Must not be null.
- `cancellationToken` (CancellationToken): Cancellation token.

**Returns:** Task returning flat key-value dictionary.

**Exceptions:**
- `ArgumentNullException`: When stream is null
- `ArgumentException`: When content is invalid
- `OperationCanceledException`: When cancelled

---

### `IConfigSource`

**Namespace:** `dotnet_config_kit.Abstractions`

Provides configuration from a specific source.

#### Members

```csharp
string Name { get; }
```
Gets a human-readable name for the source (e.g., "appsettings.json", "Environment Variables").

```csharp
IReadOnlyDictionary<string, string> Load()
```
Loads configuration from this source.

**Returns:** Dictionary of configuration keys and values in dot-notation format.

**Remarks:** Returns empty dictionary if source is unavailable or empty. Graceful degradation is preferred over exceptions for optional sources.

**Example:**
```csharp
var source = new FileConfigSource("appsettings.json", new JsonConfigParser());
var config = source.Load();
```

```csharp
ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken = default)
```
Asynchronously loads configuration from this source.

**Parameters:**
- `cancellationToken` (CancellationToken): Cancellation token.

**Returns:** Task returning configuration dictionary.

---

### `IConfigBuilder`

**Namespace:** `dotnet_config_kit.Abstractions`

Loads and aggregates configuration from multiple sources.

#### Members

```csharp
IReadOnlyDictionary<string, string> Configuration { get; }
```
Gets the aggregated configuration as a flat key-value dictionary.

**Returns:** Dictionary with all sources merged (later sources override earlier ones).

```csharp
IConfigBuilder Load()
```
Loads configuration from all registered sources synchronously.

**Returns:** Self for method chaining.

**Remarks:** Sources are loaded in registration order. Later sources override earlier ones.

```csharp
ValueTask<IConfigBuilder> LoadAsync(CancellationToken cancellationToken = default)
```
Asynchronously loads configuration from all registered sources.

**Parameters:**
- `cancellationToken` (CancellationToken): Cancellation token.

**Returns:** Task returning self for method chaining.

---

### `IConfigBinder<T>`

**Namespace:** `dotnet_config_kit.Abstractions`

Validates and binds flat configuration to strongly-typed objects.

**Type Parameters:**
- `T` (class): The target type to bind configuration to. Must have parameterless constructor.

#### Members

```csharp
T Bind(IReadOnlyDictionary<string, string> configuration)
```
Binds flat configuration to a strongly-typed object.

**Parameters:**
- `configuration` (IReadOnlyDictionary<string, string>): Configuration key-value pairs.

**Returns:** New instance of T with configuration applied.

**Exceptions:**
- `ArgumentNullException`: When configuration is null
- `InvalidOperationException`: When binding fails due to type mismatches or validation failures

**Example:**
```csharp
public class AppSettings
{
    public string? DatabaseHost { get; set; }
    public int DatabasePort { get; set; }
    public bool Debug { get; set; }
}

var config = new Dictionary<string, string>
{
    { "databasehost", "localhost" },
    { "databaseport", "5432" },
    { "debug", "true" }
};

var binder = new ReflectionConfigBinder<AppSettings>();
var settings = binder.Bind(config);

Console.WriteLine($"{settings.DatabaseHost}:{settings.DatabasePort}");
// Output: localhost:5432
```

```csharp
ValueTask<T> BindAsync(IReadOnlyDictionary<string, string> configuration, CancellationToken cancellationToken = default)
```
Asynchronously binds configuration to strongly-typed object.

**Parameters:**
- `configuration` (IReadOnlyDictionary<string, string>): Configuration key-value pairs.
- `cancellationToken` (CancellationToken): Cancellation token.

**Returns:** Task returning bound instance.

```csharp
IReadOnlyList<ConfigurationError> GetValidationErrors(IReadOnlyDictionary<string, string> configuration)
```
Gets validation errors without binding.

**Parameters:**
- `configuration` (IReadOnlyDictionary<string, string>): Configuration to validate.

**Returns:** List of ConfigurationError; empty if valid.

**Example:**
```csharp
var errors = binder.GetValidationErrors(config);
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"{error.Path}: {error.Message}");
    }
}
```

---

### `ConfigurationError`

**Namespace:** `dotnet_config_kit.Abstractions`

Represents a single configuration validation error.

#### Members

```csharp
string Path { get; init; }
```
Dot-notation path to the problematic configuration value (e.g., "database.port").

```csharp
string Message { get; init; }
```
Error message describing the validation failure.

```csharp
string? AttemptedValue { get; init; }
```
The actual value that failed validation. May be null for missing required values.

---

## Extension Methods

### `AddConfiguration`

**Namespace:** `dotnet_config_kit.Extensions`

Registers the configuration library services with dependency injection.

```csharp
public static IConfigurationSourceBuilder AddConfiguration(this IServiceCollection services)
```

**Parameters:**
- `services` (IServiceCollection): Service collection to register with.

**Returns:** Configuration builder for fluent configuration.

**Exceptions:**
- `ArgumentNullException`: When services is null

**Example:**
```csharp
var services = new ServiceCollection();

services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("MYAPP")
    .Build<AppSettings>();

var provider = services.BuildServiceProvider();
var settings = provider.GetRequiredService<AppSettings>();
```

---

## Fluent Builder Interface

### `IConfigurationSourceBuilder`

**Namespace:** `dotnet_config_kit.Extensions`

Fluent interface for registering configuration sources and binding options.

#### Members

```csharp
IConfigurationSourceBuilder AddJsonFile(string filePath)
```
Adds a JSON file source.

**Parameters:**
- `filePath` (string): Path to JSON file. Must not be null or empty.

**Returns:** Self for method chaining.

**Example:**
```csharp
.AddJsonFile("appsettings.json")
.AddJsonFile("appsettings.prod.json")  // Later overrides earlier
```

```csharp
IConfigurationSourceBuilder AddEnvironmentVariables(string? prefix = null)
```
Adds environment variable source with optional prefix filtering.

**Parameters:**
- `prefix` (string?): Optional prefix. Variables must start with `PREFIX_` to be included.

**Returns:** Self for method chaining.

**Remarks:**
- Variables are case-insensitive
- Single underscore after prefix is removed
- Double underscores are converted to dots
- Example: `MYAPP_DATABASE__HOST` → `database.host`

**Example:**
```csharp
.AddEnvironmentVariables("MYAPP")
// Matches: MYAPP_DATABASE_HOST, MYAPP_DEBUG, etc.
```

```csharp
IConfigurationSourceBuilder AddMemory(IEnumerable<KeyValuePair<string, string>> data, string? name = null)
```
Adds in-memory configuration source.

**Parameters:**
- `data` (IEnumerable<KeyValuePair>): Key-value pairs. Must not be null.
- `name` (string?): Optional source name for diagnostics.

**Returns:** Self for method chaining.

**Example:**
```csharp
.AddMemory(new Dictionary<string, string>
{
    { "database.host", "localhost" },
    { "database.port", "5432" }
}, "in-memory-defaults")
```

```csharp
IConfigurationSourceBuilder AddSource(IConfigSource source)
```
Adds a custom configuration source.

**Parameters:**
- `source` (IConfigSource): Custom source. Must not be null.

**Returns:** Self for method chaining.

**Example:**
```csharp
.AddSource(new DatabaseConfigSource())
.AddSource(new RemoteConfigServer("https://config.example.com"))
```

```csharp
IServiceCollection Build<T>() where T : class, new()
```
Binds all loaded configuration to a strongly-typed options class and registers it.

**Type Parameters:**
- `T` (class): Target type for configuration binding.

**Returns:** Service collection for further registration.

**Exceptions:**
- `InvalidOperationException`: When configuration binding fails.

**Example:**
```csharp
public class DatabaseOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    .Build<DatabaseOptions>();

// Now registered in DI:
// var options = serviceProvider.GetRequiredService<DatabaseOptions>();
```

```csharp
Task<IServiceCollection> BuildAsync<T>(CancellationToken cancellationToken = default) where T : class, new()
```
Asynchronously binds configuration and registers options.

**Type Parameters:**
- `T` (class): Target type for configuration binding.

**Parameters:**
- `cancellationToken` (CancellationToken): Cancellation token.

**Returns:** Task returning service collection.

**Example:**
```csharp
var services = new ServiceCollection();

await services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    .BuildAsync<AppSettings>();
```

---

## Built-in Implementations

### `JsonConfigParser`

**Namespace:** `dotnet_config_kit.Internal.Parsers`

Parses JSON format into flat key-value pairs.

**Features:**
- Nested objects → dot-notation keys
- Arrays → colon-indexed keys
- Type preservation (booleans, numbers)
- Null handling

**Usage:**
```csharp
var parser = new JsonConfigParser();
var config = parser.Parse(json);
```

---

### `FileConfigSource`

**Namespace:** `dotnet_config_kit.Internal.Sources`

Loads configuration from a file using a specified parser.

**Constructor:**
```csharp
public FileConfigSource(string filePath, IConfigParser parser)
```

**Remarks:**
- Returns empty dictionary if file doesn't exist
- Throws InvalidOperationException if file exists but parsing fails
- Supports both sync and async loading

**Example:**
```csharp
var source = new FileConfigSource("config.json", new JsonConfigParser());
var config = source.Load();
```

---

### `EnvironmentVariableConfigSource`

**Namespace:** `dotnet_config_kit.Internal.Sources`

Loads configuration from environment variables.

**Constructor:**
```csharp
public EnvironmentVariableConfigSource(string? prefix = null)
```

**Parameters:**
- `prefix` (string?): Optional prefix for filtering variables.

**Behavior:**
- Case-insensitive
- Single underscores become dots
- Double underscores become dots
- Prefix must be followed by underscore to match

**Example:**
```csharp
var source = new EnvironmentVariableConfigSource("MYAPP");
// Matches: MYAPP_HOST, MYAPP_PORT, MYAPP_DEBUG, etc.
// Not matched: OTHER_VAR, MYAPPHOST (wrong format)
```

---

### `MemoryConfigSource`

**Namespace:** `dotnet_config_kit.Internal.Sources`

Loads configuration from an in-memory dictionary.

**Constructor:**
```csharp
public MemoryConfigSource(IEnumerable<KeyValuePair<string, string>> data, string? name = null)
```

**Parameters:**
- `data` (IEnumerable<KeyValuePair>): Key-value pairs.
- `name` (string?): Optional source name.

**Usage:**
```csharp
var data = new Dictionary<string, string>
{
    { "key1", "value1" },
    { "key2", "value2" }
};

var source = new MemoryConfigSource(data, "test-data");
var config = source.Load();
```

---

### `ReflectionConfigBinder<T>`

**Namespace:** `dotnet_config_kit.Internal.Binding`

Binds flat configuration to strongly-typed objects using reflection.

**Type Parameters:**
- `T` (class): Target type. Must have parameterless constructor.

**Supported Types:**
- String, bool
- byte, short, int, long
- float, double, decimal
- Guid, DateTime, TimeSpan
- Enums (case-insensitive)
- Nullable versions of above
- Custom types with default constructors (via nested binding)

**Usage:**
```csharp
public class Settings
{
    public string? AppName { get; set; }
    public int Port { get; set; }
    public bool DebugMode { get; set; }
    public LogLevel Level { get; set; }
    public DatabaseSettings Database { get; set; } = new();
}

public class DatabaseSettings
{
    public string? Host { get; set; }
    public int Port { get; set; }
}

public enum LogLevel { Debug, Info, Warning, Error }

var binder = new ReflectionConfigBinder<Settings>();
var settings = binder.Bind(config);
```

---

## Configuration Key Formats

### Dot Notation (Objects)
Objects are flattened with dot-separated paths:
```json
{
  "database": {
    "connection": {
      "host": "localhost"
    }
  }
}
```
Becomes: `database.connection.host` → "localhost"

### Colon Notation (Arrays)
Array indices use colons with nested properties using dots:
```json
{
  "servers": [
    { "host": "server1.com", "port": 8080 },
    { "host": "server2.com", "port": 8081 }
  ]
}
```
Becomes:
- `servers:0.host` → "server1.com"
- `servers:0.port` → "8080"
- `servers:1.host` → "server2.com"
- `servers:1.port` → "8081"

### Environment Variables
Underscores become dots; double underscores also become dots:
```
MYAPP_DATABASE_HOST=localhost
MYAPP_DATABASE__PORT=5432
MYAPP_DEBUG=true
```
Becomes (with prefix "MYAPP"):
- `database.host` → "localhost"
- `database.port` → "5432"
- `debug` → "true"

---

## Error Handling

### Configuration Not Found
If a required key is missing during binding, the property uses its CLR default value (null for reference types, 0 for value types).

### Type Conversion Failure
When a value cannot be converted to the target type, binding throws `InvalidOperationException`:
```
InvalidOperationException: Configuration binding failed with 1 error(s):
  database.port: Failed to bind to type Int32: Cannot convert 'not_a_number' to int
```

### Validation Without Binding
To check for errors before binding:
```csharp
var errors = binder.GetValidationErrors(config);
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"{error.Path}: {error.Message} (value: {error.AttemptedValue})");
    }
}
```

---

## Thread Safety

- **Immutable:** Configuration is locked after loading. Cannot be modified.
- **Thread-Safe Reads:** Multiple threads can read configuration without locks.
- **Not Thread-Safe:** Custom sources should be thread-safe if called concurrently.

---

## Performance Tips

1. **Load Once:** Configuration is loaded once at startup. Use DI to share the instance.
2. **Async Loading:** Use `LoadAsync()` for I/O-bound sources (files, network).
3. **Eager Binding:** Bind at startup to catch errors early. Use `Build<T>()` in `Program.cs`.
4. **Cache Binders:** Create `IConfigBinder<T>` once, reuse for validation.
5. **Minimize Sources:** Each source adds load time. Consolidate where possible.

---

## Complete Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using dotnet_config_kit;
using dotnet_config_kit.Extensions;

public class AppSettings
{
    public string? AppName { get; set; }
    public int Port { get; set; } = 8080;
    public DatabaseSettings Database { get; set; } = new();
}

public class DatabaseSettings
{
    public string? Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string? Username { get; set; }
    public string? Password { get; set; }
}

// Program.cs
var services = new ServiceCollection();

services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json")
    .AddEnvironmentVariables("MYAPP")
    .Build<AppSettings>();

var provider = services.BuildServiceProvider();
var settings = provider.GetRequiredService<AppSettings>();

Console.WriteLine($"App: {settings.AppName}");
Console.WriteLine($"Port: {settings.Port}");
Console.WriteLine($"Database: {settings.Database.Host}:{settings.Database.Port}");
```

**appsettings.json:**
```json
{
  "appName": "MyApp",
  "port": 8080,
  "database": {
    "host": "localhost",
    "port": 5432,
    "username": "sa",
    "password": "changeme"
  }
}
```

**appsettings.prod.json:**
```json
{
  "database": {
    "host": "prod-db.example.com",
    "port": 5433
  }
}
```

**Environment:**
```bash
MYAPP_DATABASE_PASSWORD=SecurePassword123
```

---

Generated for **dotnet-config-kit v0.1.0**
