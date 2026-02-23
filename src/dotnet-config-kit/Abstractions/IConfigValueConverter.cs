namespace dotnet_config_kit.Abstractions;

/// <summary>
/// Contract for converting configuration string values to strongly-typed values.
/// </summary>
/// <typeparam name="T">The target type to convert to.</typeparam>
public interface IConfigValueConverter<T>
{
    /// <summary>
    /// Converts a string value to the target type.
    /// </summary>
    /// <param name="value">The string value to convert. May be null or empty.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="ArgumentException">Thrown when the value cannot be converted.</exception>
    T Convert(string? value);

    /// <summary>
    /// Gets the type this converter handles.
    /// </summary>
    Type TargetType => typeof(T);
}
