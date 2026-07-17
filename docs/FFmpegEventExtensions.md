# FFmpegEventExtensions

FFmpegEventExtensions provides a set of static extension methods for working with FFmpeg operation events. These methods simplify common tasks such as determining operation outcomes, extracting file paths, progress information, and error details from event objects. The extensions are designed to work with event types generated during FFmpeg processing operations, enabling developers to handle logging, monitoring, and error handling more effectively.

## API

### `IsSuccess`
**Purpose:** Determines whether the FFmpeg operation completed successfully.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `bool` — `true` if the operation was successful; otherwise `false`.  
**Exceptions:** None.  

### `IsFailure`
**Purpose:** Determines whether the FFmpeg operation failed.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `bool` — `true` if the operation failed; otherwise `false`.  
**Exceptions:** None.  

### `GetOperationType`
**Purpose:** Retrieves the type of FFmpeg operation (e.g., transcode, merge).  
**Parameters:** None (extension method on event instance).  
**Return Value:** `string` — The operation type identifier.  
**Exceptions:** None.  

### `GetInputFile`
**Purpose:** Gets the input file path associated with the event.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `string?` — The input file path, or `null` if not applicable.  
**Exceptions:** None.  

### `GetOutputFile`
**Purpose:** Gets the output file path associated with the event.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `string?` — The output file path, or `null` if not applicable.  
**Exceptions:** None.  

### `GetErrorMessage`
**Purpose:** Retrieves the error message from a failed operation.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `string?` — The error message, or `null` if no error occurred.  
**Exceptions:** None.  

### `GetProgressPercentage`
**Purpose:** Gets the progress percentage of the ongoing operation.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `double?` — The progress value between 0 and 100, or `null` if unavailable.  
**Exceptions:** None.  

### `GetDuration`
**Purpose:** Retrieves the duration of the processed media.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `TimeSpan?` — The duration, or `null` if not applicable.  
**Exceptions:** None.  

### `GetOutputFileSize`
**Purpose:** Gets the size of the output file in bytes.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `long?` — The file size, or `null` if not applicable.  
**Exceptions:** None.  

### `GetErrorCode`
**Purpose:** Retrieves the error code from a failed operation.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `string?` — The error code, or `null` if no error occurred.  
**Exceptions:** None.  

### `ToLogString`
**Purpose:** Formats the event data into a string suitable for logging.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `string` — A formatted log string.  
**Exceptions:** None.  

### `HasCorrelationId`
**Purpose:** Checks if the event contains a correlation identifier.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `bool` — `true` if a correlation ID exists; otherwise `false`.  
**Exceptions:** None.  

### `GetMetadataString`
**Purpose:** Retrieves metadata associated with the event as a string.  
**Parameters:** None (extension method on event instance).  
**Return Value:** `string` — The metadata string, or an empty string if none exists.  
**Exceptions:** None.  

## Usage

### Example 1: Handling Operation Outcomes
```csharp
var ffmpegEvent = GetFFmpegEvent();

if (ffmpegEvent.IsFailure())
{
    Console.WriteLine($"Operation failed: {ffmpegEvent.GetErrorMessage()}");
    Console.WriteLine($"Error Code: {ffmpegEvent.GetErrorCode()}");
}
else
{
    Console.WriteLine($"Operation succeeded. Output: {ffmpegEvent.GetOutputFile()}");
}
```

### Example 2: Monitoring Progress
```csharp
var ffmpegEvent = GetFFmpegEvent();

if (ffmpegEvent.GetProgressPercentage() is double progress)
{
    Console.WriteLine($"Progress: {progress:F2}%");
}

if (ffmpegEvent.GetDuration() is TimeSpan duration)
{
    Console.WriteLine($"Processed Duration: {duration}");
}
```

## Notes

- All methods are static and intended to be used as extension methods on FFmpeg event types.  
- Methods returning nullable types (`string?`, `double?`, `TimeSpan?`, `long?`) may return `null` when the underlying data is unavailable or inapplicable.  
- `GetMetadataString` returns an empty string instead of `null` for consistency with string-based APIs.  
- Thread safety depends on the thread safety of the underlying event object being extended.  
- `ToLogString` is recommended for diagnostic logging to ensure consistent formatting across different event types.
