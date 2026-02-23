namespace dotnet_config_kit.Internal;

/// <summary>
/// Strategy for merging configuration from multiple sources.
/// </summary>
public enum MergeStrategy
{
    /// <summary>
    /// Later sources override earlier ones (default behavior).
    /// </summary>
    LastWins = 0,

    /// <summary>
    /// First source to define a key wins; later sources are ignored.
    /// </summary>
    FirstWins = 1,

    /// <summary>
    /// Merge all sources; if a key exists in multiple sources, an exception is thrown.
    /// </summary>
    Merge = 2,

    /// <summary>
    /// Throw an exception if any key appears in multiple sources.
    /// </summary>
    Throw = 3
}

/// <summary>
/// Merger for combining configuration from multiple sources.
/// </summary>
internal sealed class ConfigurationMerger
{
    private readonly MergeStrategy _strategy;

    public ConfigurationMerger(MergeStrategy strategy = MergeStrategy.LastWins)
    {
        _strategy = strategy;
    }

    /// <summary>
    /// Merges multiple configuration dictionaries according to the merge strategy.
    /// </summary>
    /// <param name="sources">The configuration sources to merge.</param>
    /// <returns>The merged configuration dictionary.</returns>
    /// <exception cref="InvalidOperationException">Thrown when merge conflicts are detected with Throw or Merge strategies.</exception>
    public Dictionary<string, string> Merge(IEnumerable<IReadOnlyDictionary<string, string>> sources)
    {
        ArgumentNullException.ThrowIfNull(sources, nameof(sources));

        return _strategy switch
        {
            MergeStrategy.LastWins => MergeLastWins(sources),
            MergeStrategy.FirstWins => MergeFirstWins(sources),
            MergeStrategy.Merge => MergeStrict(sources),
            MergeStrategy.Throw => MergeThrow(sources),
            _ => MergeLastWins(sources)
        };
    }

    private static Dictionary<string, string> MergeLastWins(IEnumerable<IReadOnlyDictionary<string, string>> sources)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in sources)
        {
            if (source != null)
            {
                foreach (var kvp in source)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
        }

        return result;
    }

    private static Dictionary<string, string> MergeFirstWins(IEnumerable<IReadOnlyDictionary<string, string>> sources)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in sources)
        {
            if (source != null)
            {
                foreach (var kvp in source)
                {
                    if (!result.ContainsKey(kvp.Key))
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        return result;
    }

    private static Dictionary<string, string> MergeStrict(IEnumerable<IReadOnlyDictionary<string, string>> sources)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var sourcesList = sources.ToList();

        foreach (var source in sourcesList)
        {
            if (source != null)
            {
                foreach (var kvp in source)
                {
                    if (result.TryGetValue(kvp.Key, out var existing))
                    {
                        // In Merge strategy, duplicates are OK if values are identical
                        if (existing != kvp.Value)
                        {
                            throw new InvalidOperationException(
                                $"Configuration key '{kvp.Key}' exists in multiple sources with different values: " +
                                $"'{existing}' vs '{kvp.Value}'");
                        }
                    }
                    else
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        return result;
    }

    private static Dictionary<string, string> MergeThrow(IEnumerable<IReadOnlyDictionary<string, string>> sources)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var sourcesList = sources.ToList();

        foreach (var source in sourcesList)
        {
            if (source != null)
            {
                foreach (var kvp in source)
                {
                    if (result.ContainsKey(kvp.Key))
                    {
                        throw new InvalidOperationException(
                            $"Configuration key '{kvp.Key}' exists in multiple sources. Use a different merge strategy.");
                    }

                    result[kvp.Key] = kvp.Value;
                }
            }
        }

        return result;
    }
}
