namespace dotnet_config_kit.Internal.Parsers;

using dotnet_config_kit.Abstractions;
using System.Text.RegularExpressions;

/// <summary>
/// Parses INI configuration format into flat key-value pairs.
/// </summary>
public sealed class IniConfigParser : IConfigParser
{
    private static readonly string[] Separators = { "\r\n", "\r", "\n" };

    /// <inheritdoc />
    public string Format => "ini";

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));

        if (string.IsNullOrWhiteSpace(content))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var currentSection = "";
        var lines = content.Split(Separators, StringSplitOptions.None);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
            {
                continue;
            }

            // Parse section header [section]
            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
                continue;
            }

            // Parse key=value
            var eqIndex = trimmed.IndexOf('=');
            if (eqIndex > 0)
            {
                var key = trimmed.Substring(0, eqIndex).Trim();
                var value = trimmed.Substring(eqIndex + 1).Trim();

                // Remove quotes if present
                if (value.StartsWith('"') && value.EndsWith('"'))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                else if (value.StartsWith('\'') && value.EndsWith('\''))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                var configKey = string.IsNullOrEmpty(currentSection) ? key : $"{currentSection}.{key}";
                result[configKey] = value;
            }
        }

        return result;
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
            throw new ArgumentException($"Invalid INI format: {ex.Message}", nameof(stream), ex);
        }
    }
}
