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
