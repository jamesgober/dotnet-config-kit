# Advanced Usage Guide — dotnet-config-kit

This guide covers complex configuration scenarios and best practices for production environments.

---

## Multi-Environment Configuration with Presets

### Pattern: Development vs Production

Use presets to manage completely different configurations per environment without environment variables:

```csharp
var devConfig = new Dictionary<string, string>
{
    { "database.host", "localhost" },
    { "database.port", "5432" },
    { "api.timeout", "30" },
    { "logging.level", "Debug" },
    { "cache.ttl", "60" }
};

var prodConfig = new Dictionary<string, string>
{
    { "database.host", "prod-db.internal" },
    { "database.port", "5432" },
    { "api.timeout", "5" },
    { "logging.level", "Error" },
    { "cache.ttl", "3600" }
};

services
    .AddConfiguration()
    .RegisterPreset("development", devConfig)
    .RegisterPreset("production", prodConfig)
    .UsePreset(GetEnvironment())  // Your environment detection logic
    .AddJsonFile("appsettings.json", isOptional: true)  // Override preset
    .AddEnvironmentVariables("APP")  // Override file
    .Build<AppSettings>();
```

### Pattern: Team-Based Presets

Each developer has a local preset with their machine-specific settings:

```csharp
// Shared defaults for the team
var teamDefaults = new Dictionary<string, string>
{
    { "api.url", "https://api.dev.local" },
    { "cache.enabled", "true" },
    { "database.pool.size", "20" }
};

// Individual developer overrides
var alicePreset = new Dictionary<string, string>
{
    { "database.host", "localhost" },
    { "database.port", "5433" },  // Different port on her machine
    { "logging.level", "Trace" }
};

services
    .AddConfiguration()
    .AddDefaults(teamDefaults)
    .RegisterPreset("alice", alicePreset)
    .RegisterPreset("bob", bobPreset)
    .UsePreset(Environment.GetEnvironmentVariable("DEV_PRESET") ?? "default")
    .AddJsonFile("appsettings.dev.json", isOptional: true)
    .Build<AppSettings>();
```

---

## Configuration Composition Strategies

### Pattern: Layered Configuration

Build configuration in specific order of precedence:

```csharp
services
    .AddConfiguration()
    // Layer 1: Global defaults (lowest priority)
    .AddDefaults(globalDefaults)
    
    // Layer 2: Environment defaults
    .UsePreset(environment)
    
    // Layer 3: Config files
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json", isOptional: true)
    .AddJsonFile("appsettings.local.json", isOptional: true, enableHotReload: true)
    
    // Layer 4: Environment variables (override everything except CLI)
    .AddEnvironmentVariables("APP")
    
    // Layer 5: Command-line arguments (highest priority)
    .AddCommandLineArguments(args)
    
    .Build<AppSettings>();
```

**Precedence Order (highest to lowest):**
1. Command-line arguments
2. Environment variables
3. Local configuration files (with hot-reload)
4. Environment-specific files
5. Preset configurations
6. Default values
7. POCO property initializers

### Pattern: Conditional Source Loading

Load different sources based on conditions:

```csharp
var builder = services.AddConfiguration();

// Always add defaults and JSON
builder
    .AddDefaults(defaults)
    .AddJsonFile("appsettings.json");

// Load environment-specific files if they exist
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (environment == "Development")
{
    builder.AddJsonFile("appsettings.dev.json", isOptional: true, enableHotReload: true);
}
else if (environment == "Production")
{
    builder.AddJsonFile("appsettings.prod.json", isOptional: true);
}

// Add user secrets only in development
if (environment != "Production")
{
    builder.AddUserSecrets<Program>(isOptional: true);
}

// Add remote config if URL is set
var configUrl = Environment.GetEnvironmentVariable("CONFIG_URL");
if (!string.IsNullOrEmpty(configUrl))
{
    builder.AddHttpSource(
        new Uri(configUrl),
        new JsonConfigParser(),
        pollIntervalSeconds: 60);
}

builder.Build<AppSettings>();
```

---

## Validation & Error Handling

### Pattern: Pre-Flight Validation

Validate configuration before using it:

```csharp
var binder = new EnhancedReflectionConfigBinder<AppSettings>();

// Get validation errors without binding
var errors = binder.GetValidationErrors(loadedConfiguration);

if (errors.Count > 0)
{
    Console.WriteLine("Configuration validation failed:");
    foreach (var error in errors)
    {
        Console.WriteLine($"  {error.Path}: {error.Message}");
        if (error.AttemptedValue != null)
        {
            Console.WriteLine($"    Attempted: {error.AttemptedValue}");
        }
    }
    Environment.Exit(1);
}

// Configuration is valid, safe to bind
var settings = binder.Bind(loadedConfiguration);
services.AddSingleton(settings);
```

### Pattern: Custom Validation

Extend validation with business rules:

```csharp
public class AppSettings
{
    [Required]
    public string DatabaseUrl { get; set; }
    
    [Range(1, 100)]
    public int MaxThreads { get; set; }
}

public class AppSettingsValidator
{
    public List<string> Validate(AppSettings settings)
    {
        var errors = new List<string>();
        
        // Standard validation via attributes (automatic)
        
        // Custom business logic
        if (settings.DatabaseUrl.Contains("localhost") && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
        {
            errors.Add("Cannot use localhost database in production");
        }
        
        return errors;
    }
}

// Usage
var settings = provider.GetRequiredService<AppSettings>();
var validator = new AppSettingsValidator();
var validationErrors = validator.Validate(settings);

if (validationErrors.Count > 0)
{
    throw new InvalidOperationException($"Configuration validation failed: {string.Join(", ", validationErrors)}");
}
```

