namespace dotnet_config_kit.Abstractions;

/// <summary>
/// Parses configuration from a specific format into a flat key-value dictionary.
/// </summary>
/// <remarks>
/// All implementations must normalize keys to dot-notation format (e.g., "section.subsection.key").
/// Values are stored as strings. The parser must handle edge cases: null, empty strings,
/// deeply nested structures, and duplicate keys (last occurrence wins).
/// Thread-safe. May be called multiple times.
/// </remarks>
public interface IConfigParser
{
    /// <summary>
    /// Gets the format this parser handles (e.g., "json", "yaml", "toml").
    /// </summary>
    /// <remarks>
    /// Used for diagnostics and format-specific routing. Case-insensitive comparison is recommended.
    /// </remarks>
    string Format { get; }

    /// <summary>
    /// Parses configuration content into a flat key-value dictionary.
    /// </summary>
    /// <param name="content">The raw configuration content to parse. Must not be null or empty.</param>
    /// <returns>
    /// A dictionary of flat key-value pairs where keys are in dot-notation format.
    /// Empty dictionary if content is empty or contains no configuration data.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when content is invalid for the format.</exception>
    /// <exception cref="InvalidOperationException">Thrown when parsing fails for reasons other than format invalidity.</exception>
    /// <remarks>
    /// The returned dictionary owns the keys and values; the parser must not retain references.
    /// All keys are converted to lowercase for case-insensitive access.
    /// </remarks>
    IReadOnlyDictionary<string, string> Parse(string content);

    /// <summary>
    /// Asynchronously parses configuration content from a stream.
    /// </summary>
    /// <param name="stream">The stream to read configuration from. Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A task that returns a dictionary of flat key-value pairs when complete.
    /// Empty dictionary if stream is empty or contains no configuration data.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when content is invalid for the format.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is cancelled.</exception>
    /// <remarks>
    /// No seek behavior is guaranteed. The stream position after completion is undefined.
    /// For best performance, use buffered streams.
    /// </remarks>
    ValueTask<IReadOnlyDictionary<string, string>> ParseAsync(Stream stream, CancellationToken cancellationToken = default);
}
