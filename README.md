// ... (rest of README.md content remains unchanged)

## ConfigurationException

The `ConfigurationException` class represents an exception that occurs when a configuration error is encountered. It provides information about the configuration key that caused the error.

```csharp
try
{
    // Configuration code
}
catch (ConfigurationException ex)
{
    Console.WriteLine($"Configuration Error: {ex.Message}");
    Console.WriteLine($"Configuration Key: {ex.ConfigurationKey}");
}
```
// ... (rest of README.md content remains unchanged)
