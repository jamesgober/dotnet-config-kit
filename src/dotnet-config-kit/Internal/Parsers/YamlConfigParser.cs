namespace dotnet_config_kit.Internal.Parsers;

using dotnet_config_kit.Abstractions;
using YamlDotNet.RepresentationModel;

/// <summary>
/// Parses YAML configuration format into flat key-value pairs.
/// </summary>
public sealed class YamlConfigParser : IConfigParser
{
    /// <inheritdoc />
    public string Format => "yaml";

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
            using var reader = new System.IO.StringReader(content);
            var yaml = new YamlStream();
            yaml.Load(reader);

            if (yaml.Documents.Count == 0)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var root = yaml.Documents[0].RootNode;

            if (root is YamlMappingNode mapping)
            {
                FlattenMapping(mapping, "", result);
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid YAML format: {ex.Message}", nameof(content), ex);
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
            throw new ArgumentException($"Invalid YAML format: {ex.Message}", nameof(stream), ex);
        }
    }

    private static void FlattenMapping(YamlMappingNode mapping, string prefix, Dictionary<string, string> result)
    {
        foreach (var entry in mapping.Children)
        {
            var keyNode = entry.Key;
            var valueNode = entry.Value;

            if (keyNode is not YamlScalarNode keyScalar)
            {
                continue;
            }

            var keyValue = keyScalar.Value;
            if (keyValue == null)
            {
                continue;
            }

            var key = string.IsNullOrEmpty(prefix) ? keyValue : $"{prefix}.{keyValue}";

            switch (valueNode)
            {
                case YamlMappingNode nestedMapping:
                    FlattenMapping(nestedMapping, key, result);
                    break;

                case YamlSequenceNode sequence:
                    FlattenSequence(sequence, key, result);
                    break;

                case YamlScalarNode scalar:
                    result[key] = scalar.Value ?? "";
                    break;
            }
        }
    }

    private static void FlattenSequence(YamlSequenceNode sequence, string prefix, Dictionary<string, string> result)
    {
        var index = 0;
        foreach (var item in sequence.Children)
        {
            var key = $"{prefix}:{index}";

            switch (item)
            {
                case YamlMappingNode mapping:
                    FlattenMapping(mapping, key, result);
                    break;

                case YamlSequenceNode nested:
                    FlattenSequence(nested, key, result);
                    break;

                case YamlScalarNode scalar:
                    result[key] = scalar.Value ?? "";
                    break;
            }

            index++;
        }
    }
}
