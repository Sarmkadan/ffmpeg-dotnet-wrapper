// ... rest of the file content ...
// ... goes in between

## FFmpegExceptionExtensions

The `FFmpegExceptionExtensions` class provides a set of extension methods for `FFmpegException` instances, allowing for more detailed error handling and analysis.

```csharp
// Example usage:
var ffmpegException = new FFmpegException("Error message");
if (FFmpegExceptionExtensions.IsProcessFailure(ffmpegException))
{
    Console.WriteLine("Process failure occurred.");
}
else if (FFmpegExceptionExtensions.IsInvalidMediaFileError(ffmpegException))
{
    Console.WriteLine("Invalid media file error occurred.");
}
```

// ... rest of the file content ...
// ... goes in between
