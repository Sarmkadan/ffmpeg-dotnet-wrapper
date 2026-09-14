# FakeFFmpegProcessRunner Documentation

## Purpose
The `FakeFFmpegProcessRunner` is an in-memory test double for `IFFmpegProcessRunner` that simulates the execution of ffmpeg processes without requiring the actual ffmpeg binary. It is designed to facilitate unit testing of services that depend on `IFFmpegProcessRunner` by allowing the configuration of:
- The result to return (exit code, stderr, execution time, etc.)
- Progress updates to report (when progress parsing is enabled)
- A callback to inspect or modify the behavior per request.

## API Reference

| Type | Description |
|------|-------------|
| `FakeFFmpegProcessRunner` | Implementation of `IFFmpegProcessRunner` for testing. |

### Constructors
| Constructor | Description |
|-------------|-------------|
| `new FakeFFmpegProcessRunner()` | Creates a new instance with default behavior (successful, zero-duration result). |

### Properties
| Property | Type | Description |
|----------|------|-------------|
| `Requests` | `IReadOnlyList<FFmpegProcessRequest>` | List of all requests received, in order. |
| `ResultToReturn` | `FFmpegProcessResult` | The result to return from `RunAsync`. Defaults to a successful result. |
| `ProgressUpdatesToReport` | `IReadOnlyList<FFmpegProgressUpdate>` | Progress updates to report when the request has `ParseProgressFromStdOut` set to true. |
| `OnRun` | `Action<FFmpegProcessRequest>?` | Callback invoked for each request before returning the result. |

### Methods
| Method | Description |
|--------|-------------|
| `RunAsync(FFmpegProcessRequest request, IProgress<FFmpegProgressUpdate>? progress, CancellationToken cancellationToken = default)` | Records the request, invokes `OnRun`, reports progress updates (if applicable), and returns `ResultToReturn`. Throws `OperationCanceledException` if the cancellation token is already signaled. |

## Usage Example

### Wiring into FFmpegService for Unit Tests

```csharp
// Arrange
var fakeRunner = new FakeFFmpegProcessRunner();
// Configure the fake runner to return a specific result
fakeRunner.ResultToReturn = new FFmpegProcessResult
{
    ExitCode = 0,
    StdErrTail = "Successfully processed",
    ExecutionTime = TimeSpan.FromSeconds(5)
};

// Optionally, configure progress updates
fakeRunner.ProgressUpdatesToReport = new[]
{
    new FFmpegProgressUpdate { Timemark = TimeSpan.FromSeconds(1), ProgressPercent = 20 },
    new FFmpegProgressUpdate { Timemark = TimeSpan.FromSeconds(2), ProgressPercent = 40 }
};

// Create the service under test with the fake runner
var service = new FFmpegService(fakeRunner);

// Act
var result = await service.ProcessMediaAsync(/* parameters */);

// Assert
// Verify that the service made the expected request
Assert.AreEqual(1, fakeRunner.Requests.Count);
Assert.AreEqual("-i input.mp4 -output output.mkv", fakeRunner.Requests[0].Arguments);
// Verify that the service used the result from the fake runner
Assert.IsTrue(result.Success);
```