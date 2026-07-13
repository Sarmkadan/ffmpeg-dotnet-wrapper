// ... (rest of README.md content remains unchanged)

## FileOperationException

The `FileOperationException` class represents an exception that occurs when a file-related error is encountered. It provides information about the file path and the error message.

```csharp
try
{
    // File code
}
catch (FileOperationException ex)
{
    Console.WriteLine($"File Error: {ex.Message}");
    Console.WriteLine($"File Path: {ex.FilePath}");
}
```
// ... (rest of README.md content remains unchanged)
