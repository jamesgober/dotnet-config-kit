namespace dotnet_config_kit.Extensions;

using dotnet_config_kit.Abstractions;
using dotnet_config_kit.Internal;
using dotnet_config_kit.Internal.Binding;
using dotnet_config_kit.Internal.Parsers;
using dotnet_config_kit.Internal.Sources;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering configuration services with dependency injection.
/// </summary>
public static class ConfigurationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the configuration library services.
    /// </summary>
    /// <param name="services">The service collection to register with. Must not be null.</param>
    /// <returns>A builder for fluently configuring configuration sources.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IConfigurationSourceBuilder AddConfiguration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        var builder = new ConfigurationSourceBuilder(services);
        return builder;
    }
}

/// <summary>
/// Implementation of fluent builder for registering configuration sources.
/// </summary>
internal sealed class ConfigurationSourceBuilder : IConfigurationSourceBuilder
{
    private readonly IServiceCollection _services;
    private readonly ConfigBuilder _configBuilder = new();
    private readonly ConfigurationPresets _presets = new();

    internal ConfigurationSourceBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddJsonFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));
        _configBuilder.AddSource(new FileConfigSource(filePath, new JsonConfigParser()));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddJsonFile(string filePath, bool isOptional = false, bool enableHotReload = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));
        
        if (!isOptional && !File.Exists(filePath))
        {
            throw new InvalidOperationException($"JSON configuration file '{filePath}' not found and is not optional.");
        }

        if (!File.Exists(filePath))
        {
            return this; // Skip if optional and missing
        }

        var source = new FileConfigSource(filePath, new JsonConfigParser());
        
        if (enableHotReload)
        {
            var hotReloadSource = new HotReloadFileSource(source, true);
            _services.AddSingleton(hotReloadSource);
            _configBuilder.AddSource(hotReloadSource);
        }
        else
        {
            _configBuilder.AddSource(source);
        }
        
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddYamlFile(string filePath, bool isOptional = false, bool enableHotReload = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));
        
        if (!isOptional && !File.Exists(filePath))
        {
            throw new InvalidOperationException($"YAML configuration file '{filePath}' not found and is not optional.");
        }

        if (!File.Exists(filePath))
        {
            return this;
        }

        var source = new FileConfigSource(filePath, new YamlConfigParser());
        
        if (enableHotReload)
        {
            var hotReloadSource = new HotReloadFileSource(source, true);
            _services.AddSingleton(hotReloadSource);
            _configBuilder.AddSource(hotReloadSource);
        }
        else
        {
            _configBuilder.AddSource(source);
        }
        
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddTomlFile(string filePath, bool isOptional = false, bool enableHotReload = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));
        
        if (!isOptional && !File.Exists(filePath))
        {
            throw new InvalidOperationException($"TOML configuration file '{filePath}' not found and is not optional.");
        }

        if (!File.Exists(filePath))
        {
            return this;
        }

        var source = new FileConfigSource(filePath, new TomlConfigParser());
        
        if (enableHotReload)
        {
            var hotReloadSource = new HotReloadFileSource(source, true);
            _services.AddSingleton(hotReloadSource);
            _configBuilder.AddSource(hotReloadSource);
        }
        else
        {
            _configBuilder.AddSource(source);
        }
        
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddIniFile(string filePath, bool isOptional = false, bool enableHotReload = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));
        
        if (!isOptional && !File.Exists(filePath))
        {
            throw new InvalidOperationException($"INI configuration file '{filePath}' not found and is not optional.");
        }

        if (!File.Exists(filePath))
        {
            return this;
        }

        var source = new FileConfigSource(filePath, new IniConfigParser());
        
        if (enableHotReload)
        {
            var hotReloadSource = new HotReloadFileSource(source, true);
            _services.AddSingleton(hotReloadSource);
            _configBuilder.AddSource(hotReloadSource);
        }
        else
        {
            _configBuilder.AddSource(source);
        }
        
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddXmlFile(string filePath, bool isOptional = false, bool enableHotReload = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));
        
        if (!isOptional && !File.Exists(filePath))
        {
            throw new InvalidOperationException($"XML configuration file '{filePath}' not found and is not optional.");
        }

        if (!File.Exists(filePath))
        {
            return this;
        }

        var source = new FileConfigSource(filePath, new XmlConfigParser());
        
        if (enableHotReload)
        {
            var hotReloadSource = new HotReloadFileSource(source, true);
            _services.AddSingleton(hotReloadSource);
            _configBuilder.AddSource(hotReloadSource);
        }
        else
        {
            _configBuilder.AddSource(source);
        }
        
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddEnvironmentVariables(string? prefix = null)
    {
        _configBuilder.AddSource(new EnvironmentVariableConfigSource(prefix));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddMemory(IEnumerable<KeyValuePair<string, string>> data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));
        _configBuilder.AddSource(new MemoryConfigSource(data, name));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddCommandLineArguments(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        _configBuilder.AddSource(new CommandLineConfigSource(args));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddUserSecrets<T>() where T : class
    {
        _configBuilder.AddSource(UserSecretsConfigSource.FromAssembly<T>());
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddUserSecrets(string userSecretsId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userSecretsId, nameof(userSecretsId));
        _configBuilder.AddSource(new UserSecretsConfigSource(userSecretsId));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder WithProfile(string? profile)
    {
        _configBuilder.SetProfile(profile);
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder WithAutoProfile()
    {
        _configBuilder.AutoDetectProfile();
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddHttpSource(Uri endpoint, IConfigParser parser, int? pollIntervalSeconds = null)
    {
        ArgumentNullException.ThrowIfNull(endpoint, nameof(endpoint));
        ArgumentNullException.ThrowIfNull(parser, nameof(parser));

        var source = new HttpConfigSource(endpoint, parser, pollIntervalSeconds);
        _services.AddSingleton(source);
        _configBuilder.AddSource(source);

        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder WithMergeStrategy(MergeStrategy strategy)
    {
        _configBuilder.SetMergeStrategy(strategy);
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddLazySource(IConfigSource source, int? timeoutSeconds = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        var lazySource = new LazyConfigSource(source, timeoutSeconds);
        _configBuilder.AddSource(lazySource);

        return this;
    }

    /// <inheritdoc />
    public string ExportAsJson(bool indent = true)
    {
        return ConfigurationSerializer.ExportAsJson(_configBuilder.Configuration, indent);
    }

    /// <inheritdoc />
    public string ExportAsYaml()
    {
        return ConfigurationSerializer.ExportAsYaml(_configBuilder.Configuration);
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddSource(IConfigSource source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        _configBuilder.AddSource(source);
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddDefaults(IEnumerable<KeyValuePair<string, string>> defaults, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(defaults, nameof(defaults));
        
        // Defaults are added first so other sources can override them
        _configBuilder.AddSource(new MemoryConfigSource(defaults, name ?? "defaults"));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder RegisterPreset(string presetName, IEnumerable<KeyValuePair<string, string>> configuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(presetName, nameof(presetName));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in configuration)
        {
            dict[kvp.Key] = kvp.Value;
        }

        _presets.Register(presetName, dict);
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder UsePreset(string presetName)
    {
        ArgumentException.ThrowIfNullOrEmpty(presetName, nameof(presetName));

        var preset = _presets.Get(presetName);
        if (preset.Count == 0)
        {
            throw new InvalidOperationException($"Preset '{presetName}' is not registered. Available presets: {string.Join(", ", _presets.GetNames())}");
        }

        _configBuilder.AddSource(new MemoryConfigSource(preset, $"preset:{presetName}"));
        return this;
    }

    /// <inheritdoc />
    public IServiceCollection Build<T>() where T : class, new()
    {
        _configBuilder.Load();
        var configuration = _configBuilder.Configuration;

        var binder = new ReflectionConfigBinder<T>();
        var bindingErrors = binder.GetValidationErrors(configuration);
        if (bindingErrors.Count > 0)
        {
            var errorMessages = string.Join(Environment.NewLine, bindingErrors.Select(e => $"  {e.Path}: {e.Message}"));
            throw new InvalidOperationException($"Configuration validation failed with {bindingErrors.Count} error(s):{Environment.NewLine}{errorMessages}");
        }

        var options = binder.Bind(configuration);
        _services.AddSingleton(options);
        _services.AddSingleton<IConfigBinder<T>>(binder);

        return _services;
    }

    /// <inheritdoc />
    public async Task<IServiceCollection> BuildAsync<T>(CancellationToken cancellationToken = default) where T : class, new()
    {
        await _configBuilder.LoadAsync(cancellationToken).ConfigureAwait(false);
        var configuration = _configBuilder.Configuration;

        var binder = new ReflectionConfigBinder<T>();
        var bindingErrors = binder.GetValidationErrors(configuration);
        if (bindingErrors.Count > 0)
        {
            var errorMessages = string.Join(Environment.NewLine, bindingErrors.Select(e => $"  {e.Path}: {e.Message}"));
            throw new InvalidOperationException($"Configuration validation failed with {bindingErrors.Count} error(s):{Environment.NewLine}{errorMessages}");
        }

        var options = binder.Bind(configuration);
        _services.AddSingleton(options);
        _services.AddSingleton<IConfigBinder<T>>(binder);

        return _services;
    }
}
