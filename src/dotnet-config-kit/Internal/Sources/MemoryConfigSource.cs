namespace dotnet_config_kit.Internal.Sources;

using dotnet_config_kit.Abstractions;

/// <summary>
/// Loads configuration from an in-memory dictionary.
/// </summary>
public sealed class MemoryConfigSource : IConfigSource
{
    private readonly Dictionary<string, string> _data;

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Creates a new memory-based configuration source.
    /// </summary>
    /// <param name="data">The configuration data. Must not be null. Keys are case-insensitive.</param>
    /// <param name="name">Optional name for this source. Defaults to "Memory".</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public MemoryConfigSource(IEnumerable<KeyValuePair<string, string>> data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        _data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in data)
        {
            if (kvp.Key != null && kvp.Value != null)
            {
                _data[kvp.Key] = kvp.Value;
            }
        }

        Name = name ?? "Memory";
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Load()
    {
        return new Dictionary<string, string>(_data, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<IReadOnlyDictionary<string, string>>(Load());
    }
}
