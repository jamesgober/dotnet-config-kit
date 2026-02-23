namespace dotnet_config_kit.Extensions;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Builder for fluently registering configuration sources and binding options.
/// </summary>
public interface IConfigurationSourceBuilder
{
    /// <summary>
    /// Adds a JSON file source.
    /// </summary>
    /// <param name="filePath">The path to the JSON file. Must not be null or empty.</param>
    /// <returns>Self for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
    IConfigurationSourceBuilder AddJsonFile(string filePath);

    /// <summary>
    /// Adds a YAML file source with optional hot-reload.
    /// </summary>
    /// <param name="filePath">The path to the YAML file.</param>
    /// <param name="enableHotReload">Whether to watch for file changes and reload.</param>
    /// <returns>Self for method chaining.</returns>
    IConfigurationSourceBuilder AddYamlFile(string filePath, bool enableHotReload = false);

    /// <summary>
    /// Adds a TOML file source with optional hot-reload.
    /// </summary>
    /// <param name="filePath">The path to the TOML file.</param>
    /// <param name="enableHotReload">Whether to watch for file changes and reload.</param>
    /// <returns>Self for method chaining.</returns>
    IConfigurationSourceBuilder AddTomlFile(string filePath, bool enableHotReload = false);

    /// <summary>
    /// Adds an INI file source with optional hot-reload.
    /// </summary>
    /// <param name="filePath">The path to the INI file.</param>
    /// <param name="enableHotReload">Whether to watch for file changes and reload.</param>
    /// <returns>Self for method chaining.</returns>
    IConfigurationSourceBuilder AddIniFile(string filePath, bool enableHotReload = false);

    /// <summary>
    /// Adds an XML file source with optional hot-reload.
    /// </summary>
    /// <param name="filePath">The path to the XML file.</param>
    /// <param name="enableHotReload">Whether to watch for file changes and reload.</param>
    /// <returns>Self for method chaining.</returns>
    IConfigurationSourceBuilder AddXmlFile(string filePath, bool enableHotReload = false);

    /// <summary>
    /// Adds a command-line arguments source.
    /// </summary>
    /// <param name="args">Command-line arguments from Main(). Must not be null.</param>
    /// <returns>Self for method chaining.</returns>
    IConfigurationSourceBuilder AddCommandLineArguments(string[] args);

    /// <summary>
    /// Adds a user secrets source.
    /// </summary>
    /// <typeparam name="T">The assembly type to scan for UserSecretsIdAttribute.</typeparam>
    /// <returns>Self for method chaining.</returns>
    IConfigurationSourceBuilder AddUserSecrets<T>() where T : class;

    /// <summary>
    /// Adds a user secrets source with explicit UserSecretsId.
    /// </summary>
    /// <param name="userSecretsId">The UserSecretsId. Must not be null or empty.</param>
    /// <returns>Self for method chaining.</returns>
    IConfigurationSourceBuilder AddUserSecrets(string userSecretsId);

    /// <summary>
    /// Sets the configuration profile for environment-specific settings.
    /// </summary>
    /// <param name="profile">The profile name (e.g., "development", "production").</param>
    /// <returns>Self for method chaining.</returns>
    IConfigurationSourceBuilder WithProfile(string? profile);

    /// <summary>
    /// Auto-detects the configuration profile from environment variables.
    /// Checks: ASPNETCORE_ENVIRONMENT, ENVIRONMENT, DOTNET_ENVIRONMENT
    /// </summary>
    /// <returns>Self for method chaining.</returns>
    IConfigurationSourceBuilder WithAutoProfile();

    /// <summary>
    /// Adds an environment variable source with optional prefix filtering.
    /// </summary>
    /// <param name="prefix">Optional prefix for filtering environment variables. Variables must start with prefix + underscore to be included.</param>
    /// <returns>Self for method chaining.</returns>
    IConfigurationSourceBuilder AddEnvironmentVariables(string? prefix = null);

    /// <summary>
    /// Adds an in-memory configuration source.
    /// </summary>
    /// <param name="data">The configuration data. Must not be null.</param>
    /// <param name="name">Optional name for this source.</param>
    /// <returns>Self for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    IConfigurationSourceBuilder AddMemory(IEnumerable<KeyValuePair<string, string>> data, string? name = null);

    /// <summary>
    /// Adds a custom configuration source.
    /// </summary>
    /// <param name="source">The source to add. Must not be null.</param>
    /// <returns>Self for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    IConfigurationSourceBuilder AddSource(Abstractions.IConfigSource source);

    /// <summary>
    /// Registers a configuration binder for the specified type and loads all sources.
    /// </summary>
    /// <typeparam name="T">The type to bind configuration to.</typeparam>
    /// <returns>The fully configured service collection.</returns>
    IServiceCollection Build<T>() where T : class, new();

    /// <summary>
    /// Registers a configuration binder for the specified type and asynchronously loads all sources.
    /// </summary>
    /// <typeparam name="T">The type to bind configuration to.</typeparam>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task that returns the fully configured service collection.</returns>
    Task<IServiceCollection> BuildAsync<T>(CancellationToken cancellationToken = default) where T : class, new();
}
