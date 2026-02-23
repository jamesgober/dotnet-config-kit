namespace dotnet_config_kit.Internal;

using dotnet_config_kit.Abstractions;
using System.Collections.Concurrent;

/// <summary>
/// Builds and aggregates configuration from multiple sources in registration order.
/// </summary>
internal sealed class ConfigBuilder : IConfigBuilder
{
    private readonly List<IConfigSource> _sources = new();
    private readonly ConfigurationProfile _profile = new();
    private MergeStrategy _mergeStrategy = MergeStrategy.LastWins;
    private IReadOnlyDictionary<string, string> _configuration = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Configuration => _configuration;

    /// <summary>
    /// Registers a configuration source.
    /// </summary>
    /// <param name="source">The source to register. Must not be null.</param>
    /// <returns>Self for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    internal ConfigBuilder AddSource(IConfigSource source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        _sources.Add(source);
        return this;
    }

    /// <summary>
    /// Sets the merge strategy for combining sources.
    /// </summary>
    internal void SetMergeStrategy(MergeStrategy strategy)
    {
        _mergeStrategy = strategy;
    }

    /// <summary>
    /// Sets the active configuration profile.
    /// </summary>
    internal void SetProfile(string? profile)
    {
        _profile.SetProfile(profile);
    }

    /// <summary>
    /// Auto-detects the configuration profile from environment variables.
    /// </summary>
    internal void AutoDetectProfile()
    {
        _profile.AutoDetectProfile();
    }

    /// <summary>
    /// Gets the current profile.
    /// </summary>
    internal string? CurrentProfile => _profile.CurrentProfile;

    /// <inheritdoc />
    public IConfigBuilder Load()
    {
        var sourceConfigs = new List<IReadOnlyDictionary<string, string>>();
        var merger = new ConfigurationMerger(_mergeStrategy);

        foreach (var source in _sources)
        {
            try
            {
                var sourceConfig = source.Load();
                if (sourceConfig != null && sourceConfig.Count > 0)
                {
                    sourceConfigs.Add(sourceConfig);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load configuration from source '{source.Name}': {ex.Message}");
            }
        }

        _configuration = merger.Merge(sourceConfigs);
        return this;
    }

    /// <inheritdoc />
    public async ValueTask<IConfigBuilder> LoadAsync(CancellationToken cancellationToken = default)
    {
        var sourceConfigs = new List<IReadOnlyDictionary<string, string>>();
        var merger = new ConfigurationMerger(_mergeStrategy);

        foreach (var source in _sources)
        {
            try
            {
                var sourceConfig = await source.LoadAsync(cancellationToken).ConfigureAwait(false);
                if (sourceConfig != null && sourceConfig.Count > 0)
                {
                    sourceConfigs.Add(sourceConfig);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load configuration from source '{source.Name}': {ex.Message}");
            }
        }

        _configuration = merger.Merge(sourceConfigs);
        return this;
    }
}
