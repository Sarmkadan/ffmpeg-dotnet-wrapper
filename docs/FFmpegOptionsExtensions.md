# FFmpegOptionsExtensions

Provides extension methods for retrieving and interpreting FFmpeg-related configuration options from the application's settings. These methods centralize access to runtime parameters used by the `ffmpeg-dotnet-wrapper` library, allowing consumers to query effective paths, encoding presets, timeouts, concurrency limits, and other behavior flags without direct dependency on internal configuration structures.

## API

### `GetEffectiveFFmpegPath()`
Returns the resolved path to the FFmpeg executable, or `null` if no valid path is configured. The effective path is determined by checking the `FFmpegPath` setting, falling back to the system `PATH` environment variable if the setting is missing or empty. This method does not validate the existence of the resolved path.

### `GetEffectiveFFprobePath()`
Returns the resolved path to the FFprobe executable, or `null` if no valid path is configured. Similar to `GetEffectiveFFmpegPath`, it checks the `FFprobePath` setting and falls back to the system `PATH`. The returned path is not validated for existence.

### `IsHardwareAccelerationEnabled()`
Indicates whether hardware acceleration is enabled for FFmpeg operations. The value is derived from the `HardwareAccelerationEnabled` configuration setting. Returns `true` if acceleration is enabled, otherwise `false`.

### `GetEffectiveEncodingPreset()`
Returns the effective encoding preset string used by FFmpeg, typically one of `ultrafast`, `superfast`, `veryfast`, `faster`, `fast`, `medium`, `slow`, `slower`, or `veryslow`. The preset is resolved from the `EncodingPreset` configuration setting, defaulting to `medium` if not specified.

### `GetTimeoutMilliseconds()`
Returns the timeout duration in milliseconds for FFmpeg operations. The value is read from the `TimeoutMilliseconds` setting, defaulting to `30000` (30 seconds) if not configured. This timeout applies to process execution and I/O operations.

### `CanRunConcurrently()`
Indicates whether FFmpeg operations are allowed to run concurrently. The value is derived from the `ConcurrentOperationsEnabled` configuration setting. Returns `true` if concurrent execution is permitted, otherwise `false`.

### `GetMaxConcurrentOperations()`
Returns the maximum number of concurrent FFmpeg operations permitted. The value is read from the `MaxConcurrentOperations` setting, defaulting to `4` if not specified. This limit is enforced only when `CanRunConcurrently()` returns `true`.

### `IsFormatSupported(string format)`
Determines whether the specified media format is supported by the configured FFmpeg build. The `format` parameter is case-insensitive and should match a format identifier (e.g., `mp4`, `avi`). Returns `true` if the format is supported, otherwise `false`. This method does not throw exceptions; unsupported formats return `false`.

### `GetSupportedFormatsString()`
Returns a comma-separated string listing all media formats supported by the configured FFmpeg build. The string is generated from the output of `ffmpeg -formats` and is cached after the first invocation. The result is never `null`; if no formats are supported, an empty string is returned.

### `GetEffectiveTemporaryDirectory()`
Returns the effective temporary directory path used for storing intermediate files. The path is resolved from the `TemporaryDirectory` setting, defaulting to the system's temporary directory (e.g., `%TEMP%` on Windows, `/tmp` on Unix) if not specified. The returned path is guaranteed to be non-null and absolute.

### `ShouldKeepTemporaryFiles()`
Indicates whether temporary files generated during FFmpeg operations should be retained after completion. The value is derived from the `KeepTemporaryFiles` configuration setting. Returns `true` if files should be kept, otherwise `false`.

### `GetRetryConfiguration()`
Returns a tuple `(Attempts, DelayMs)` representing the retry policy for failed FFmpeg operations. The values are read from the `RetryAttempts` and `RetryDelayMilliseconds` settings, defaulting to `(3, 1000)` if not configured. `Attempts` indicates the maximum number of retry attempts; `DelayMs` specifies the delay in milliseconds between retries.

### `IsVerboseLoggingEnabled()`
Indicates whether verbose logging is enabled for FFmpeg operations. The value is derived from the `VerboseLogging` configuration setting. Returns `true` if verbose logging is enabled, otherwise `false`.

### `GetDefaultAudioBitrate()`
Returns the default audio bitrate in kilobits per second (kbps) used when not explicitly specified. The value is read from the `DefaultAudioBitrate` setting, defaulting to `192` if not configured.

### `GetDefaultVideoBitrate()`
Returns the default video bitrate in kilobits per second (kbps) used when not explicitly specified. The value is read from the `DefaultVideoBitrate` setting, defaulting to `2500` if not configured.

### `GetDefaultQuality()`
Returns the default quality level as a nullable integer, typically in the range `0`–`100` for encoding quality settings. The value is read from the `DefaultQuality` setting. Returns `null` if no default quality is configured.

### `IsPathValidationEnabled()`
Indicates whether path validation is enabled for FFmpeg and FFprobe executables. The value is derived from the `ValidatePaths` configuration setting. Returns `true` if validation is enabled, otherwise `false`.

### `IsOutputPathValidationEnabled()`
Indicates whether validation of output file paths is enabled. The value is derived from the `ValidateOutputPaths` configuration setting. Returns `true` if output path validation is enabled, otherwise `false`.

## Usage

```csharp
// Example 1: Checking hardware acceleration and encoding preset
bool useHardwareAcceleration = FFmpegOptionsExtensions.IsHardwareAccelerationEnabled();
string preset = FFmpegOptionsExtensions.GetEffectiveEncodingPreset();

Console.WriteLine($"Hardware acceleration: {useHardwareAcceleration}");
Console.WriteLine($"Encoding preset: {preset}");

// Example 2: Retrieving retry policy and timeouts
var (attempts, delayMs) = FFmpegOptionsExtensions.GetRetryConfiguration();
int timeoutMs = FFmpegOptionsExtensions.GetTimeoutMilliseconds();

Console.WriteLine($"Retry attempts: {attempts}, delay: {delayMs}ms");
Console.WriteLine($"Operation timeout: {timeoutMs}ms");
```

## Notes

- All methods are thread-safe and may be invoked concurrently without external synchronization.
- Path resolution methods (`GetEffectiveFFmpegPath`, `GetEffectiveFFprobePath`) do not validate file existence; consumers should validate paths as needed.
- The `GetSupportedFormatsString` method caches its result after the first call, improving performance for repeated invocations.
- Default values are applied only when the corresponding configuration setting is missing or invalid; no exceptions are thrown for missing settings.
- Retry configuration values are validated to ensure non-negative values, with defaults applied if validation fails.
- When `IsPathValidationEnabled()` returns `false`, consumers should not assume that FFmpeg or FFprobe paths are valid or accessible.
