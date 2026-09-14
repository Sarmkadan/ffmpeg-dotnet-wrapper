# ProcessResult

The `ProcessResult` class represents the outcome of a process execution within the `ffmpeg-dotnet-wrapper` project. It provides access to the exit code, standard error output tail, and flags indicating timeout or cancellation status.

## API

### Properties

*   **`ExitCode`** (`int`)
    Gets or sets the exit code of the process.

*   **`StdErrTail`** (`string`)
    Gets or sets the tail of the standard error output. Defaults to an empty string.

*   **`TimedOut`** (`bool`)
    Gets or sets a value indicating whether the process timed out.

*   **`WasCancelled`** (`bool`)
    Gets or sets a value indicating whether the process was cancelled.

### Methods

*   **`ToString()`** (`string`)
    Returns a culture-invariant string representation of the process result.
    Returns a string containing `ExitCode`, `TimedOut`, `WasCancelled`, and a truncated `StdErrTail` (limited to 100 characters).

## Usage

### Checking Process Outcome
```csharp
var result = ffmpegProcessRunner.Run(processInfo);

if (result.ExitCode == 0 && !result.TimedOut && !result.WasCancelled)
{
    Console.WriteLine("Process completed successfully.");
}
else
{
    Console.WriteLine($"Process failed: {result}");
}
```

### Logging Detailed Error Information
```csharp
var result = ffmpegProcessRunner.Run(processInfo);

if (!result.IsSuccess) // Note: IsSuccess is not a property; check ExitCode and flags instead
{
    Console.WriteLine($"Process exited with code {result.ExitCode}");
    if (result.TimedOut)
    {
        Console.WriteLine("Process timed out.");
    }
    if (result.WasCancelled)
    {
        Console.WriteLine("Process was cancelled.");
    }
    if (!string.IsNullOrEmpty(result.StdErrTail))
    {
        Console.WriteLine($"Standard error tail: {result.StdErrTail}");
    }
}
```

## Notes

*   **String Representation:** The `ToString()` method truncates `StdErrTail` to 100 characters, appending "..." if truncated, to prevent excessively long output.
*   **Error Handling:** The class does not include an `IsSuccess` property; success must be determined by checking `ExitCode == 0` and that `TimedOut` and `WasCancelled` are false.
*   **Thread Safety:** The `ProcessResult` class does not contain static state and is safe for concurrent use by different threads, but instances should not be shared without synchronization if modified after creation.