# FFmpegOptions

The `FFmpegOptions` class serves as a centralized configuration container for the `ffmpeg-dotnet-wrapper` library, allowing developers to customize the behavior of FFmpeg and FFprobe executions. It defines paths to binaries, resource constraints such as timeouts and file sizes, hardware acceleration flags, encoding presets, and operational policies regarding concurrency, retry logic, and temporary file management.

## API

### FFmpegPath
*   **Type**: `public string? FFmpegPath`
*   **Description**: Specifies the absolute or relative file path to the FFmpeg executable. If null, the library attempts to locate the binary via the system PATH environment variable.
*   **Parameters**: None (Property setter accepts a string or null).
*   **Return Value**: Returns the current path string or null.
*   **Throws**: No exceptions are thrown by the property itself; invalid paths may cause `FileNotFoundException` during execution if `ValidatePaths` is enabled.

### FFprobePath
*   **Type**: `public string? FFprobePath`
*   **Description**: Specifies the absolute or relative file path to the FFprobe executable. If null, the library attempts to locate the binary via the system PATH environment variable.
*   **Parameters**: None (Property setter accepts a string or null).
*   **Return Value**: Returns the current path string or null.
*   **Throws**: No exceptions are thrown by the property itself; invalid paths may cause `FileNotFoundException` during execution if `ValidatePaths` is enabled.

### OperationTimeoutSeconds
*   **Type**: `public int OperationTimeoutSeconds`
*   **Description**: Defines the maximum duration, in seconds, allowed for a single FFmpeg or FFprobe operation to complete before it is forcibly terminated.
*   **Parameters**: None (Property setter accepts an integer).
*   **Return Value**: Returns the current timeout value in seconds.
*   **Throws**: No exceptions thrown by the property; a value of 0 or less may result in immediate timeout depending on implementation logic.

### MaxFileSizeBytes
*   **Type**: `public long MaxFileSizeBytes`
*   **Description**: Sets the upper limit for the size of output files in bytes. Operations expected to exceed this limit may be prevented or truncated.
*   **Parameters**: None (Property setter accepts a long integer).
*   **Return Value**: Returns the current maximum file size limit.
*   **Throws**: No exceptions thrown by the property.

### EnableHardwareAcceleration
*   **Type**: `public bool EnableHardwareAcceleration`
*   **Description**: Toggles the use of hardware-accelerated encoding and decoding (e.g., NVENC, VAAPI) if supported by the underlying hardware and drivers.
*   **Parameters**: None (Property setter accepts a boolean).
*   **Return Value**: Returns `true` if hardware acceleration is enabled; otherwise `false`.
*   **Throws**: No exceptions thrown by the property.

### EncodingPreset
*   **Type**: `public string? EncodingPreset`
*   **Description**: Specifies the encoding preset (e.g., "ultrafast", "medium", "veryslow") to balance encoding speed and compression efficiency.
*   **Parameters**: None (Property setter accepts a string or null).
*   **Return Value**: Returns the current preset string or null if no specific preset is defined.
*   **Throws**: No exceptions thrown by the property; invalid preset strings may cause FFmpeg execution errors.

### KeepTemporaryFiles
*   **Type**: `public bool KeepTemporaryFiles`
*   **Description**: Determines whether intermediate or temporary files generated during processing are deleted upon completion (`false`) or preserved for debugging (`true`).
*   **Parameters**: None (Property setter accepts a boolean).
*   **Return Value**: Returns `true` if temporary files are retained; otherwise `false`.
*   **Throws**: No exceptions thrown by the property.

### TemporaryDirectory
*   **Type**: `public string? TemporaryDirectory`
*   **Description**: Defines the specific directory path where temporary files should be stored. If null, the system's default temporary directory is used.
*   **Parameters**: None (Property setter accepts a string or null).
*   **Return Value**: Returns the configured directory path or null.
*   **Throws**: No exceptions thrown by the property; invalid paths may cause `DirectoryNotFoundException` during operation.

### VerboseLogging
*   **Type**: `public bool VerboseLogging`
*   **Description**: Enables detailed logging output from FFmpeg processes, useful for debugging complex encoding issues.
*   **Parameters**: None (Property setter accepts a boolean).
*   **Return Value**: Returns `true` if verbose logging is active; otherwise `false`.
*   **Throws**: No exceptions thrown by the property.

### DefaultQuality
*   **Type**: `public int? DefaultQuality`
*   **Description**: Sets the default quality level (e.g., CRF value) for video encoding when not explicitly overridden in a specific job.
*   **Parameters**: None (Property setter accepts an integer or null).
*   **Return Value**: Returns the current quality value or null.
*   **Throws**: No exceptions thrown by the property.

### DefaultAudioBitrate
*   **Type**: `public int DefaultAudioBitrate`
*   **Description**: Defines the default audio bitrate in bits per second (bps) for encoding operations.
*   **Parameters**: None (Property setter accepts an integer).
*   **Return Value**: Returns the current default audio bitrate.
*   **Throws**: No exceptions thrown by the property.

### DefaultVideoBitrate
*   **Type**: `public int DefaultVideoBitrate`
*   **Description**: Defines the default video bitrate in bits per second (bps) for encoding operations.
*   **Parameters**: None (Property setter accepts an integer).
*   **Return Value**: Returns the current default video bitrate.
*   **Throws**: No exceptions thrown by the property.

### AllowConcurrentOperations
*   **Type**: `public bool AllowConcurrentOperations`
*   **Description**: Enables or disables the ability to run multiple FFmpeg processes simultaneously.
*   **Parameters**: None (Property setter accepts a boolean).
*   **Return Value**: Returns `true` if concurrency is allowed; otherwise `false`.
*   **Throws**: No exceptions thrown by the property.

