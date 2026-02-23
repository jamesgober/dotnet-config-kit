namespace dotnet_config_kit.Internal;

/// <summary>
/// Manages configuration profiles for environment-specific settings.
/// </summary>
public sealed class ConfigurationProfile
{
    private string? _currentProfile;

    /// <summary>
    /// Gets the current active profile.
    /// </summary>
    public string? CurrentProfile => _currentProfile;

    /// <summary>
    /// Sets the active profile.
    /// </summary>
    /// <param name="profile">The profile name (e.g., "development", "production"). Can be null.</param>
    public void SetProfile(string? profile)
    {
        _currentProfile = profile;
    }

    /// <summary>
    /// Gets the profile-specific filename for a base filename.
    /// </summary>
    /// <param name="baseFileName">The base filename (e.g., "appsettings.json").</param>
    /// <returns>
    /// If profile is set: "appsettings.{profile}.json"
    /// If profile is null: original filename
    /// </returns>
    public string GetProfileFileName(string baseFileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseFileName, nameof(baseFileName));

        if (string.IsNullOrEmpty(_currentProfile))
        {
            return baseFileName;
        }

        var extension = Path.GetExtension(baseFileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);
        var directory = Path.GetDirectoryName(baseFileName) ?? "";

        var profileFileName = $"{nameWithoutExtension}.{_currentProfile}{extension}";
        return string.IsNullOrEmpty(directory) ? profileFileName : Path.Combine(directory, profileFileName);
    }

    /// <summary>
    /// Auto-detect profile from environment variable.
    /// Common variable names: ASPNETCORE_ENVIRONMENT, ENVIRONMENT, DOTNET_ENVIRONMENT
    /// </summary>
    public void AutoDetectProfile()
    {
        var profile = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                      Environment.GetEnvironmentVariable("ENVIRONMENT") ??
                      Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        if (!string.IsNullOrEmpty(profile))
        {
            SetProfile(profile);
        }
    }
}