---

## Performance Optimization

### Pattern: Lazy Loading for Remote Config

Don't block startup waiting for remote configuration:

```csharp
services
    .AddConfiguration()
    .AddJsonFile("appsettings.json")  // Fast local file
    .AddLazySource(
        new HttpConfigSource(
            new Uri("https://config.example.com/settings"),
            new JsonConfigParser()),
        timeoutSeconds: 10)  // Load async, timeout after 10s
    .Build<AppSettings>();
```

### Pattern: Configuration Caching

Configuration is cached after loading—reads are zero-copy:

```csharp
// First access: loads and parses
var config1 = provider.GetRequiredService<AppSettings>();

// Subsequent accesses: return cached instance
var config2 = provider.GetRequiredService<AppSettings>();

// Same object (unless hot-reload triggered reload)
Assert.Same(config1, config2);
```

### Pattern: Exporting for Debugging

Export loaded configuration for inspection:

```csharp
var configBuilder = services.AddConfiguration()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("APP");

// See what the final merged configuration looks like
var json = configBuilder.ExportAsJson(indent: true);
var yaml = configBuilder.ExportAsYaml();

// Log or save for debugging
Console.WriteLine("Final Configuration:");
Console.WriteLine(json);

File.WriteAllText("config-export.json", json);
```

---

## Custom Type Converters

### Example: Converting to Complex Types

```csharp
public class UriConverter : IConfigValueConverter<Uri>
{
    public Uri Convert(string? value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("URI cannot be empty");
        
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            throw new ArgumentException($"Invalid URI: {value}");
        
        return uri;
    }
}

public class AppSettings
{
    public Uri ApiEndpoint { get; set; }
    public Uri WebhookUrl { get; set; }
}

// Register converter
var binder = new EnhancedReflectionConfigBinder<AppSettings>();
binder.RegisterConverter<Uri>(new UriConverter());

var settings = binder.Bind(configuration);
```

---

## Hot-Reload Patterns

### Pattern: Reacting to Configuration Changes

```csharp
var hotReloadSource = provider.GetRequiredService<HotReloadFileSource>();

hotReloadSource.OnChange(newConfiguration =>
{
    Console.WriteLine("Configuration changed, reloading...");
    
    // Rebind to new settings
    var binder = new ReflectionConfigBinder<AppSettings>();
    var updatedSettings = binder.Bind(newConfiguration);
    
    // Update singleton or notify observers
    var settingsProvider = provider.GetRequiredService<ISettingsProvider>();
    settingsProvider.Update(updatedSettings);
});
```

### Pattern: Feature Flag Hot-Reload

```csharp
public class FeatureFlags
{
    public bool NewUiEnabled { get; set; }
    public bool BetaFeature { get; set; }
    public bool MaintenanceMode { get; set; }
}

// Monitor features.json for changes
var hotReload = provider.GetRequiredService<HotReloadFileSource>();
hotReload.OnChange(config =>
{
    var binder = new ReflectionConfigBinder<FeatureFlags>();
    var flags = binder.Bind(config);
    
    // Notify feature service
    var featureService = provider.GetRequiredService<IFeatureService>();
    featureService.UpdateFlags(flags);
});
```

---

## Merging Strategies

### Understanding Merge Strategies

```csharp
// Given two sources:
var source1 = new Dictionary<string, string>
{
    { "db.host", "localhost" },
    { "db.port", "5432" },
    { "cache.enabled", "true" }
};

var source2 = new Dictionary<string, string>
{
    { "db.port", "3306" },  // Override
    { "api.timeout", "30" }
};

// LastWins (default): source2 overrides source1
//   Result: host=localhost, port=3306, cache=true, timeout=30
.WithMergeStrategy(MergeStrategy.LastWins)

// FirstWins: source1 takes precedence
//   Result: host=localhost, port=5432, cache=true, timeout=30
.WithMergeStrategy(MergeStrategy.FirstWins)

// Merge: identical values OK, different values error
//   Result: Error because port differs (5432 vs 3306)
.WithMergeStrategy(MergeStrategy.Merge)

// Throw: any key in multiple sources is an error
//   Result: Error because port appears in both
.WithMergeStrategy(MergeStrategy.Throw)
```

---

## Best Practices

1. **Always validate configuration** — Use DataAnnotations or custom validators
2. **Use defaults for optional settings** — Makes configuration optional graceful
3. **Layer configuration by precedence** — Defaults → Files → Env → CLI
4. **Monitor hot-reload** — React to configuration changes appropriately
5. **Export for debugging** — Use ExportAsJson() to see final merged config
6. **Test with presets** — Use presets for consistent test environments
7. **Timeout remote sources** — Don't block startup on network calls
8. **Use lazy loading** — For optional/slow sources like HTTP endpoints
9. **Document your configuration** — Include examples in your project
10. **Review merge strategy** — Choose the right strategy for your needs
