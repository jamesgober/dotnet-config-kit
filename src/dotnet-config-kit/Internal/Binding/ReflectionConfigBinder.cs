namespace dotnet_config_kit.Internal.Binding;

using dotnet_config_kit.Abstractions;
using System.Globalization;
using System.Reflection;

/// <summary>
/// Binds flat configuration to strongly-typed objects using reflection.
/// </summary>
/// <typeparam name="T">The target type to bind configuration to.</typeparam>
public sealed class ReflectionConfigBinder<T> : IConfigBinder<T> where T : class, new()
{
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

    private static object? ConvertValue(string value, Type targetType)
    {
        if (targetType == typeof(string))
        {
            return value;
        }

        if (string.IsNullOrEmpty(value))
        {
            if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
            {
                return Activator.CreateInstance(targetType);
            }
            return null;
        }

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(bool))
        {
            if (bool.TryParse(value, out var result))
            {
                return result;
            }
            throw new InvalidOperationException($"Cannot convert '{value}' to boolean");
        }

        if (underlyingType == typeof(int))
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            throw new InvalidOperationException($"Cannot convert '{value}' to int");
        }

        if (underlyingType == typeof(long))
        {
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            throw new InvalidOperationException($"Cannot convert '{value}' to long");
        }

        if (underlyingType == typeof(double))
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            throw new InvalidOperationException($"Cannot convert '{value}' to double");
        }

        if (underlyingType == typeof(float))
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            throw new InvalidOperationException($"Cannot convert '{value}' to float");
        }

        if (underlyingType == typeof(decimal))
        {
            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            throw new InvalidOperationException($"Cannot convert '{value}' to decimal");
        }

        if (underlyingType == typeof(Guid))
        {
            if (Guid.TryParse(value, out var result))
            {
                return result;
            }
            throw new InvalidOperationException($"Cannot convert '{value}' to Guid");
        }

        if (underlyingType == typeof(DateTime))
        {
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
            {
                return result;
            }
            throw new InvalidOperationException($"Cannot convert '{value}' to DateTime");
        }

        if (underlyingType.IsEnum)
        {
            try
            {
                return Enum.Parse(underlyingType, value, ignoreCase: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot convert '{value}' to {underlyingType.Name}", ex);
            }
        }

        throw new InvalidOperationException($"Type {targetType.Name} is not supported for configuration binding");
    }
}
