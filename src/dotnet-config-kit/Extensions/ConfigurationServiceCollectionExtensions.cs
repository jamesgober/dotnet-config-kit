namespace dotnet_config_kit.Extensions;

using dotnet_config_kit.Abstractions;
using dotnet_config_kit.Internal;
using dotnet_config_kit.Internal.Binding;
using dotnet_config_kit.Internal.Parsers;
using dotnet_config_kit.Internal.Sources;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering configuration services with dependency injection.
/// </summary>
public static class ConfigurationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the configuration library services.
    /// </summary>
    /// <param name="services">The service collection to register with. Must not be null.</param>
    /// <returns>A builder for fluently configuring configuration sources.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IConfigurationSourceBuilder AddConfiguration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        var builder = new ConfigurationSourceBuilder(services);
        return builder;
    }
}

/// <summary>
/// Implementation of fluent builder for registering configuration sources.
/// </summary>
internal sealed class ConfigurationSourceBuilder : IConfigurationSourceBuilder
{
    private readonly IServiceCollection _services;
    private readonly ConfigBuilder _configBuilder = new();

    internal ConfigurationSourceBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddJsonFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));
        _configBuilder.AddSource(new FileConfigSource(filePath, new JsonConfigParser()));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddEnvironmentVariables(string? prefix = null)
    {
        _configBuilder.AddSource(new EnvironmentVariableConfigSource(prefix));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddMemory(IEnumerable<KeyValuePair<string, string>> data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));
        _configBuilder.AddSource(new MemoryConfigSource(data, name));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationSourceBuilder AddSource(IConfigSource source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        _configBuilder.AddSource(source);
        return this;
    }

    /// <inheritdoc />
    public IServiceCollection Build<T>() where T : class, new()
    {
        _configBuilder.Load();
        var configuration = _configBuilder.Configuration;

        var binder = new ReflectionConfigBinder<T>();
        var bindingErrors = binder.GetValidationErrors(configuration);
        if (bindingErrors.Count > 0)
        {
            var errorMessages = string.Join(Environment.NewLine, bindingErrors.Select(e => $"  {e.Path}: {e.Message}"));
            throw new InvalidOperationException($"Configuration validation failed with {bindingErrors.Count} error(s):{Environment.NewLine}{errorMessages}");
        }

        var options = binder.Bind(configuration);
        _services.AddSingleton(options);
        _services.AddSingleton<IConfigBinder<T>>(binder);

        return _services;
    }

    /// <inheritdoc />
    public async Task<IServiceCollection> BuildAsync<T>(CancellationToken cancellationToken = default) where T : class, new()
    {
        await _configBuilder.LoadAsync(cancellationToken).ConfigureAwait(false);
        var configuration = _configBuilder.Configuration;

        var binder = new ReflectionConfigBinder<T>();
        var bindingErrors = binder.GetValidationErrors(configuration);
        if (bindingErrors.Count > 0)
        {
            var errorMessages = string.Join(Environment.NewLine, bindingErrors.Select(e => $"  {e.Path}: {e.Message}"));
            throw new InvalidOperationException($"Configuration validation failed with {bindingErrors.Count} error(s):{Environment.NewLine}{errorMessages}");
        }

        var options = binder.Bind(configuration);
        _services.AddSingleton(options);
        _services.AddSingleton<IConfigBinder<T>>(binder);

        return _services;
    }
}
