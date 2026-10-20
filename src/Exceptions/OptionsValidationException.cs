// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Thrown when an options object fails validation. Carries every violated rule in
/// <see cref="Violations"/> so callers can fix all problems in one pass.
/// </summary>
public class OptionsValidationException : ValidationException
{
    /// <summary>
    /// Gets the human-readable description of each violated rule, in the order the rules were checked.
    /// </summary>
    public IReadOnlyList<string> Violations { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsValidationException"/> class.
    /// </summary>
    /// <param name="violations">The violated rules. Must contain at least one entry.</param>
    public OptionsValidationException(IReadOnlyList<string> violations)
        : base(BuildMessage(violations))
    {
        Violations = violations;
    }

    private static string BuildMessage(IReadOnlyList<string> violations)
    {
        ArgumentNullException.ThrowIfNull(violations);
        if (violations.Count == 0)
            throw new ArgumentException("At least one violation is required.", nameof(violations));

        var lines = violations.Select(v => $"  - {v}");
        return $"Invalid options ({violations.Count} violation(s)):{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
    }
}
