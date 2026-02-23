namespace dotnet_config_kit.Internal;

/// <summary>
/// Manages preset configurations for reuse across multiple configurations.
/// </summary>
public sealed class ConfigurationPresets
{
    private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _presets = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a preset configuration.
    /// </summary>
    /// <param name="name">The preset name (e.g., "development", "production").</param>
    /// <param name="configuration">The preset configuration dictionary.</param>
    public void Register(string name, IReadOnlyDictionary<string, string> configuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        _presets[name] = configuration;
    }

    /// <summary>
    /// Gets a registered preset configuration.
    /// </summary>
    /// <param name="name">The preset name.</param>
    /// <returns>The preset configuration, or empty dictionary if not found.</returns>
    public IReadOnlyDictionary<string, string> Get(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        if (_presets.TryGetValue(name, out var config))
        {
            return config;
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if a preset is registered.
    /// </summary>
    /// <param name="name">The preset name.</param>
    /// <returns>True if the preset exists, false otherwise.</returns>
    public bool Exists(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        return _presets.ContainsKey(name);
    }

    /// <summary>
    /// Gets all registered preset names.
    /// </summary>
    /// <returns>Collection of preset names.</returns>
    public IReadOnlyCollection<string> GetNames()
    {
        return _presets.Keys;
    }
}
