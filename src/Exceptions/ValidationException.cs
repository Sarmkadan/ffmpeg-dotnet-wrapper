// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Thrown when input validation fails for API requests, settings, or parameters.
/// Contains detailed information about which validation rules were violated.
/// </summary>
public class ValidationException : FFmpegException
{
    /// <summary>
    /// Gets the validation errors dictionary.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    public ValidationException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public ValidationException(string message, Dictionary<string, string[]> validationErrors)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ValidationErrors = validationErrors;
        Context[nameof(ValidationErrors)] = $"Count: {validationErrors?.Count ?? 0}";
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public ValidationException(string message, Dictionary<string, string[]> validationErrors, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ValidationErrors = validationErrors;
        Context[nameof(ValidationErrors)] = $"Count: {validationErrors?.Count ?? 0}";
    }

    /// <summary>
    /// Creates a validation exception with formatted error messages.
    /// </summary>
    public static ValidationException FromDictionary(Dictionary<string, string[]> errors, string message = "Validation failed")
    {
        ArgumentNullException.ThrowIfNull(errors);
        ArgumentException.ThrowIfNullOrEmpty(message);

        var formattedErrors = new Dictionary<string, string[]>();
        foreach (var error in errors)
        {
            formattedErrors[error.Key] = error.Value;
        }

        return new ValidationException(message, formattedErrors);
    }
}
