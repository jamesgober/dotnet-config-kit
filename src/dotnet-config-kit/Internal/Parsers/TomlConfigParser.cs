namespace dotnet_config_kit.Internal.Parsers;

using dotnet_config_kit.Abstractions;
using Tomlyn;
using Tomlyn.Model;

/// <summary>
/// Parses TOML configuration format into flat key-value pairs.
/// </summary>
public sealed class TomlConfigParser : IConfigParser
{
    /// <inheritdoc />
    public string Format => "toml";

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));

        if (string.IsNullOrWhiteSpace(content))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var model = Toml.ToModel(content);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            FlattenTable(model, "", result);
            return result;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid TOML format: {ex.Message}", nameof(content), ex);
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyDictionary<string, string>> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        try
        {
            using var reader = new System.IO.StreamReader(stream);
            var content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            return Parse(content);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid TOML format: {ex.Message}", nameof(stream), ex);
        }
    }

    private static void FlattenTable(TomlTable table, string prefix, Dictionary<string, string> result)
    {
        foreach (var kvp in table)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
            var value = kvp.Value;

            if (value is TomlTable nestedTable)
            {
                FlattenTable(nestedTable, key, result);
            }
            else if (value is System.Collections.IList list)
            {
                FlattenArray(list, key, result);
            }
            else
            {
                result[key] = value?.ToString() ?? "";
            }
        }
    }

    private static void FlattenArray(System.Collections.IList list, string prefix, Dictionary<string, string> result)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var key = $"{prefix}:{i}";
            var item = list[i];

            if (item is TomlTable table)
            {
                FlattenTable(table, key, result);
            }
            else if (item is System.Collections.IList nestedList)
            {
                FlattenArray(nestedList, key, result);
            }
            else
            {
                result[key] = item?.ToString() ?? "";
            }
        }
    }
}
