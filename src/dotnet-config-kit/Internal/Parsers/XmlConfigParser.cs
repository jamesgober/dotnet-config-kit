namespace dotnet_config_kit.Internal.Parsers;

using dotnet_config_kit.Abstractions;
using System.Xml.Linq;

/// <summary>
/// Parses XML configuration format into flat key-value pairs.
/// </summary>
public sealed class XmlConfigParser : IConfigParser
{
    /// <inheritdoc />
    public string Format => "xml";

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
            var doc = XDocument.Parse(content);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            if (doc.Root != null)
            {
                FlattenElement(doc.Root, "", result);
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid XML format: {ex.Message}", nameof(content), ex);
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyDictionary<string, string>> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        try
        {
            var doc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            if (doc.Root != null)
            {
                FlattenElement(doc.Root, "", result);
            }

            return result;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid XML format: {ex.Message}", nameof(stream), ex);
        }
    }

    private static void FlattenElement(XElement element, string prefix, Dictionary<string, string> result)
    {
        var elementKey = string.IsNullOrEmpty(prefix) ? element.Name.LocalName : $"{prefix}.{element.Name.LocalName}";

        // Add attributes as keys
        foreach (var attr in element.Attributes())
        {
            var attrKey = $"{elementKey}:@{attr.Name.LocalName}";
            result[attrKey] = attr.Value;
        }

        // Process child elements
        var childElements = element.Elements().ToList();
        
        if (childElements.Count == 0)
        {
            // Leaf node: use element text if present
            var text = element.Value;
            if (!string.IsNullOrWhiteSpace(text))
            {
                result[elementKey] = text;
            }
        }
        else
        {
            // Group children by name to handle arrays
            var groups = childElements.GroupBy(e => e.Name.LocalName).ToList();

            foreach (var group in groups)
            {
                if (group.Count() == 1)
                {
                    // Single child: recurse
                    FlattenElement(group.First(), elementKey, result);
                }
                else
                {
                    // Multiple children with same name: treat as array
                    var index = 0;
                    foreach (var child in group)
                    {
                        var arrayKey = $"{elementKey}:{index}";
                        FlattenElement(child, arrayKey, result);
                        index++;
                    }
                }
            }
        }
    }
}
