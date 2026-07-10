# ServiceException

Represents an exception that occurs during interaction with a specific service (e.g., an external process or API) within the ffmpeg-dotnet-wrapper library. It extends `System.Exception` and provides additional context such as the name of the service that caused the failure, an optional exit code, and the error output from the service. This type is typically thrown when an ffmpeg operation fails, allowing callers to distinguish service-level errors from general application exceptions.

## API

### `public string? ServiceName`

Gets or sets the name of the service that generated the exception. The value may be `null` if no service name was provided at construction time. This property is intended to help identify which component or external tool (e.g., "ffmpeg", "ffprobe") caused the error.

### Constructors

All constructors initialize a new instance of `ServiceException` and delegate to the base `Exception` class. They do not throw additional exceptions beyond those inherent to object creation.

#### `public ServiceException(string message)`

- **Parameters**  
  `message` – A human-readable description of the error.
- **Remarks**  
  Creates an exception with no service name, exit code, or inner exception. The `ServiceName` property remains `null`.

#### `public ServiceException(string message, string serviceName)`

- **Parameters**  
  `message` – A human-readable description of the error.  
  `serviceName` – The name of the service that caused the exception.
- **Remarks**  
  Sets the `ServiceName` property to the provided value.

#### `public ServiceException(string message, Exception innerException)`

- **Parameters**  
  `message` – A human-readable description of the error.  
  `innerException` – The exception that is the cause of the current exception.
- **Remarks**  
  Wraps an inner exception without specifying a service name. `ServiceName` remains `null`.

#### `public ServiceException(string message, string serviceName, Exception innerException)`

- **Parameters**  
  `message` – A human-readable description of the error.  
  `serviceName` – The name of the service that caused the exception.  
  `innerException` – The exception that is the cause of the current exception.
- **Remarks**  
  Combines a service name with an inner exception for detailed error chaining.

#### `public ServiceException(string message, int exitCode, string errorOutput)`

- **Parameters**  
  `message` – A human-readable description of the error.  
  `exitCode` – The exit code returned by the service process.  
  `errorOutput` – The standard error output captured from the service.
- **Remarks**  
  Creates an exception with process-level failure details. The `ServiceName` property is not set by this constructor (remains `null`). The exit code and error output are stored in the base `Exception.Data` dictionary or can be accessed via custom properties if exposed; however, this constructor does not define dedicated public members for those values. They are intended for inclusion in the exception message or for logging.

## Usage

### Example 1: Catching a service exception and inspecting the service name

```csharp
using FFMpegWrapper; // hypothetical namespace

try
{
    // Perform an ffmpeg operation that may fail
    FFMpeg.Convert("input.mp4", "output.avi");
}
catch (ServiceException ex) when (ex.ServiceName == "ffmpeg")
{
    Console.WriteLine($"FFmpeg error: {ex.Message}");
    // Log or handle specifically for ffmpeg failures
}
catch (ServiceException ex)
{
    Console.WriteLine($"Service '{ex.ServiceName ?? "unknown"}' failed: {ex.Message}");
}
```

### Example 2: Throwing a ServiceException with exit code and error output

```csharp
using FFMpegWrapper;

public void RunProcess(string arguments)
{
    int exitCode = RunExternalProcess("ffmpeg", arguments, out string errorOutput);
    if (exitCode != 0)
    {
        throw new ServiceException(
            $"ffmpeg exited with code {exitCode}",
            exitCode,
            errorOutput);
    }
}
```

## Notes

- **Edge Cases**  
  - The `ServiceName` property can be `null` if the exception was created using a constructor that does not accept a service name. Code that reads this property should handle the `null` case gracefully.  
  - The `exitCode` and `errorOutput` parameters in the last constructor are not exposed as separate public properties; they are only used to construct the exception message. If you need to programmatically access these values, consider storing them in the `Data` dictionary or wrapping them in a custom derived class.  
  - Negative exit codes are technically allowed but are uncommon in practice; the library does not validate the range of `exitCode`.

- **Thread Safety**  
  Instances of `ServiceException` are immutable after construction if the `ServiceName` property is not modified. The property is read-write, so concurrent writes to the same instance are not thread-safe. In typical usage, the exception is thrown and then caught on the same thread, making thread-safety concerns negligible. If an exception object is shared across threads (e.g., stored in a static field), external synchronization is required.
