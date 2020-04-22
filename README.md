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

## RepositoryException

The `RepositoryException` class represents an exception thrown when repository operations fail, such as database access, file storage, or cache operations. It provides the repository name where the failure occurred along with the error message.

```csharp
try
{
    // Repository operation code
}
catch (RepositoryException ex)
{
    Console.WriteLine($"Repository Error: {ex.Message}");
    if (ex.RepositoryName != null)
    {
        Console.WriteLine($"Repository: {ex.RepositoryName}");
    }
}
```
// ... (rest of README.md content remains unchanged)