### MaxConcurrentOperations
*   **Type**: `public int MaxConcurrentOperations`
*   **Description**: Specifies the maximum number of FFmpeg processes allowed to run in parallel when `AllowConcurrentOperations` is true.
*   **Parameters**: None (Property setter accepts an integer).
*   **Return Value**: Returns the maximum concurrency limit.
*   **Throws**: No exceptions thrown by the property.

### SupportedFormats
*   **Type**: `public List<string> SupportedFormats`
*   **Description**: A list of file extensions or format names that the wrapper is configured to accept or process.
*   **Parameters**: None (Property setter accepts a `List<string>`).
*   **Return Value**: Returns the list of supported format strings.
*   **Throws**: No exceptions thrown by the property; modifying the list concurrently may cause collection errors.

### ValidatePaths
*   **Type**: `public bool ValidatePaths`
*   **Description**: Enables pre-execution validation to ensure that input file paths and binary paths exist and are accessible.
*   **Parameters**: None (Property setter accepts a boolean).
*   **Return Value**: Returns `true` if path validation is active; otherwise `false`.
*   **Throws**: May cause `IOException` or `FileNotFoundException` during operation initialization if validation fails.

### ValidateOutputPath
*   **Type**: `public bool ValidateOutputPath`
*   **Description**: Specifically enables validation for the output file path, ensuring the directory exists and is writable before starting the process.
*   **Parameters**: None (Property setter accepts a boolean).
*   **Return Value**: Returns `true` if output path validation is active; otherwise `false`.
*   **Throws**: May cause `IOException` or `UnauthorizedAccessException` during operation initialization if validation fails.

### RetryAttempts
*   **Type**: `public int RetryAttempts`
*   **Description**: Defines the number of times the wrapper will automatically retry a failed operation before throwing a final exception.
*   **Parameters**: None (Property setter accepts an integer).
*   **Return Value**: Returns the configured retry count.
*   **Throws**: No exceptions thrown by the property.

### RetryDelayMs
*   **Type**: `public int RetryDelayMs`
*   **Description**: Specifies the delay in milliseconds between consecutive retry attempts.
*   **Parameters**: None (Property setter accepts an integer).
*   **Return Value**: Returns the delay duration in milliseconds.
*   **Throws**: No exceptions thrown by the property.

### Enabled
*   **Type**: `public bool Enabled`
*   **Description**: A master switch to enable or disable the functionality of the wrapper. If false, operations may be short-circuited or ignored.
*   **Parameters**: None (Property setter accepts a boolean).
*   **Return Value**: Returns `true` if the wrapper is active; otherwise `false`.
*   **Throws**: No exceptions thrown by the property.

## Usage

### Example 1: Basic Configuration with Hardware Acceleration
This example demonstrates configuring the wrapper to use a specific FFmpeg binary, enable hardware acceleration, and set a strict timeout.

```csharp
using FFmpegDotNetWrapper;

var options = new FFmpegOptions
{
    FFmpegPath = "/usr/local/bin/ffmpeg",
    FFprobePath = "/usr/local/bin/ffprobe",
    EnableHardwareAcceleration = true,
    EncodingPreset = "fast",
    OperationTimeoutSeconds = 300,
    VerboseLogging = false,
    Enabled = true
};

// Initialize the processor with these options
// var processor = new FFmpegProcessor(options);
```

### Example 2: Robust Processing with Retry Logic and Concurrency
This example configures the wrapper for a high-throughput environment, allowing concurrent operations with retry mechanisms and custom temporary file handling.

```csharp
using FFmpegDotNetWrapper;
using System.Collections.Generic;

var options = new FFmpegOptions
{
    AllowConcurrentOperations = true,
    MaxConcurrentOperations = 4,
    RetryAttempts = 3,
    RetryDelayMs = 1000,
    KeepTemporaryFiles = false,
    TemporaryDirectory = "/var/tmp/ffmpeg_jobs",
    ValidatePaths = true,
    ValidateOutputPath = true,
    SupportedFormats = new List<string> { "mp4", "mkv", "avi" },
    DefaultVideoBitrate = 2500000,
    DefaultAudioBitrate = 128000
};

// Assign options to the service
// FFmpegService.Configure(options);
```

## Notes

*   **Thread Safety**: The `FFmpegOptions` class itself does not appear to enforce internal locking mechanisms. While primitive property assignments are generally atomic, modifying the `SupportedFormats` list from multiple threads simultaneously without external synchronization may result in `InvalidOperationException`. It is recommended to configure this object once during application startup and treat it as immutable during runtime.
*   **Path Validation**: Enabling both `ValidatePaths` and `ValidateOutputPath` adds overhead before every operation. In environments with network-mounted storage or volatile paths, this may introduce latency or false negatives if the storage is momentarily unavailable.
*   **Concurrency Limits**: Setting `AllowConcurrentOperations` to `true` without defining a reasonable `MaxConcurrentOperations` limit (or setting it too high) can exhaust system resources (CPU, RAM, I/O), leading to degraded performance or OS-level process killing.
*   **Timeout Behavior**: The `OperationTimeoutSeconds` applies to the entire process lifecycle. For very large files or slow storage, ensure this value is sufficiently high to prevent premature termination of valid long-running tasks.
*   **Nullable Properties**: Properties such as `FFmpegPath`, `FFprobePath`, and `DefaultQuality` are nullable. If these are left null, the library relies on default resolution strategies (e.g., system PATH). Explicitly setting them to null after initialization may revert behavior to these defaults.
