namespace dotnet_config_kit.Internal;

using dotnet_config_kit.Abstractions;
using System.Collections.Concurrent;

/// <summary>
/// Builds and aggregates configuration from multiple sources in registration order.
/// </summary>
internal sealed class ConfigBuilder : IConfigBuilder
{
    private readonly List<IConfigSource> _sources = new();
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

    /// <inheritdoc />
    public IConfigBuilder Load()
    {
        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in _sources)
        {
            try
            {
                var sourceConfig = source.Load();
                if (sourceConfig != null && sourceConfig.Count > 0)
                {
                    foreach (var kvp in sourceConfig)
                    {
                        merged[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load configuration from source '{source.Name}': {ex.Message}");
            }
        }

        _configuration = merged;
        return this;
    }

    /// <inheritdoc />
    public async ValueTask<IConfigBuilder> LoadAsync(CancellationToken cancellationToken = default)
    {
        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in _sources)
        {
            try
            {
                var sourceConfig = await source.LoadAsync(cancellationToken).ConfigureAwait(false);
                if (sourceConfig != null && sourceConfig.Count > 0)
                {
                    foreach (var kvp in sourceConfig)
                    {
                        merged[kvp.Key] = kvp.Value;
                    }
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

        _configuration = merged;
        return this;
    }
}
