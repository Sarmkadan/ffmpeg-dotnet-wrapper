# FFmpeg .NET Wrapper - Code Style Guidelines

This document outlines the coding conventions and patterns used in the FFmpeg .NET Wrapper project. New code should follow these guidelines to maintain consistency.

## Namespace Declarations

The project uses a mix of file-scoped and block-style namespace declarations:

### File-scoped namespace (preferred for new files)
```csharp
namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Provides static methods for exception throwing with context enrichment.
/// </summary>
public static class Throw
{
    // ...
}
```
*Used in:* Most exception classes (`src/Exceptions/*.cs`), simple utility classes

### Block-style namespace
```csharp
namespace FFmpegDotnetWrapper.Cli
{
    /// <summary>
    /// Formats CLI command output for display.
    /// </summary>
    public class OutputFormatter
    {
        // ...
    }
}
```
*Used in:* More complex classes with multiple nested types or when file-scoped feels inappropriate

**Recommendation:** Use file-scoped namespace for new files unless there's a specific reason to use block-style.

## Guard Clauses & Argument Validation

The project uses modern .NET 6+ argument validation methods combined with custom helper methods:

### Modern .NET validation (preferred)
```csharp
ArgumentNullException.ThrowIfNull(value, paramName);
ArgumentException.ThrowIfNullOrEmpty(value, paramName);
ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
ArgumentOutOfRangeException.ThrowIfNegative(value);
```

### Custom Throw helpers (for consistency with existing code)
```csharp
Throw.IfNull(value, paramName);
Throw.IfNullOrEmpty(value, paramName);
Throw.IfNullOrWhitespace(value, paramName);
```

**Recommendation:** Use modern .NET validation methods for new code. The custom `Throw` helpers are maintained for backward compatibility but modern methods are preferred.

## ToString() Conventions

Exception classes override `ToString()` to provide meaningful debug information:

### Format pattern
```csharp
public override string ToString()
{
    return !string.IsNullOrEmpty(ConfigurationKey)
        ? $"[ConfigurationKey: {ConfigurationKey}] {base.ToString()}"
        : base.ToString();
}
```

### FFmpegException base format
```csharp
public override string ToString()
{
    return $"FFmpegException {{ ExitCode = {ExitCode}, ErrorOutput = {ErrorOutput} }}";
}
```

**Recommendation:** Override `ToString()` to include relevant properties in a readable format. Use the pattern `[PropertyName: {value}] {base.ToString()}` when adding contextual information.

## Constants Naming

Constants use PascalCase naming:

```csharp
public static class FFmpegConstants
{
    public const string FfmpegExeName = "ffmpeg";
    public const string FfprobeExeName = "ffprobe";
    public const int DefaultBufferSize = 8192;
    public const double DefaultProgressUpdateInterval = 0.5;
}
```

**Recommendation:** Use PascalCase for constants. Group related constants in static classes under the `Constants` namespace.

## XML Documentation

All public types and members should include XML documentation:

```csharp
/// <summary>
/// Provides extension methods for FFmpeg-related exceptions to enhance error handling and diagnostics.
/// </summary>
/// <param name="exception">The FFmpeg exception to format.</param>
/// <returns>A formatted error message string.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
public static string ToDetailedErrorMessage(this FFmpegException exception)
{
    // ...
}
```

**Recommendation:** Include `<summary>`, `<param>`, `<returns>`, and `<exception>` tags for all public members. Use `<see cref="">` for cross-references.

## Extension Method Pattern

Companion files follow a consistent pattern:

### Main Class
`src/Models/FFmpegOperation.cs` - Core class definition

### Extension Methods
`src/Models/FFmpegOperationJsonExtensions.cs` - JSON serialization/deserialization
`src/Models/FFmpegOperationValidation.cs` - Validation logic
`src/Models/FFmpegOperationExtensions.cs` - Additional functionality

**Recommendation:** When creating new classes that need extension points, follow this three-file pattern:
- `{ClassName}.cs` - Core implementation
- `{ClassName}Extensions.cs` - Extension methods
- `{ClassName}JsonExtensions.cs` - JSON serialization (if needed)
- `{ClassName}Validation.cs` - Validation logic (if needed)

## Exception Enrichment Pattern

The `Throw` class provides context enrichment methods:

```csharp
public static T WithCliContext<T>(
    T exception,
    string? cliCommand,
    int? exitCode = null,
    string? errorOutput = null) where T : FFmpegException
{
    ArgumentNullException.ThrowIfNull(exception);

    if (cliCommand is not null)
    {
        exception.Data[nameof(cliCommand)] = cliCommand;
    }

    // ... additional context enrichment

    return exception;
}
```

**Recommendation:** Use the `Throw.With*Context()` methods to enrich exceptions with contextual information rather than manually populating the `Data` dictionary.

## File Organization

- **Exceptions:** `src/Exceptions/` - Custom exception types and related extensions
- **Abstractions:** `src/Abstractions/` - Interfaces and core data transfer objects
- **Api:** `src/Api/` - API controllers and DTOs
- **BackgroundJobs:** `src/BackgroundJobs/` - Background job processing
- **Caching:** `src/Caching/` - Caching services
- **Cli:** `src/Cli/` - Command-line interface components
- **Configuration:** `src/Configuration/` - Configuration models and services
- **Events:** `src/Events/` - Event publishing
- **Integration:** `src/Integration/` - External service integrations
- **Middleware:** `src/Middleware/` - ASP.NET Core middleware
- **Models:** `src/Models/` - Core domain models
- **Monitoring:** `src/Monitoring/` - Monitoring and metrics
- **Policies:** `src/Policies/` - Retry and resilience policies
- **Program.cs:** `src/Program.cs` - Application entry point
- **Repository:** `src/Repository/` - Data access repositories
- **Serialization:** `src/Serialization/` - Custom serialization components
- **Services:** `src/Services/` - Business logic services
- **Utilities:** `src/Utilities/` - Utility classes and helpers

**Recommendation:** Place new files in the appropriate folder based on their responsibility. Follow existing naming conventions.