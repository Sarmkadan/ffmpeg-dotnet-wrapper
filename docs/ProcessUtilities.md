# ProcessUtilities

The `ProcessUtilities` class provides a set of static and instance members for executing external processes, capturing their output and error streams, measuring execution time, and handling timeouts. It is designed primarily for wrapping command-line tools such as FFmpeg, but can be used with any executable. Instance properties (`ExitCode`, `StandardOutput`, `StandardError`, `ExecutionTime`, `TimedOut`) are populated after a process has been run via the static execution methods.

## API

### `public int ExitCode`

Gets the exit code returned by the executed process. A value of `0` typically indicates success. This property is only meaningful after a process has been run.

### `public string StandardOutput`

Gets the complete standard output stream of the executed process as a single string. May be empty if the process produced no output.

### `public string StandardError`

Gets the complete standard error stream of the executed process as a single string. May be empty if the process produced no error output.

### `public TimeSpan ExecutionTime`

Gets the total elapsed time measured from process start to process exit. This value is set regardless of whether the process timed out.

### `public bool TimedOut`

Indicates whether the process was terminated due to a timeout. `true` if the process exceeded the allowed execution time; otherwise `false`.

### `public static ProcessResult ExecuteProcess`

Synchronously executes a process and returns a `ProcessResult` object containing the exit code, standard output, standard error, execution time, and timeout status. The method blocks the calling thread until the process exits or times out.

- **Parameters:** (not specified in the public API – typically accepts an executable path, arguments, and optional timeout settings)
- **Returns:** A `ProcessResult` instance with the results of the execution.
- **Throws:** `InvalidOperationException` if the process cannot be started (e.g., executable not found). `TimeoutException` if the process times out and the implementation throws on timeout (otherwise the `TimedOut` property is set).

### `public static async Task<ProcessResult> ExecuteProcessAsync`

Asynchronously executes a process and returns a `Task<ProcessResult>`. The calling thread is not blocked while the process runs.

- **Parameters:** (not specified in the public API – typically accepts an executable path, arguments, and optional cancellation token or timeout)
- **Returns:** A task that resolves to a `ProcessResult` instance.
- **Throws:** `InvalidOperationException` if the process cannot be started. `OperationCanceledException` if the operation is cancelled.

### `public static bool IsExecutableAvailable`

Checks whether a given executable is available on the system (i.e., can be found in the PATH or at the specified location).

- **Parameters:** (not specified in the public API – typically accepts the executable name or path)
- **Returns:** `true` if the executable is available; otherwise `false`.
- **Throws:** None.

### `public static double ExtractProgressPercentage`

Parses a line of output (typically from FFmpeg) and extracts a progress percentage as a double between `0.0` and `100.0`.

- **Parameters:** (not specified in the public API – typically accepts a string line of output)
- **Returns:** The extracted percentage, or a negative value (e.g., `-1.0`) if the line does not contain progress information.
- **Throws:** None.

### `public static string EscapeArgument`

Escapes a string argument so that it can be safely passed to a command-line process, handling spaces, quotes, and special characters.

- **Parameters:** (not specified in the public API – typically accepts the raw argument string)
- **Returns:** The escaped argument string.
- **Throws:** None.

## Usage

### Example 1: Synchronous execution with timeout

```csharp
using ProcessUtilities;

var result = ProcessUtilities.ExecuteProcess(
    executable: "ffmpeg",
    arguments: "-i input.mp4 -vf scale=320:240 output.mp4",
    timeout: TimeSpan.FromSeconds(30));

if (result.TimedOut)
{
    Console.WriteLine("Process timed out after {0}", result.ExecutionTime);
}
else
{
    Console.WriteLine("Exit code: {0}", result.ExitCode);
    Console.WriteLine("Output: {0}", result.StandardOutput);
    Console.WriteLine("Error: {0}", result.StandardError);
}
```

### Example 2: Asynchronous execution with progress extraction

```csharp
using ProcessUtilities;

var task = ProcessUtilities.ExecuteProcessAsync(
    executable: "ffmpeg",
    arguments: "-i input.mp4 -c:v libx264 output.mp4",
    cancellationToken: CancellationToken.None);

// While the process runs, you could periodically read output lines
// and call ExtractProgressPercentage to update a progress bar.
// (This example omits the actual reading loop for brevity.)

var result = await task;

if (result.ExitCode == 0)
{
    Console.WriteLine("Conversion succeeded in {0}", result.ExecutionTime);
}
else
{
    Console.WriteLine("Conversion failed with error: {0}", result.StandardError);
}
```

## Notes

- **Thread safety:** The static methods (`ExecuteProcess`, `ExecuteProcessAsync`, `IsExecutableAvailable`, `ExtractProgressPercentage`, `EscapeArgument`) are thread-safe. Instance properties (`ExitCode`, `StandardOutput`, etc.) are intended to be read after a process has completed; they are not safe to modify concurrently from multiple threads.
- **Edge cases:** If a process produces no output, `StandardOutput` and `StandardError` will be empty strings. `ExecutionTime` is always set, even if the process fails to start (in that case it will be near zero). `TimedOut` is `false` if the process exits normally before the timeout.
- **Timeout behavior:** When a timeout occurs, the process is killed. The `ExitCode` property will reflect the exit code of the killed process (typically a non‑zero value). The `TimedOut` property is set to `true`.
- **Executable availability:** `IsExecutableAvailable` checks the system PATH by default; if the executable is specified with a full path, it verifies that the file exists and is executable.
- **Progress extraction:** `ExtractProgressPercentage` expects lines in the format produced by FFmpeg (e.g., `frame=  123 fps=... time=00:00:05.00 ...`). It returns `-1.0` for lines that do not match the expected pattern.
- **Argument escaping:** `EscapeArgument` wraps the argument in double quotes if it contains spaces or special characters, and escapes any embedded double quotes. It does not handle all platform‑specific quoting rules (e.g., Unix vs. Windows) – use with caution on cross‑platform scenarios.
