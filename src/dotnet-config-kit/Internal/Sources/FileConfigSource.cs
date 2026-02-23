namespace dotnet_config_kit.Internal.Sources;

using dotnet_config_kit.Abstractions;

/// <summary>
/// Loads configuration from a file using a specified parser.
/// </summary>
public sealed class FileConfigSource : IConfigSource
{
    private readonly string _filePath;
    private readonly IConfigParser _parser;

    /// <inheritdoc />
    public string Name => $"File: {_filePath}";

    /// <summary>
    /// Creates a new file-based configuration source.
    /// </summary>
    /// <param name="filePath">The path to the configuration file. Must not be null or empty.</param>
    /// <param name="parser">The parser to use for the file format. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when filePath is empty.</exception>
    public FileConfigSource(string filePath, IConfigParser parser)
    {
        ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));
        ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));
        ArgumentNullException.ThrowIfNull(parser, nameof(parser));

        _filePath = filePath;
        _parser = parser;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Load()
    {
        if (!File.Exists(_filePath))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var content = File.ReadAllText(_filePath);
            return _parser.Parse(content);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"Failed to load configuration from file '{_filePath}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load configuration from file '{_filePath}'", ex);
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            return await _parser.ParseAsync(stream, cancellationToken).ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"Failed to load configuration from file '{_filePath}': {ex.Message}", ex);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load configuration from file '{_filePath}'", ex);
        }
    }
}
