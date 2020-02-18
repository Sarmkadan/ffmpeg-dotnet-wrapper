# FFmpegException

Exception thrown when an error occurs during FFmpeg operations. This type serves as the base for more specific FFmpeg-related exceptions and provides contextual error details such as exit codes, error output, and file paths.

## API

### `public int? ExitCode`
Gets the exit code returned by the FFmpeg process, if available. This value is `null` when the process did not terminate with an exit code or when the exception was not generated from a process failure.

### `public string? ErrorOutput`
Gets the raw error output from the FFmpeg process, if available. This string may contain diagnostic information or error messages produced by FFmpeg. The value is `null` when no error output was captured.

### `public FFmpegException(string message) : base(message)`
Constructs a new `FFmpegException` with the specified error message. This constructor initializes the exception with user-provided context about the failure.

### `public FFmpegException()`
Constructs a new `FFmpegException` with a default message. This parameterless constructor is useful when the cause of the error is not immediately known or when the message can be set later.

### `public string? FilePath`
Gets the file path associated with the operation that failed, if applicable. This value is `null` when the exception is unrelated to a specific file (e.g., configuration errors).

### `public TimeSpan? Timeout`
Gets the timeout duration applied to the operation, if applicable. This value is `null` when no timeout was enforced or when the exception was not caused by a timeout.

### `public InvalidMediaFileException(string message) : base(message)`
Constructs a new `InvalidMediaFileException` with the specified error message. This exception indicates that the media file provided is invalid or unsupported by FFmpeg.

### `public InvalidMediaFileException()`
Constructs a new `InvalidMediaFileException` with a default message. This parameterless constructor is useful when the invalidity of the file is detected without additional context.

### `public FFmpegProcessException(string message) : base(message)`
Constructs a new `FFmpegProcessException` with the specified error message. This exception indicates that the FFmpeg process failed to execute or terminated abnormally.

### `public FFmpegProcessException()`
Constructs a new `FFmpegProcessException` with a default message. This parameterless constructor is useful when the process failure is detected without additional context.

### `public string? ConfigurationKey`
Gets the configuration key associated with the operation that failed, if applicable. This value is `null` when the exception is unrelated to a specific configuration setting.

### `public InvalidOperationConfigurationException(string message) : base(message)`
Constructs a new `InvalidOperationConfigurationException` with the specified error message. This exception indicates that the requested operation cannot proceed due to invalid or missing configuration.

### `public InvalidOperationConfigurationException()`
Constructs a new `InvalidOperationConfigurationException` with a default message. This parameterless constructor is useful when the configuration issue is detected without additional context.

### `public UnsupportedOperationException(string message) : base(message)`
Constructs a new `UnsupportedOperationException` with the specified error message. This exception indicates that the requested operation is not supported by the current FFmpeg version or build.

## Usage
