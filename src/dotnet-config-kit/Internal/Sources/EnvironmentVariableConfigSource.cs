namespace dotnet_config_kit.Internal.Sources;

using dotnet_config_kit.Abstractions;

/// <summary>
/// Loads configuration from environment variables with an optional prefix.
/// </summary>
public sealed class EnvironmentVariableConfigSource : IConfigSource
{
    private readonly string? _prefix;

    /// <inheritdoc />
    public string Name => string.IsNullOrEmpty(_prefix) ? "Environment Variables" : $"Environment Variables ({_prefix}_)";

    /// <summary>
    /// Creates a new environment variable configuration source.
    /// </summary>
    /// <param name="prefix">Optional prefix for filtering environment variables. If provided, only variables starting with this prefix (followed by underscore) are included.</param>
    public EnvironmentVariableConfigSource(string? prefix = null)
    {
        _prefix = prefix;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Load()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var envVars = Environment.GetEnvironmentVariables();

        foreach (System.Collections.DictionaryEntry entry in envVars)
        {
            var key = entry.Key as string;
            var value = entry.Value as string;

            if (key != null && value != null)
            {
                if (_prefix != null)
                {
                    var expectedPrefix = $"{_prefix}_";
                    if (!key.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    key = key.Substring(expectedPrefix.Length);
                }

                var flatKey = key.Replace("__", ".", StringComparison.Ordinal).Replace("_", ".", StringComparison.Ordinal);
                result[flatKey] = value;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<IReadOnlyDictionary<string, string>>(Load());
    }
}
