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

## v0.5.0 New Features

### `ConfigurationPresets` ⭐ NEW

**Namespace:** `dotnet_config_kit.Internal`

Manages preset configurations for reuse across multiple configurations.

#### Members

```csharp
void Register(string name, IReadOnlyDictionary<string, string> configuration)
```
Registers a preset configuration.

**Parameters:**
- `name` (string): The preset name (e.g., "development", "production").
- `configuration` (IReadOnlyDictionary): The preset configuration dictionary.

```csharp
IReadOnlyDictionary<string, string> Get(string name)
```
Gets a registered preset configuration.

**Parameters:**
- `name` (string): The preset name.

**Returns:** The preset configuration, or empty dictionary if not found.

```csharp
bool Exists(string name)
```
Checks if a preset is registered.

**Parameters:**
- `name` (string): The preset name.

**Returns:** True if the preset exists, false otherwise.

**Example:**
```csharp
var presets = new ConfigurationPresets();
presets.Register("dev", new Dictionary<string, string>
{
    { "database.host", "localhost" },
    { "logging.level", "Debug" }
});

var devConfig = presets.Get("dev");
var isDev = presets.Exists("dev");  // true
```

---

## v0.5.0 Extension Methods

### Default Configuration Values

```csharp
IConfigurationSourceBuilder AddDefaults(
    IEnumerable<KeyValuePair<string, string>> defaults, 
    string? name = null)
```

Adds default configuration values that serve as a fallback. Defaults are applied first, so other sources can override them.

**Parameters:**
- `defaults` (IEnumerable<KeyValuePair>): The default configuration key-value pairs.
- `name` (string?): Optional name for this defaults source.

**Returns:** Self for method chaining.

**Usage:**
```csharp
services
    .AddConfiguration()
    .AddDefaults(new Dictionary<string, string>
    {
        { "api.timeout", "30" },
        { "logging.level", "Info" },
        { "features.caching", "true" }
    })
    .AddJsonFile("appsettings.json", isOptional: true)
    .AddEnvironmentVariables("APP")
    .Build<AppSettings>();
```

**Behavior:**
- Defaults are applied FIRST
- Later sources (environment variables, files, CLI args) override defaults
- Missing keys in later sources fall back to defaults
- Perfect for development environments with optional production overrides

---

### Configuration Presets

```csharp
IConfigurationSourceBuilder RegisterPreset(
    string presetName, 
    IEnumerable<KeyValuePair<string, string>> configuration)
```

Registers a configuration preset for reuse.

**Parameters:**
- `presetName` (string): The name of the preset (e.g., "development", "production").
- `configuration` (IEnumerable<KeyValuePair>): The preset configuration.

**Returns:** Self for method chaining.

---

```csharp
IConfigurationSourceBuilder UsePreset(string presetName)
```
Loads a registered preset configuration.

**Parameters:**
- `presetName` (string): The name of the preset to load.

**Returns:** Self for method chaining.

**Throws:** `InvalidOperationException` when preset is not registered.

**Usage:**
```csharp
services
    .AddConfiguration()
    .RegisterPreset("development", devDefaults)
    .RegisterPreset("production", prodDefaults)
    .RegisterPreset("staging", stagingDefaults)
    .UsePreset(Environment.GetEnvironmentVariable("APP_ENV") ?? "development")
    .AddJsonFile("appsettings.json", isOptional: true)
    .AddEnvironmentVariables("APP")
    .Build<AppSettings>();
```

**When to Use:**
- Multiple environment configurations
- Team preset configurations (e.g., each developer has a preset)
- Template-based configurations
- Feature flags per environment

---

## Complete v0.5.0 File Source Methods

All file source methods now support both `isOptional` and `enableHotReload` parameters:

```csharp
IConfigurationSourceBuilder AddJsonFile(string filePath, bool isOptional = false, bool enableHotReload = false)
IConfigurationSourceBuilder AddYamlFile(string filePath, bool isOptional = false, bool enableHotReload = false)
IConfigurationSourceBuilder AddTomlFile(string filePath, bool isOptional = false, bool enableHotReload = false)
IConfigurationSourceBuilder AddIniFile(string filePath, bool isOptional = false, bool enableHotReload = false)
IConfigurationSourceBuilder AddXmlFile(string filePath, bool isOptional = false, bool enableHotReload = false)
```

**Parameters:**
- `filePath` (string): Path to the configuration file.
- `isOptional` (bool): If true, ignores missing files. Default: false.
- `enableHotReload` (bool): If true, watches for changes and reloads. Default: false.

**Examples:**
```csharp
// Required file, no hot-reload
.AddJsonFile("appsettings.json")

// Optional file with hot-reload
.AddYamlFile("features.yaml", isOptional: true, enableHotReload: true)

// Required base config, optional environment override
.AddJsonFile("appsettings.json")
.AddJsonFile("appsettings.local.json", isOptional: true)

// Profile-specific files that may not exist
.AddJsonFile("appsettings.{profile}.json", isOptional: true, enableHotReload: true)
