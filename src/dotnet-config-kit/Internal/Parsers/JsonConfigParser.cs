namespace dotnet_config_kit.Internal.Parsers;

using dotnet_config_kit.Abstractions;
using System.Text.Json;

/// <summary>
/// Parses JSON configuration format into flat key-value pairs.
/// </summary>
public sealed class JsonConfigParser : IConfigParser
{
    /// <inheritdoc />
    public string Format => "json";

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
            var doc = JsonDocument.Parse(content);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            FlattenElement(doc.RootElement, "", result);
            return result;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format: {ex.Message}", nameof(content), ex);
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyDictionary<string, string>> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        try
        {
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            FlattenElement(doc.RootElement, "", result);
            return result;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format: {ex.Message}", nameof(stream), ex);
        }
    }

    private static void FlattenElement(JsonElement element, string prefix, Dictionary<string, string> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    FlattenElement(property.Value, key, result);
                }
                break;

            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var key = $"{prefix}:{index}";
                    FlattenElement(item, key, result);
                    index++;
                }
                break;

            case JsonValueKind.String:
                result[prefix] = element.GetString() ?? "";
                break;

            case JsonValueKind.Number:
                result[prefix] = element.GetRawText();
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                result[prefix] = element.GetBoolean().ToString(System.Globalization.CultureInfo.InvariantCulture);
                break;

            case JsonValueKind.Null:
                result[prefix] = "";
                break;
        }
    }
}
