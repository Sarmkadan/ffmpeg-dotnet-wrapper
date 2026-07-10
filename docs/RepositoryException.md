# RepositoryException

`RepositoryException` is a custom exception class designed to signal errors occurring during operations within the `ffmpeg-dotnet-wrapper` repository layer. By allowing the inclusion of an optional repository identifier, it enables calling code to more effectively diagnose, categorize, or report failures related to specific infrastructure components or data sources.

## API

### Properties

- `public string? RepositoryName { get; }`
  - Gets the name of the repository associated with the error. May be `null` if no specific repository context was provided during instantiation.

### Constructors

- `public RepositoryException(string message)`
  - Initializes a new instance of the `RepositoryException` class with a specified error message.

- `public RepositoryException(string message, string repositoryName)`
  - Initializes a new instance of the `RepositoryException` class with a specified error message and the name of the repository involved in the failure.

- `public RepositoryException(string message, Exception innerException)`
  - Initializes a new instance of the `RepositoryException` class with a specified error message and a reference to the inner exception that is the cause of this exception.

- `public RepositoryException(string message, string repositoryName, Exception innerException)`
  - Initializes a new instance of the `RepositoryException` class with a specified error message, the repository name, and the inner exception that caused the current error.

## Usage

### Example 1: Catching and wrapping a low-level exception
```csharp
try
{
    // Simulating a database or file system access error
    await _fileRepository.LoadAsync("video.mp4");
}
catch (IOException ex)
{
    throw new RepositoryException("Failed to load media file.", nameof(_fileRepository), ex);
}
```

### Example 2: Throwing an exception for a validation failure
```csharp
if (string.IsNullOrWhiteSpace(repositoryName))
{
    throw new RepositoryException("Repository name cannot be null or empty.");
}
```

## Notes

- **Thread Safety**: This class is immutable regarding its state after construction and is thread-safe for reading.
- **Serialization**: As with all exception types, if this exception is intended to cross application domain boundaries or be serialized, ensure that custom serialization logic is implemented if necessary, though it typically relies on the standard `Exception` serialization mechanisms.
- **RepositoryName**: This property is nullable. Callers should handle potential null values when accessing `RepositoryName` if they cannot guarantee that it was provided during the construction of the exception instance.
