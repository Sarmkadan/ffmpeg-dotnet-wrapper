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

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ConfigurationException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message and the configuration key that caused the exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="configurationKey">The configuration key that caused this exception.</param>
    public ConfigurationException(string message, string configurationKey)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ConfigurationKey = configurationKey;
        Context[nameof(ConfigurationKey)] = configurationKey ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message, the configuration key that caused the exception, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="configurationKey">The configuration key that caused this exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public ConfigurationException(string message, string configurationKey, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ConfigurationKey = configurationKey;
        Context[nameof(ConfigurationKey)] = configurationKey ?? string.Empty;
    }

    public override string ToString()
    {
        return !string.IsNullOrEmpty(ConfigurationKey)
            ? $"[ConfigurationKey: {ConfigurationKey}] {base.ToString()}"
            : base.ToString();
    }
}