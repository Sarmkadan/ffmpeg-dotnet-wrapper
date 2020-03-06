# ThumbnailResult

The `ThumbnailResult` class serves as the immutable return container for thumbnail generation operations within the `ffmpeg-dotnet-wrapper` library. It encapsulates the outcome of an asynchronous FFmpeg process, providing a unified interface to access generated image paths, media duration metadata, and error states without requiring exception handling for routine failure scenarios.

## API

### `IsSuccess`
```csharp
public bool IsSuccess { get; }
```
Indicates whether the underlying FFmpeg process completed successfully and generated the requested thumbnails. A value of `true` confirms that the `Thumbnails` list contains valid file paths and `ErrorMessage` is null. A value of `false` indicates a processing failure, in which case `Thumbnails` will be empty or incomplete, and `ErrorMessage` will contain diagnostic details.

### `Thumbnails`
```csharp
public List<string> Thumbnails { get; }
```
A collection of absolute or relative file paths pointing to the generated thumbnail images. The order of paths in the list corresponds to the chronological order of the frames extracted from the source media. If `IsSuccess` is `false`, this list may be empty or contain only partially generated files before the error occurred.

### `Duration`
```csharp
public TimeSpan Duration { get; }
```
Represents the total duration of the source media file as detected by FFmpeg during the analysis phase. This value is populated regardless of whether thumbnail generation succeeded or failed, provided the input file was readable and metadata could be extracted. If the input file is corrupt or unreadable, this value defaults to `TimeSpan.Zero`.

### `ErrorMessage`
```csharp
public string? ErrorMessage { get; }
```
Contains a descriptive error message if `IsSuccess` is `false`. This string typically includes the FFmpeg exit code and standard error output summarizing the failure reason (e.g., invalid codec, missing input file, permission denied). If `IsSuccess` is `true`, this property is `null`.

## Usage

### Example 1: Standard Success Handling
The following example demonstrates generating thumbnails and verifying success before iterating over the resulting file paths.

```csharp
var generator = new ThumbnailGenerator();
var result = await generator.GenerateAsync("input_video.mp4", count: 5);

if (result.IsSuccess)
{
    Console.WriteLine($"Media Duration: {result.Duration}");
    
    foreach (var path in result.Thumbnails)
    {
        Console.WriteLine($"Thumbnail saved to: {path}");
        // Proceed with image processing or upload logic
    }
}
else
{
    Console.Error.WriteLine($"Thumbnail generation failed: {result.ErrorMessage}");
}
```

### Example 2: Duration-Based Logic with Fallback
This example utilizes the `Duration` property even in failure scenarios to determine if the input file was readable, allowing for specific fallback logic based on the error type.

```csharp
var result = await thumbnailService.ExtractFrameAsync("corrupt_input.mkv");

if (!result.IsSuccess)
{
    if (result.Duration == TimeSpan.Zero)
    {
        // Critical failure: Input file could not be read or parsed
        Logger.Critical("Input file is unreadable or invalid format.");
    }
    else
    {
        // Recoverable failure: Metadata read, but encoding failed
        Logger.Warning($"Encoding failed after {result.Duration}. Error: {result.ErrorMessage}");
        // Attempt alternative codec or lower resolution here
    }
    
    return; 
}

// Process successful result
ProcessImages(result.Thumbnails);
```

## Notes

*   **Immutability**: Instances of `ThumbnailResult` are intended to be immutable after creation. The `Thumbnails` list should be treated as read-only; modifying the list contents does not affect the underlying file system or the state of the completed operation.
*   **Thread Safety**: The `ThumbnailResult` class is thread-safe for read operations. Multiple threads can safely access `IsSuccess`, `Duration`, `ErrorMessage`, and iterate over `Thumbnails` concurrently without synchronization, provided no external code modifies the `Thumbnails` list reference or contents.
*   **Partial Results**: In rare edge cases where FFmpeg terminates abruptly after generating some frames but before completion, `IsSuccess` will be `false`, yet `Thumbnails` may contain paths to files that were successfully written to disk prior to the crash. Consumers should verify file existence if operating in such failure modes.
*   **Nullability**: `ErrorMessage` is explicitly nullable. Code accessing this property must check `IsSuccess` or perform a null check before reading the error string to avoid potential null reference exceptions in strict null-context environments.
