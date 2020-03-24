# FileUtilities

The `FileUtilities` class provides a collection of static helper methods designed to streamline common file system operations specifically tailored for media processing workflows within the `ffmpeg-dotnet-wrapper` project. It offers robust validation for input and output paths, utilities for sanitizing filenames, retrieving file metadata, and managing temporary files, ensuring that file interactions adhere to strict formatting and existence requirements before being passed to FFmpeg processes.

## API

### `IsValidFilePath`
Determines whether a given string represents a syntactically valid file path on the current operating system.
*   **Parameters**: `string path` – The file path to validate.
*   **Returns**: `bool` – `true` if the path contains valid characters and structure; otherwise, `false`.
*   **Throws**: No exceptions are thrown; invalid inputs result in a `false` return value.

### `IsValidInputFile`
Verifies that a specified path points to an existing file that is accessible for reading.
*   **Parameters**: `string path` – The path to the potential input file.
*   **Returns**: `bool` – `true` if the file exists and is readable; otherwise, `false`.
*   **Throws**: No exceptions are thrown; access issues or missing files result in a `false` return value.

### `IsValidOutputPath`
Validates that a specified path is suitable for creating a new output file, ensuring the directory exists and the path is writable.
*   **Parameters**: `string path` – The intended output file path.
*   **Returns**: `bool` – `true` if the parent directory exists and is writable; otherwise, `false`.
*   **Throws**: No exceptions are thrown; permission issues or missing directories result in a `false` return value.

### `GetFileExtension`
Extracts the file extension from a given file path, including the leading period.
*   **Parameters**: `string path` – The file path to analyze.
*   **Returns**: `string` – The file extension (e.g., `.mp4`) or an empty string if no extension is present.
*   **Throws**: `ArgumentNullException` if `path` is null.

### `GetHumanReadableFileSize`
Converts a file size in bytes into a human-readable string format (e.g., "1.5 MB").
*   **Parameters**: `long fileSize` – The size of the file in bytes.
*   **Returns**: `string` – A formatted string representing the size with appropriate units (B, KB, MB, GB, etc.).
*   **Throws**: `ArgumentOutOfRangeException` if `fileSize` is negative.

### `GetFileSize`
Retrieves the exact size of a file in bytes.
*   **Parameters**: `string path` – The path to the file.
*   **Returns**: `long` – The size of the file in bytes.
*   **Throws**: `FileNotFoundException` if the file does not exist; `IOException` if the file is inaccessible.

### `SafeDeleteFile`
Attempts to delete a file if it exists, suppressing exceptions if the operation fails.
*   **Parameters**: `string path` – The path to the file to delete.
*   **Returns**: `bool` – `true` if the file was successfully deleted or did not exist; `false` if the deletion failed due to permissions or locks.
*   **Throws**: No exceptions are thrown; all errors are handled internally.

### `GetTempFilePath`
Generates a unique, unused file path within the system's temporary directory, optionally preserving a specific extension.
*   **Parameters**: `string extension` – (Optional) The desired file extension (e.g., `.tmp`). If null or empty, a random extension is used.
*   **Returns**: `string` – A full path to a non-existent temporary file.
*   **Throws**: `IOException` if a unique path cannot be generated after multiple attempts.

### `SanitizeFileName`
Removes or replaces invalid characters from a filename string to ensure compatibility with the file system.
*   **Parameters**: `string fileName` – The raw filename to sanitize.
*   **Returns**: `string` – A cleaned filename safe for use in path construction.
*   **Throws**: `ArgumentNullException` if `fileName` is null.

### `AreFormatsCompatible`
Checks if two file formats (extensions) are generally compatible for direct stream copying or specific conversion scenarios supported by the wrapper.
*   **Parameters**: `string sourceExtension`, `string targetExtension` – The extensions to compare (with or without leading periods).
*   **Returns**: `bool` – `true` if the formats are deemed compatible for optimized processing; otherwise, `false`.
*   **Throws**: `ArgumentNullException` if either extension is null.

## Usage

### Example 1: Validating and Preparing Input/Output Paths
This example demonstrates how to validate an input media file, sanitize a user-provided output filename, and verify the output destination before initiating a process.

```csharp
using FFmpegWrapper.Utilities;

public void ProcessMedia(string inputPath, string rawOutputName)
{
    // Validate the source file exists and is readable
    if (!FileUtilities.IsValidInputFile(inputPath))
    {
        throw new InvalidOperationException("Input file is missing or inaccessible.");
    }

    // Sanitize the output filename provided by the user
    string safeFileName = FileUtilities.SanitizeFileName(rawOutputName);
    string outputPath = System.IO.Path.Combine(@"C:\Exports", safeFileName);

    // Ensure the output path is writable
    if (!FileUtilities.IsValidOutputPath(outputPath))
    {
        throw new InvalidOperationException("Output path is invalid or directory is not writable.");
    }

    // Retrieve file size for logging
    long sizeBytes = FileUtilities.GetFileSize(inputPath);
    Console.WriteLine($"Processing {FileUtilities.GetHumanReadableFileSize(sizeBytes)}...");
    
    // Proceed with FFmpeg logic...
}
```

### Example 2: Managing Temporary Files and Cleanup
This example illustrates generating a temporary file path for intermediate processing and ensuring safe cleanup after the operation completes.

```csharp
using FFmpegWrapper.Utilities;
using System.IO;

public void PerformIntermediateConversion()
{
    string tempPath = null;
    try
    {
        // Create a unique temp path with .mkv extension
        tempPath = FileUtilities.GetTempFilePath(".mkv");
        
        // Perform intermediate writing logic here...
        File.WriteAllText(tempPath, "dummy data"); 

        // Verify the temp file was created
        if (FileUtilities.IsValidInputFile(tempPath))
        {
            Console.WriteLine($"Temp file created: {FileUtilities.GetHumanReadableFileSize(FileUtilities.GetFileSize(tempPath))}");
        }
    }
    finally
    {
        // Safely attempt to delete the temp file, ignoring errors if locked or missing
        if (!string.IsNullOrEmpty(tempPath))
        {
            bool deleted = FileUtilities.SafeDeleteFile(tempPath);
            if (!deleted)
            {
                Console.WriteLine("Warning: Temporary file could not be deleted automatically.");
            }
        }
    }
}
```

## Notes

*   **Thread Safety**: All methods in `FileUtilities` are static and stateless, making them inherently thread-safe for concurrent calls, provided the underlying file system operations do not conflict on the same specific file paths (e.g., two threads attempting to delete the same file simultaneously).
*   **Exception Handling**: Methods prefixed with `IsValid` and `SafeDeleteFile` are designed to fail gracefully by returning `false` rather than throwing exceptions, suitable for pre-flight checks. Conversely, data retrieval methods like `GetFileSize` will throw standard .NET IO exceptions if the operation cannot be completed.
*   **Path Separators**: The validation methods (`IsValidFilePath`, `IsValidInputFile`, `IsValidOutputPath`) rely on the host operating system's path rules. Paths constructed manually should use `Path.Combine` to ensure cross-platform compatibility.
*   **Format Compatibility**: The `AreFormatsCompatible` method relies on a static internal mapping of container formats. It does not inspect the actual binary content of the files, only their extensions.
