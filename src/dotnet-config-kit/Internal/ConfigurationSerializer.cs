namespace dotnet_config_kit.Internal;

/// <summary>
/// Serializes flat configuration to various formats.
/// </summary>
public sealed class ConfigurationSerializer
{
    /// <summary>
    /// Exports configuration as JSON format.
    /// </summary>
    /// <param name="configuration">The flat configuration dictionary.</param>
    /// <param name="indent">Whether to format with indentation for readability.</param>
    /// <returns>JSON string representation.</returns>
    public static string ExportAsJson(IReadOnlyDictionary<string, string> configuration, bool indent = true)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        // Convert flat keys to nested structure
        var nested = FlattenToNested(configuration);
        
        // Simple JSON serialization
        var json = SerializeToJson(nested, indent ? 0 : -1);
        return json;
    }

    /// <summary>
    /// Exports configuration as YAML format.
    /// </summary>
    /// <param name="configuration">The flat configuration dictionary.</param>
    /// <returns>YAML string representation.</returns>
    public static string ExportAsYaml(IReadOnlyDictionary<string, string> configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var nested = FlattenToNested(configuration);
        return SerializeToYaml(nested, 0);
    }

    private static Dictionary<string, object> FlattenToNested(IReadOnlyDictionary<string, string> flat)
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in flat)
        {
            var parts = kvp.Key.Split('.');
            var current = (IDictionary<string, object>)result;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (!current.TryGetValue(part, out var existingValue))
                {
                    var newDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    current[part] = newDict;
                    current = newDict;
                }
                else if (existingValue is IDictionary<string, object> dict)
                {
                    current = dict;
                }
            }

            var lastPart = parts[^1];
            current[lastPart] = kvp.Value;
        }

        return result;
    }

    private static string SerializeToJson(Dictionary<string, object> obj, int indentLevel)
    {
        var sb = new System.Text.StringBuilder();
        var indent = indentLevel >= 0 ? new string(' ', indentLevel) : "";
        var nextIndent = indentLevel >= 0 ? new string(' ', indentLevel + 2) : "";

        sb.Append('{');
        if (indentLevel >= 0) sb.AppendLine();

        var items = obj.ToList();
        for (int i = 0; i < items.Count; i++)
        {
            var (key, value) = items[i];
            if (indentLevel >= 0) sb.Append(nextIndent);

            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "\"{0}\": ", key);

            if (value is Dictionary<string, object> nested)
            {
                sb.Append(SerializeToJson(nested, indentLevel >= 0 ? indentLevel + 2 : -1));
            }
            else
            {
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "\"{0}\"", value);
            }

            if (i < items.Count - 1)
                sb.Append(',');

            if (indentLevel >= 0) sb.AppendLine();
        }

        if (indentLevel >= 0) sb.Append(indent);
        sb.Append('}');

        return sb.ToString();
    }

    private static string SerializeToYaml(Dictionary<string, object> obj, int indentLevel)
    {
        var sb = new System.Text.StringBuilder();
        var indent = new string(' ', indentLevel);

        foreach (var (key, value) in obj)
        {
            sb.Append(indent);
            sb.Append(key);
            sb.Append(':');sb.Append(' ');

            if (value is Dictionary<string, object> nested)
            {
                sb.AppendLine();
                sb.Append(SerializeToYaml(nested, indentLevel + 2));
            }
            else
            {
                sb.AppendLine(value.ToString());
            }
        }

        return sb.ToString();
    }
}
