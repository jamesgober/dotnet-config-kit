namespace dotnet_config_kit.Internal.Binding;

using dotnet_config_kit.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

/// <summary>
/// Enhanced reflection-based configuration binder with custom converters and validation attributes.
/// </summary>
/// <typeparam name="T">The target type to bind configuration to.</typeparam>
public sealed class EnhancedReflectionConfigBinder<T> : IConfigBinder<T> where T : class, new()
{
    private readonly Dictionary<Type, object> _customConverters = new();

    /// <summary>
    /// Registers a custom type converter.
    /// </summary>
    /// <typeparam name="TValue">The type to convert to.</typeparam>
    /// <param name="converter">The converter implementation.</param>
    public void RegisterConverter<TValue>(IConfigValueConverter<TValue> converter)
    {
        ArgumentNullException.ThrowIfNull(converter, nameof(converter));
        _customConverters[typeof(TValue)] = converter;
    }

    /// <inheritdoc />
    public T Bind(IReadOnlyDictionary<string, string> configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var instance = new T();
        var errors = new List<ConfigurationError>();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (!property.CanWrite)
            {
                continue;
            }

            var key = property.Name.ToLowerInvariant();
            var foundValue = false;
            var value = "";
            
            foreach (var kvp in configuration)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = kvp.Value;
                    foundValue = true;
                    break;
                }
            }

            if (foundValue)
            {
                try
                {
                    var convertedValue = ConvertValue(value, property.PropertyType);
                    property.SetValue(instance, convertedValue);
                }
                catch (Exception ex)
                {
                    errors.Add(new ConfigurationError
                    {
                        Path = key,
                        Message = $"Failed to bind to type {property.PropertyType.Name}: {ex.Message}",
                        AttemptedValue = value
                    });
                }
            }
        }

        if (errors.Count > 0)
        {
            var errorMessages = string.Join(Environment.NewLine, errors.Select(e => $"  {e.Path}: {e.Message}"));
            throw new InvalidOperationException($"Configuration binding failed with {errors.Count} error(s):{Environment.NewLine}{errorMessages}");
        }

        // Validate with DataAnnotations if available
        ValidateWithAttributes(instance, errors);
        
        if (errors.Count > 0)
        {
            var errorMessages = string.Join(Environment.NewLine, errors.Select(e => $"  {e.Path}: {e.Message}"));
            throw new InvalidOperationException($"Configuration validation failed with {errors.Count} error(s):{Environment.NewLine}{errorMessages}");
        }

        return instance;
    }

    /// <inheritdoc />
    public ValueTask<T> BindAsync(IReadOnlyDictionary<string, string> configuration, CancellationToken cancellationToken = default)
    {
        return new ValueTask<T>(Bind(configuration));
    }

    /// <inheritdoc />
    public IReadOnlyList<ConfigurationError> GetValidationErrors(IReadOnlyDictionary<string, string> configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var errors = new List<ConfigurationError>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanWrite)
            {
                continue;
            }

            var key = property.Name.ToLowerInvariant();
            var foundValue = false;
            var value = "";

            foreach (var kvp in configuration)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = kvp.Value;
                    foundValue = true;
                    break;
                }
            }

            if (foundValue)
            {
                try
                {
                    ConvertValue(value, property.PropertyType);
                }
                catch (Exception ex)
                {
                    errors.Add(new ConfigurationError
                    {
                        Path = key,
                        Message = $"Failed to bind to type {property.PropertyType.Name}: {ex.Message}",
                        AttemptedValue = value
                    });
                }
            }
        }

        return errors;
    }

    private object? ConvertValue(string? value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
        {
            if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
            {
                throw new ArgumentException($"Cannot convert empty value to non-nullable {targetType.Name}");
            }
            return null;
        }

        // Check for custom converter first
        if (_customConverters.TryGetValue(targetType, out var converter))
        {
            var converterType = typeof(IConfigValueConverter<>).MakeGenericType(targetType);
            var convertMethod = converterType.GetMethod("Convert");
            return convertMethod?.Invoke(converter, new object?[] { value });
        }

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(string))
            return value;

        if (underlyingType == typeof(bool))
            return bool.Parse(value);

        if (underlyingType == typeof(byte))
            return byte.Parse(value, CultureInfo.InvariantCulture);

        if (underlyingType == typeof(short))
            return short.Parse(value, CultureInfo.InvariantCulture);

        if (underlyingType == typeof(int))
            return int.Parse(value, CultureInfo.InvariantCulture);

        if (underlyingType == typeof(long))
            return long.Parse(value, CultureInfo.InvariantCulture);

        if (underlyingType == typeof(float))
            return float.Parse(value, CultureInfo.InvariantCulture);

        if (underlyingType == typeof(double))
            return double.Parse(value, CultureInfo.InvariantCulture);

        if (underlyingType == typeof(decimal))
            return decimal.Parse(value, CultureInfo.InvariantCulture);

        if (underlyingType == typeof(Guid))
            return Guid.Parse(value);

        if (underlyingType == typeof(DateTime))
            return DateTime.Parse(value, CultureInfo.InvariantCulture);

        if (underlyingType == typeof(TimeSpan))
            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);

        if (underlyingType.IsEnum)
            return Enum.Parse(underlyingType, value, ignoreCase: true);

        throw new ArgumentException($"Unsupported type: {targetType.Name}");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "CA1822:Mark members as static")]
    private void ValidateWithAttributes(T instance, List<ConfigurationError> errors)
    {
        var context = new ValidationContext(instance, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(instance, context, results, validateAllProperties: true))
        {
            foreach (var result in results)
            {
                var property = result.MemberNames.FirstOrDefault() ?? "unknown";
                errors.Add(new ConfigurationError
                {
                    Path = property,
                    Message = result.ErrorMessage ?? "Validation failed",
                    AttemptedValue = null
                });
            }
        }
    }
}
