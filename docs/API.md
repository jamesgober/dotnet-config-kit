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

### `IConfigValueConverter<T>` ⭐ NEW in v0.3.0

**Namespace:** `dotnet_config_kit.Abstractions`

Contract for converting configuration string values to strongly-typed values.

**Type Parameters:**
- `T`: The target type to convert to.

#### Members

```csharp
T Convert(string? value)
```
Converts a string value to the target type.

**Parameters:**
- `value` (string?): The string value to convert. May be null or empty.

**Returns:** The converted value.

**Exceptions:**
- `ArgumentException`: Thrown when the value cannot be converted.

**Example:**
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

// Register and use
var binder = new EnhancedReflectionConfigBinder<AppSettings>();
binder.RegisterConverter<Uri>(new UriConverter());
var settings = binder.Bind(config);
```

---

### `EnhancedReflectionConfigBinder<T>` ⭐ NEW in v0.3.0

**Namespace:** `dotnet_config_kit.Internal.Binding`

Enhanced reflection-based binder with custom converters and DataAnnotations validation support.

**Type Parameters:**
- `T` (class): The target type to bind configuration to.

#### Members

```csharp
void RegisterConverter<TValue>(IConfigValueConverter<TValue> converter)
```
Registers a custom type converter.

**Type Parameters:**
- `TValue`: The type to convert to.

**Parameters:**
- `converter` (IConfigValueConverter<TValue>): The converter implementation.

**Example:**
```csharp
var binder = new EnhancedReflectionConfigBinder<AppSettings>();
binder.RegisterConverter<IPAddress>(s => IPAddress.Parse(s));
binder.RegisterConverter<Uri>(new UriConverter());

var settings = binder.Bind(config);

```

---

## New Extension Methods in v0.3.0

Added additional methods to the configuration builder interface for improved functionality.

### Command-Line Arguments

```csharp
IConfigurationSourceBuilder AddCommandLineArguments(string[] args)
```
Adds a command-line arguments source.

**Parameters:**
- `args` (string[]): Command-line arguments from Main(). Must not be null.

**Returns:** Self for method chaining.

**Remarks:**
- Supports `--key=value`, `--key value`, and `-k value` formats
- Flags without values default to "true": `--flag` → `"true"`
- Later sources override earlier ones (command-line highest priority)

**Example:**
```csharp
.AddCommandLineArguments(args)
// Usage: app --database-host=localhost --port 5432 --debug
```

---

### User Secrets

```csharp
IConfigurationSourceBuilder AddUserSecrets<T>() where T : class
```
Adds a user secrets source using assembly's UserSecretsIdAttribute.

**Type Parameters:**
- `T` (class): The assembly type to scan for UserSecretsIdAttribute.

**Returns:** Self for method chaining.

**Exceptions:**
- `InvalidOperationException`: When UserSecretsIdAttribute is not found.

**Example:**
```csharp
// Requires: [assembly: UserSecretsId("my-app-id")]
.AddUserSecrets<Program>()
```

---

```csharp
IConfigurationSourceBuilder AddUserSecrets(string userSecretsId)
```
Adds a user secrets source with explicit UserSecretsId.

**Parameters:**
- `userSecretsId` (string): The UserSecretsId. Must not be null or empty.

**Returns:** Self for method chaining.

**Remarks:** Loads from `~/.microsoft/usersecrets/{userSecretsId}/secrets.json`

**Example:**
```csharp
.AddUserSecrets("my-app-id")
// Searches: ~/.microsoft/usersecrets/my-app-id/secrets.json
```

---

### Configuration Profile

```csharp
IConfigurationSourceBuilder WithProfile(string? profile)
```
Sets the configuration profile for environment-specific settings.

**Parameters:**
- `profile` (string?): The profile name (e.g., "development", "production").

**Returns:** Self for method chaining.

**Example:**
```csharp
.WithProfile("production")
.AddJsonFile("appsettings.{profile}.json")
// Loads: appsettings.production.json
```

---

```csharp
IConfigurationSourceBuilder WithAutoProfile()
```
Auto-detects the configuration profile from environment variables.

**Checks:**
- `ASPNETCORE_ENVIRONMENT`
- `ENVIRONMENT`
- `DOTNET_ENVIRONMENT`

**Returns:** Self for method chaining.

**Example:**
```csharp
.WithAutoProfile()
// If ASPNETCORE_ENVIRONMENT=production → profile="production"
.AddJsonFile("appsettings.{profile}.json")
// Loads: appsettings.production.json
