# CliOutputFormatterValidation

Provides static validation rule collections for `CliOutputFormatter` configuration properties. Each member exposes a read-only list of validation error messages that correspond to specific constraint violations, enabling declarative validation of formatter settings before application.

## API

### ValidateConsoleWidth
```csharp
public static IReadOnlyList<string> ValidateConsoleWidth { get; }
```
Returns the set of validation messages for console width constraints. Contains messages for values less than the minimum supported width (typically 20) or exceeding the maximum (typically 500). Access when validating `CliOutputFormatter.ConsoleWidth` assignments.

### ValidateUseColors
```csharp
public static IReadOnlyList<string> ValidateUseColors { get; }
```
Returns the set of validation messages for color usage settings. Contains messages for invalid enum values or unsupported color mode combinations. Access when validating `CliOutputFormatter.UseColors` assignments.

### ValidatePercentage
```csharp
public static IReadOnlyList<string> ValidatePercentage { get; }
```
Returns the set of validation messages for percentage display constraints. Contains messages for values outside the valid range (0–100) or invalid formatting specifiers. Access when validating `CliOutputFormatter.Percentage` assignments.

### ValidateWidth
```csharp
public static IReadOnlyList<string> ValidateWidth { get; }
```
Returns the set of validation messages for general width constraints. Contains messages for negative values, zero, or values exceeding implementation limits. Access when validating `CliOutputFormatter.Width` assignments.

### ValidateStringList
```csharp
public static IReadOnlyList<string> ValidateStringList { get; }
```
Returns the set of validation messages for string list properties. Contains messages for null references, empty collections, or entries containing invalid characters. Access when validating list-type formatter properties.

### ValidateMessage
```csharp
public static IReadOnlyList<string> ValidateMessage { get; }
```
Returns the set of validation messages for custom message templates. Contains messages for null/empty strings, placeholder mismatches, or format string syntax errors. Access when validating `CliOutputFormatter.Message` assignments.

## Usage

```csharp
var formatter = new CliOutputFormatter();
var errors = new List<string>();

if (formatter.ConsoleWidth < 20 || formatter.ConsoleWidth > 500)
    errors.AddRange(CliOutputFormatterValidation.ValidateConsoleWidth);

if (formatter.Width <= 0)
    errors.AddRange(CliOutputFormatterValidation.ValidateWidth);

if (errors.Count > 0)
    throw new ArgumentException(string.Join(Environment.NewLine, errors));
```

```csharp
public bool TryConfigureFormatter(CliOutputFormatter formatter, out IReadOnlyList<string> validationErrors)
{
    var allErrors = new List<string>();

    if (!Enum.IsDefined(typeof(ColorMode), formatter.UseColors))
        allErrors.AddRange(CliOutputFormatterValidation.ValidateUseColors);

    if (formatter.Percentage < 0 || formatter.Percentage > 100)
        allErrors.AddRange(CliOutputFormatterValidation.ValidatePercentage);

    if (formatter.Message is { Length: > 0 } msg && msg.Contains("{invalid}"))
        allErrors.AddRange(CliOutputFormatterValidation.ValidateMessage);

    validationErrors = allErrors;
    return validationErrors.Count == 0;
}
```

## Notes

- All members return pre-allocated, immutable `IReadOnlyList<string>` instances; repeated access returns the same collection reference.
- Thread-safe: the returned collections are read-only and initialized at type initialization time, safe for concurrent access without synchronization.
- The lists contain message templates, not formatted errors; consumers must substitute parameter values (e.g., actual width value) when presenting to users.
- Empty lists indicate no validation rules defined for that property; treat as "validation passes" rather than an error condition.
- Validation logic (range checks, enum validation, etc.) is not encapsulated here—these collections only supply the message catalog.
