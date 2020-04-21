// ... (rest of README.md content remains unchanged)

## ValidationException

The `ValidationException` class represents an exception that occurs when validation fails. It provides information about the validation errors, including a dictionary of validation errors.

```csharp
try
{
    // Validation code
}
catch (ValidationException ex)
{
    Console.WriteLine($"Validation Errors:");
    foreach (var error in ex.ValidationErrors ?? new Dictionary<string, string[]>())
    {
        Console.WriteLine($"  - {error.Key}: {string.Join(", ", error.Value)}");
    }
}
```
// ... (rest of README.md content remains unchanged)
