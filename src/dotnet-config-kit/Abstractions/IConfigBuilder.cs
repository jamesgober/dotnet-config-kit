namespace dotnet_config_kit.Abstractions;

/// <summary>
/// Loads and aggregates configuration from multiple sources.
/// </summary>
/// <remarks>
/// This is the primary entry point for configuration loading. Sources are loaded in registration order,
/// with later sources overriding earlier ones (last write wins). Supports synchronous and asynchronous loading.
/// Thread-safe for loading, but configuration should not be mutated during iteration.
/// </remarks>
public interface IConfigBuilder
{
    /// <summary>
    /// Gets the aggregated configuration as a flat key-value dictionary.
    /// </summary>
    /// <remarks>
    /// Keys are lowercase. Values are strings. Empty dictionary if no sources have been loaded.
    /// The returned dictionary is a snapshot; subsequent source changes do not affect it.
    /// </remarks>
    IReadOnlyDictionary<string, string> Configuration { get; }

    /// <summary>
    /// Loads configuration from all registered sources synchronously.
    /// </summary>
    /// <returns>Self for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when loading fails for a required source.</exception>
    /// <remarks>
    /// Sources are loaded in order. Later sources override earlier ones. Partial failures are logged
    /// but do not prevent subsequent sources from loading (unless configured as required).
    /// </remarks>
    IConfigBuilder Load();

    /// <summary>
    /// Asynchronously loads configuration from all registered sources.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Self for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when loading fails for a required source.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is cancelled.</exception>
    /// <remarks>
    /// Sources are loaded concurrently where possible, but ordering semantics are preserved
    /// (later sources override earlier ones, regardless of actual completion order).
    /// </remarks>
    ValueTask<IConfigBuilder> LoadAsync(CancellationToken cancellationToken = default);
}
