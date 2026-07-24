// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Thrown when there's an issue with application or service configuration.
/// This includes missing configuration values, invalid configuration combinations,
/// or configuration that violates system constraints.
/// </summary>
public class ConfigurationException : FFmpegException
{
    /// <summary>
    /// Gets the configuration key that caused this exception.
    /// </summary>
    public string? ConfigurationKey { get; set; }

    public ConfigurationException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public ConfigurationException(string message, string configurationKey)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ConfigurationKey = configurationKey;
        Context[nameof(ConfigurationKey)] = configurationKey ?? string.Empty;
    }

    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public ConfigurationException(string message, string configurationKey, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ConfigurationKey = configurationKey;
        Context[nameof(ConfigurationKey)] = configurationKey ?? string.Empty;
    }
}
