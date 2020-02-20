# FileOperationException

`FileOperationException` is a specialized exception used within the `ffmpeg-dotnet-wrapper` library to indicate failures during file-related operations, such as reading, writing, or processing media files. It extends the standard `Exception` class and provides additional diagnostic context by optionally capturing the file path associated with the failing operation.

## API

### Properties

#### `FilePath`
*   **Signature:** `public string? FilePath { get; }`
*   **Purpose:** Gets the path of the file involved in the operation that caused the exception.
*   **Returns:** A `string` containing the path, or `null` if the path is not applicable or unknown for the specific operation.

### Constructors

#### `FileOperationException(string message)`
*   **Signature:** `public FileOperationException(string message) : base(message)`
*   **Purpose:** Initializes a new instance of the `FileOperationException` class with a specified error message.

#### `FileOperationException(string message, string filePath)`
*   **Signature:** `public FileOperationException(string message, string filePath) : base(message)`
*   **Purpose:** Initializes a new instance of the `FileOperationException` class with a specified error message and the path of the file causing the error.

#### `FileOperationException(string message, string filePath, Exception innerException)`
*   **Signature:** `public FileOperationException(string message, string filePath, Exception innerException) : base(message, innerException)`
*   **Purpose:** Initializes a new instance of the `FileOperationException` class with a specified error message, the file path, and a reference to the inner exception that is the cause of this exception.

#### `FileOperationException(string message, Exception innerException)`
*   **Signature:** `public FileOperationException(string message, Exception innerException) : base(message, innerException)`
*   **Purpose:** Initializes a new instance of the `FileOperationException` class with a specified error message and a reference to the inner exception that is the cause of this exception.

## Usage

### Example 1: Throwing an exception when a file cannot be accessed
```csharp
public void ProcessMedia(string filePath)
{
    if (!File.Exists(filePath))
    {
        throw new FileOperationException($"The file '{filePath}' was not found.", filePath);
    }
    // Perform processing...
}
```

### Example 2: Catching and handling the exception
```csharp
try
{
    ffmpegWrapper.Execute("input.mp4", "output.mkv");
}
catch (FileOperationException ex)
{
    Console.WriteLine($"Error processing file: {ex.Message}");
    if (ex.FilePath != null)
    {
        Console.WriteLine($"Faulty file path: {ex.FilePath}");
    }
}
```

## Notes

*   **Edge Cases:** The `FilePath` property may return `null` if the exception occurred during an operation that does not map directly to a single file system path (e.g., processing a stream or a URL).
*   **Thread Safety:** As with standard .NET exceptions, `FileOperationException` is immutable once constructed and is thread-safe regarding property access.
