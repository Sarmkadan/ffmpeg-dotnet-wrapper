// ... (rest of README.md content remains unchanged)

## FFmpegException

The `FFmpegException` class represents an exception that occurs when an error occurs during FFmpeg operations. It provides information about the error, including the exit code, error output, and file path where the error occurred.

```csharp
try
{
    // FFmpeg operation code
}
catch (FFmpegException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Exit Code: {ex.ExitCode}");
    Console.WriteLine($"Error Output: {ex.ErrorOutput}");
    Console.WriteLine($"File Path: {ex.FilePath}");
}
```
// ... (rest of README.md content remains unchanged)
