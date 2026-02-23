namespace dotnet_config_kit.Internal.Sources;

using dotnet_config_kit.Abstractions;
using dotnet_config_kit.Internal.Parsers;
using System.Reflection;

/// <summary>
/// Loads configuration from .NET user secrets.
/// Stores in: ~/.microsoft/usersecrets/{UserSecretsId}/secrets.json
/// </summary>
public sealed class UserSecretsConfigSource : IConfigSource
{
    private const string UserSecretsIdAttributeTypeName = "Microsoft.Extensions.Configuration.UserSecrets.UserSecretsIdAttribute";
    private readonly string _userSecretsId;
    private readonly JsonConfigParser _parser = new();

    /// <inheritdoc />
    public string Name => "User Secrets";

    /// <summary>
    /// Creates a new user secrets source using the app's UserSecretsId.
    /// </summary>
    /// <typeparam name="T">The assembly type to scan for UserSecretsIdAttribute.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown when UserSecretsIdAttribute is not found.</exception>
    public static UserSecretsConfigSource FromAssembly<T>() where T : class
    {
        var attrType = Type.GetType(UserSecretsIdAttributeTypeName);
        if (attrType == null)
        {
            throw new InvalidOperationException(
                "UserSecretsIdAttribute not found. Ensure you have installed Microsoft.Extensions.Configuration.UserSecrets package.");
        }

        var attr = typeof(T).Assembly.GetCustomAttributes().FirstOrDefault(a => a.GetType().FullName == UserSecretsIdAttributeTypeName);
        if (attr == null)
        {
            throw new InvalidOperationException(
                $"Assembly {typeof(T).Assembly.GetName().Name} does not have UserSecretsIdAttribute. " +
                $"Add it to AssemblyInfo.cs or use the constructor with explicit UserSecretsId.");
        }

        var idProperty = attrType.GetProperty("UserSecretsId");
        var userSecretsId = idProperty?.GetValue(attr) as string;
        
        if (string.IsNullOrEmpty(userSecretsId))
        {
            throw new InvalidOperationException("UserSecretsId is empty or null");
        }

        return new UserSecretsConfigSource(userSecretsId);
    }

    /// <summary>
    /// Creates a new user secrets source with an explicit UserSecretsId.
    /// </summary>
    /// <param name="userSecretsId">The UserSecretsId. Must not be null or empty.</param>
    /// <exception cref="ArgumentException">Thrown when userSecretsId is null or empty.</exception>
    public UserSecretsConfigSource(string userSecretsId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userSecretsId, nameof(userSecretsId));
        _userSecretsId = userSecretsId;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Load()
    {
        var secretsPath = GetSecretsPath();

        if (!File.Exists(secretsPath))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var json = File.ReadAllText(secretsPath);
            return _parser.Parse(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load user secrets from {secretsPath}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var secretsPath = GetSecretsPath();

        if (!File.Exists(secretsPath))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var json = await File.ReadAllTextAsync(secretsPath, cancellationToken).ConfigureAwait(false);
            return _parser.Parse(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load user secrets from {secretsPath}: {ex.Message}", ex);
        }
    }

    private string GetSecretsPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataPath, "microsoft", "usersecrets", _userSecretsId, "secrets.json");
    }
}
