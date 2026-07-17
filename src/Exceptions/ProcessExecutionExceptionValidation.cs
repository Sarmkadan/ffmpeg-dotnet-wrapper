// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="ProcessExecutionException"/> instances.
/// </summary>
public static class ProcessExecutionExceptionValidation
{
    /// <summary>
    /// Validates a <see cref="ProcessExecutionException"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A read-only list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ProcessExecutionException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate base Exception properties
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message cannot be null, empty, or whitespace.");
        }

        // Validate ExitCode (non-negative when set)
        if (value.ExitCode.HasValue && value.ExitCode.Value < 0)
        {
            problems.Add("ExitCode must be a non-negative integer when set.");
        }

        // Validate ErrorOutput when ExitCode is set
        if (value.ExitCode.HasValue && string.IsNullOrWhiteSpace(value.ErrorOutput))
        {
            problems.Add("ErrorOutput must be provided when ExitCode is set.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ProcessExecutionException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if the exception is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ProcessExecutionException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ProcessExecutionException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the exception is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ProcessExecutionException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ProcessExecutionException is invalid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }
}