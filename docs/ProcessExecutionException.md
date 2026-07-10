# ProcessExecutionException

The `ProcessExecutionException` class is a specialized exception designed to be thrown when an underlying FFmpeg process fails during execution. It captures critical diagnostic information, including the non-zero exit code returned by the process and any standard error output produced, facilitating robust error handling and debugging within applications leveraging the `ffmpeg-dotnet-wrapper`.

## API

### Properties

*   **`public new int? ExitCode`**
    Gets the exit code returned by the failed process, if available. Returns `null` if a code was not provided or could not be determined.
*   **`public new string? ErrorOutput`**
    Gets the captured standard error output from the failed process. Returns `null` or an empty string if no error output was generated.

### Constructors

*   **`ProcessExecutionException(string message)`**
    Initializes a new instance with a specified error message. Calls the base `Exception` constructor.
*   **`ProcessExecutionException(string message, int exitCode)`**
    Initializes a new instance with an error message and the associated exit code.
*   **`ProcessExecutionException(string message, int exitCode, string errorOutput)`**
    Initializes a new instance with an error message, the exit code, and the captured error output.
*   **`ProcessExecutionException(string message, Exception innerException)`**
    Initializes a new instance with an error message and a reference to the inner exception that caused this failure.
*   **`ProcessExecutionException(string message, int exitCode, string errorOutput, Exception innerException)`**
    Initializes a new instance with an error message, the exit code, the error output, and the inner exception.

## Usage

### Basic Exception Handling

```csharp
try
{
    await ffmpegProcess.ExecuteAsync();
}
catch (ProcessExecutionException ex)
{
    Console.WriteLine($"FFmpeg operation failed: {ex.Message}");
}
```

### Detailed Diagnostic Handling

```csharp
try
{
    await ffmpegProcess.ExecuteAsync();
}
catch (ProcessExecutionException ex)
{
    Logger.LogError("FFmpeg failed with exit code: {ExitCode}", ex.ExitCode);
    
    if (!string.IsNullOrEmpty(ex.ErrorOutput))
    {
        Logger.LogError("Error details: {ErrorOutput}", ex.ErrorOutput);
    }
    
    throw; // Rethrow or handle accordingly
}
```

## Notes

*   **Thread Safety:** Instances of `ProcessExecutionException` are thread-safe for reading once constructed, as the `ExitCode` and `ErrorOutput` properties are effectively immutable.
*   **Edge Cases:** 
    *   The `ExitCode` property may be `null` if the process failed to initialize or was terminated abruptly before an exit code could be captured.
    *   `ErrorOutput` may be `null` or empty if the process did not write to the standard error stream, or if the stream was unable to be captured.
*   **Inheritance:** This class hides the `ExitCode` and `ErrorOutput` members of its base class (indicated by the `new` modifier) to provide more specific, nullable types relevant to the process execution context.
