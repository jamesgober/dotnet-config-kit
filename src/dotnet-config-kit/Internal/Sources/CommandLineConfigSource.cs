namespace dotnet_config_kit.Internal.Sources;

using dotnet_config_kit.Abstractions;
using System.Text.RegularExpressions;

/// <summary>
/// Loads configuration from command-line arguments.
/// Supports formats: --key=value, --key value, -k value
/// </summary>
public sealed class CommandLineConfigSource : IConfigSource
{
    private readonly string[] _args;

    /// <inheritdoc />
    public string Name => "Command-Line Arguments";

    /// <summary>
    /// Creates a new command-line configuration source.
    /// </summary>
    /// <param name="args">Command-line arguments from Main(). Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when args is null.</exception>
    public CommandLineConfigSource(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        _args = args;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Load()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        for (int i = 0; i < _args.Length; i++)
        {
            var arg = _args[i];

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                // Format: --key=value or --key value
                var keyValue = arg.Substring(2);
                var eqIndex = keyValue.IndexOf('=');

                if (eqIndex > 0)
                {
                    // --key=value
                    var key = keyValue.Substring(0, eqIndex);
                    var value = keyValue.Substring(eqIndex + 1);
                    result[key] = value;
                }
                else if (eqIndex < 0)
                {
                    // --key value (look ahead for value)
                    if (i + 1 < _args.Length && !_args[i + 1].StartsWith('-'))
                    {
                        result[keyValue] = _args[i + 1];
                        i++; // Skip next arg since we consumed it
                    }
                    else
                    {
                        // --key with no value
                        result[keyValue] = "true";
                    }
                }
            }
            else if (arg.Length == 2 && arg[0] == '-' && arg[1] != '-')
            {
                // Format: -k value
                var key = arg.Substring(1);
                
                if (i + 1 < _args.Length && !_args[i + 1].StartsWith('-'))
                {
                    result[key] = _args[i + 1];
                    i++; // Skip next arg
                }
                else
                {
                    result[key] = "true";
                }
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
