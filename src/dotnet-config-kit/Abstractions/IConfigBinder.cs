#pragma warning disable CA1707
namespace dotnet_config_kit.Abstractions;
#pragma warning restore CA1707

/// <summary>
/// Validates configuration against a type schema and provides strongly-typed access.
/// </summary>
/// <typeparam name="T">The target type to bind configuration to.</typeparam>
/// <remarks>
/// Implementations perform type conversion, validation, and error reporting.
/// Must handle: null/missing values, type mismatches, collection binding, nested objects, and custom converters.
/// All conversion failures must be reported with clear paths to the problematic configuration.
/// Thread-safe for reading and binding, but configuration should not be mutated during binding.
/// </remarks>
public interface IConfigBinder<T> where T : class, new()
{
    /// <summary>
    /// Binds flat configuration to a strongly-typed object.
    /// </summary>
    /// <param name="configuration">The flat key-value configuration dictionary.</param>
    /// <returns>A new instance of <typeparamref name="T"/> with configuration applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when configuration cannot be bound due to type mismatches, validation failures, or missing required values.
    /// </exception>
    /// <remarks>
    /// Default values (CLR defaults) are used for missing configuration keys.
    /// Required fields must be marked explicitly; primitive types are assumed optional.
    /// Collections are bound by matching key patterns (e.g., "items:0:name", "items:1:name").
    /// </remarks>
    T Bind(IReadOnlyDictionary<string, string> configuration);

    /// <summary>
    /// Asynchronously binds flat configuration to a strongly-typed object.
    /// </summary>
    /// <param name="configuration">The flat key-value configuration dictionary.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that returns a new instance of <typeparamref name="T"/> when complete.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when configuration cannot be bound due to type mismatches, validation failures, or missing required values.
    /// </exception>
    /// <remarks>
    /// Async binding allows for custom converters that may perform I/O (e.g., validating with external services).
    /// For most cases, use the synchronous Bind method.
    /// </remarks>
    ValueTask<T> BindAsync(IReadOnlyDictionary<string, string> configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets validation errors for the configuration, if any.
    /// </summary>
    /// <remarks>
    /// Returns an empty collection if the configuration is valid.
    /// Validation is performed automatically during binding; this method allows checking without binding.
    /// </remarks>
    IReadOnlyList<ConfigurationError> GetValidationErrors(IReadOnlyDictionary<string, string> configuration);
}

/// <summary>
/// Represents a single configuration validation error.
/// </summary>
public sealed class ConfigurationError
{
    /// <summary>
    /// Gets the dot-notation path to the problematic configuration value.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the error message describing the validation failure.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the actual value that failed validation, if available.
    /// </summary>
    /// <remarks>
    /// May be null if the error is due to a missing required value.
    /// </remarks>
    public string? AttemptedValue { get; init; }
}
