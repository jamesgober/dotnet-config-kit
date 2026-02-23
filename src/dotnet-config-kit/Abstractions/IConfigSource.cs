namespace dotnet_config_kit.Abstractions;

/// <summary>
/// Provides configuration from a specific source.
/// </summary>
/// <remarks>
/// Sources can be files, environment variables, memory, command-line arguments, or any external provider.
/// Implementations are responsible for loading data and converting it to a flat key-value dictionary.
/// Thread-safe. May be called multiple times across different instances.
/// </remarks>
public interface IConfigSource
{
    /// <summary>
    /// Gets a human-readable name for this source (e.g., "appsettings.json", "Environment Variables").
    /// </summary>
    /// <remarks>
    /// Used for diagnostics, logging, and error reporting. Should uniquely identify the source.
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Loads configuration from this source.
    /// </summary>
    /// <returns>
    /// A dictionary of configuration keys and values in flat dot-notation format.
    /// Empty dictionary if the source is unavailable or empty.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the source is misconfigured or unavailable.</exception>
    /// <remarks>
    /// Graceful degradation is preferred: if a source is optional and unavailable, return an empty dictionary
    /// rather than throwing. Only throw for configuration errors or required sources that cannot be loaded.
    /// </remarks>
    IReadOnlyDictionary<string, string> Load();

    /// <summary>
    /// Asynchronously loads configuration from this source.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A task that returns a dictionary of configuration keys and values when complete.
    /// Empty dictionary if the source is unavailable or empty.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the source is misconfigured or unavailable.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is cancelled.</exception>
    /// <remarks>
    /// Graceful degradation is preferred: if a source is optional and unavailable, return an empty dictionary.
    /// Timeout calls that may hang indefinitely (e.g., remote sources). Maximum wait is source-dependent.
    /// </remarks>
    ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken = default);
}
