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

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ValidationException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and validation errors.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="validationErrors">The validation errors dictionary.</param>
    public ValidationException(string message, Dictionary<string, string[]> validationErrors)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(validationErrors);
        ValidationErrors = validationErrors;
        Context[nameof(ValidationErrors)] = $"Count: {validationErrors.Count}";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(innerException);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message, validation errors, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="validationErrors">The validation errors dictionary.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public ValidationException(string message, Dictionary<string, string[]> validationErrors, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(validationErrors);
        ArgumentNullException.ThrowIfNull(innerException);
        ValidationErrors = validationErrors;
        Context[nameof(ValidationErrors)] = $"Count: {validationErrors.Count}";
    }

    /// <summary>
    /// Creates a validation exception with formatted error messages.
    /// </summary>
    /// <param name="errors">The validation errors dictionary.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <returns>A <see cref="ValidationException"/> instance containing the validation errors.</returns>
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

    /// <summary>
    /// Returns a string representation of the validation exception, including the validation errors if present.
    /// </summary>
    /// <returns>A string representation of the validation exception.</returns>
    public override string ToString()
    {
        var baseString = base.ToString();
        if (ValidationErrors == null || !ValidationErrors.Any())
        {
            return baseString;
        }

        var errors = string.Join(" ", ValidationErrors.Select(kv => $"{kv.Key}: {string.Join("; ", kv.Value)}"));
        return $"{baseString} ValidationErrors: {errors}";
    }
}
