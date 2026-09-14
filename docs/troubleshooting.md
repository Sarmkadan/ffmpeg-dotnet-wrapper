# Troubleshooting Guide

Common issues and solutions when using FFmpeg .NET Wrapper.

## FFmpeg Not Found

### Issue: FFmpeg is not installed or not available

**Symptoms**: 
- `FFmpegNotInstalled` error (exit code 1007)
- `ProcessExecutionException` with message indicating FFmpeg not found
- Service reports `IsFFmpegAvailableAsync()` returns false

**Solutions**:

1. **Install FFmpeg**:
   ```bash
   # macOS
   brew install ffmpeg

   # Linux (Ubuntu/Debian)
   sudo apt-get install ffmpeg

   # Linux (CentOS/RHEL/Fedora)
   sudo yum install ffmpeg
   # or
   sudo dnf install ffmpeg

   # Windows (Chocolatey)
   choco install ffmpeg
   ```

2. **Verify Installation**:
   ```bash
   ffmpeg -version
   ```

3. **Configure FFmpeg Path** (if not in PATH):
   ```csharp
   services.AddFFmpegWrapper(options =>
   {
       options.FFmpegPath = "/usr/local/bin/ffmpeg"; // Linux/macOS
       // options.FFmpegPath = @"C:\ffmpeg\bin\ffmpeg.exe"; // Windows
   });
   ```

4. **Configure FFprobe Path** (if needed):
   ```csharp
   services.AddFFmpegWrapper(options =>
   {
       options.FFprobePath = "/usr/local/bin/ffprobe"; // Linux/macOS
       // options.FFprobePath = @"C:\ffmpeg\bin\ffprobe.exe"; // Windows
   });
   ```

## ProcessExecutionException

### Issue: Process execution fails with exit codes

**Symptoms**: 
- `ProcessExecutionException` thrown with exit code and error output
- Operation fails unexpectedly during FFmpeg execution

**Common Exit Codes**:

| Exit Code | Meaning | Solution |
|-----------|---------|----------|
| 0 | Success | No action needed |
| 1 | Generic error | Check FFmpeg command and arguments |
| 2 | Generic error | Check FFmpeg command and arguments |
| 126 | Cannot execute | Check file permissions on FFmpeg binary |
| 127 | Command not found | Verify FFmpeg is installed and in PATH |
| 130 | Terminated by signal (Ctrl+C) | Operation was cancelled |
| 255 | FFmpeg error | Check error output for specific FFmpeg issue |

**Handling ProcessExecutionException**:

```csharp
try
{
    await ffmpeg.TranscodeAsync(input, output, settings);
}
catch (ProcessExecutionException ex)
{
    // Log detailed information
    _logger.LogError(ex, "FFmpeg process failed with exit code {ExitCode}", ex.ExitCode);
    _logger.LogError("Error output: {ErrorOutput}", ex.ErrorOutput);
    
    // Handle specific exit codes
    switch (ex.ExitCode)
    {
        case 127: // Command not found
            throw new InvalidOperationException(
                "FFmpeg not found. Please install FFmpeg and ensure it's in your PATH.", ex);
        case 126: // Cannot execute
            throw new InvalidOperationException(
                "FFmpeg binary cannot be executed. Check file permissions.", ex);
        default:
            // Re-throw or handle based on your application logic
            throw;
    }
}
```

## Timeouts

### Issue: Operations timeout unexpectedly

**Symptoms**:
- `TimeoutException` or `OperationCanceledException`
- Operation takes longer than expected
- FFmpeg process killed due to timeout

**Configuration**:

Timeouts are configured in `FFmpegOptions` and `TimeoutConstants`:

```csharp
// Configure via code
services.AddFFmpegWrapper(options =>
{
    options.OperationTimeoutSeconds = 1800; // 30 minutes
});

// Configure via appsettings.json
{
  "FFmpeg": {
    "OperationTimeoutSeconds": 1800
  }
}
```

**Timeout Constants** (from `src/Constants/OperationConstants.cs`):
- `DefaultOperationTimeoutSeconds` = 600 (10 minutes)
- `MaxOperationTimeoutSeconds` = 3600 (1 hour)
- `MinOperationTimeoutSeconds` = 10 (10 seconds)
- `ProbeTimeoutSeconds` = 30 (media analysis)
- `WebhookTimeoutSeconds` = 30 (webhook delivery)
- `HttpClientTimeoutSeconds` = 60 (HTTP calls)

**Rule of Thumb**: Allow approximately 1 minute per GB of video file for transcoding operations.

**Handling Timeouts**:

```csharp
try
{
    await ffmpeg.TranscodeAsync(input, output, settings);
}
catch (OperationCanceledException) when (/* timeout cancellation */)
{
    // Handle timeout specifically
    _logger.LogWarning("Operation timed out after {Timeout} seconds", 
        ffmpegOptions.OperationTimeoutSeconds);
    
    // Options: retry with longer timeout, notify user, etc.
    throw new TimeoutException(
        $"Operation timed out after {ffmpegOptions.OperationTimeoutSeconds} seconds. " +
        "Consider increasing timeout or reducing file size/complexity.");
}
```

## Rate Limiting (429 Responses)

### Issue: Rate limit exceeded errors

**Symptoms**:
- HTTP 429 Too Many Requests responses
- `RateLimitExceeded` error (exit code 2000 from `ErrorCode` enum)
- Service rejects requests due to rate limiting

**Configuration**:

Rate limiting is configured in `FFmpegOptions.RateLimitingOptions`:

```csharp
services.AddFFmpegWrapper(options =>
{
    options.RateLimiting.Enabled = true;
    options.RateLimiting.TranscodeOperationsPerHour = 10; // Increased from default 5
    options.RateLimiting.WatermarkOperationsPerHour = 30;
    options.RateLimiting.MergeOperationsPerHour = 15;
    options.RateLimiting.WindowSeconds = 3600; // 1 hour window
    options.RateLimiting.PerUserLimiting = true;
});
```

**Handling Rate Limiting**:

The `RateLimitingMiddleware` automatically handles rate limiting and returns appropriate HTTP responses when used in ASP.NET Core applications.

For direct service usage, check rate limit status before operations:

```csharp
var rateLimiter = serviceProvider.GetRequiredService<IRateLimiter>();

if (!rateLimiter.AllowRequest(userId: "user123", policyName: "transcode"))
{
    var status = rateLimiter.GetStatus("user123", "transcode");
    throw new InvalidOperationException(
        $"Rate limit exceeded. Try again in {status.SecondsUntilReset:F0} seconds. " +
        $"Made {status.RequestsMade}/{status.MaxRequests} requests.");
}

// Proceed with operation
await ffmpeg.TranscodeAsync(input, output, settings);
```

## Circuit Breaker Open

### Issue: Circuit breaker prevents operations

**Symptoms**:
- `CircuitBreakerOpenException` thrown
- Service temporarily stops processing operations
- Failures are blocked to prevent cascading failures

**Configuration**:

Circuit breaker is applied via `CircuitBreakerRetryPolicy`:

```csharp
services.AddFFmpegWrapper(options =>
{
    // Circuit breaker settings are configured when registering policies
    // Default: 5 failures before opening, 30 second break duration
});
```

**Handling Circuit Breaker Open**:

```csharp
try
{
    await ffmpeg.TranscodeAsync(input, output, settings);
}
catch (CircuitBreakerOpenException ex)
{
    // Log the issue
    _logger.LogWarning(ex, "Circuit breaker is open");
    
    // Inform user/service
    throw new ServiceUnavailableException(
        "Service temporarily unavailable due to repeated failures. " +
        $"Please try again later. {ex.Message}");
}
```

**Manual Reset** (for administrative purposes):

```csharp
var circuitBreakerPolicy = serviceProvider.GetRequiredService<CircuitBreakerRetryPolicy>();
circuitBreakerPolicy.Reset(); // Resets to closed state
```

## Webhook Retries

### Issue: Webhook delivery fails repeatedly

**Symptoms**:
- `RetryFailedException` thrown after webhook delivery attempts
- Webhook notifications not received
- Delivery attempts exhausted

**Configuration**:

Webhook retry settings are in `FFmpegOptions.WebhookOptions`:

```csharp
services.AddFFmpegWrapper(options =>
{
    options.Webhooks.Enabled = true;
    options.Webhooks.MaxRetries = 5; // Increased from default 3
    options.Webhooks.TimeoutSeconds = 60; // Increased from default 30
});
```

**Handling Webhook Delivery Failures**:

```csharp
try
{
    await webhookService.RegisterWebhookAsync(
        url: "https://example.com/webhook",
        events: WebhookEvent.OperationCompleted);
}
catch (RetryFailedException ex)
{
    // Log delivery failure
    _logger.LogError(ex, "Webhook delivery failed after all retry attempts");
    
    // Options: alert administrators, store for manual retry, etc.
    await _notificationService.NotifyAdminAsync(
        "Webhook delivery failure", 
        $"Failed to deliver webhook after {ex.Attempts} attempts: {ex.Message}");
}
```

**Webhook Retry Logic**:
- Uses `ExponentialBackoffRetryPolicy` with default settings
- Retries on transient failures (network issues, timeouts, 5xx responses)
- Does not retry on permanent failures (4xx responses except 429, validation errors)
- Delay increases exponentially with jitter to prevent thundering herd

## Using FakeFFmpegProcessRunner to Isolate Issues

### Issue: Need to test service behavior without FFmpeg binary

**Symptoms**:
- Want to unit test FFmpeg service interactions
- Need to simulate various FFmpeg outputs/exit codes
- Developing on machine without FFmpeg installed

**Solution**: Use `FakeFFmpegProcessRunner` to mock FFmpeg process execution.

**Basic Usage**:

```csharp
// Arrange
var fakeRunner = new FakeFFmpegProcessRunner();
// Configure to return specific result
fakeRunner.ResultToReturn = new FFmpegProcessResult
{
    ExitCode = 0,
    StdOut = "frame=  100 fps=0.0 q=-1.0 Lsize=    1024kB time=00:00:04.00 bitrate=2048.0kbits/s speed=4x",
    StdErrTail = "",
    ExecutionTime = TimeSpan.FromSeconds(2)
};

// Configure service to use fake runner
var services = new ServiceCollection();
services.AddSingleton<IFFmpegProcessRunner>(fakeRunner);
services.AddFFmpegWrapper();
var provider = services.BuildServiceProvider();
var ffmpeg = provider.GetRequiredService<IFFmpegService>();

// Act
var result = await ffmpeg.TranscodeAsync("input.mp4", "output.mp4", new TranscodeSettings());

// Assert
Assert.Single(fakeRunner.Requests); // Verify request was made
Assert.Equal(0, result.ExitCode);   // Verify result mapping
```

**Simulating Failures**:

```csharp
// Simulate FFmpeg not found (exit code 127)
fakeRunner.ResultToReturn = new FFmpegProcessResult
{
    ExitCode = 127,
    StdErrTail = "ffmpeg: command not found",
    ExecutionTime = TimeSpan.FromSeconds(0)
};

// Simulate timeout
fakeRunner.OnRun = request =>
{
    // Simulate long-running process that times out
    Thread.Sleep(Timeout.Infinite); // This will be cancelled by timeout token
};

// Simulate specific FFmpeg error
fakeRunner.ResultToReturn = new FFmpegProcessResult
{
    ExitCode = 1,
    StdErrTail = "Unknown encoder 'libx265'",
    ExecutionTime = TimeSpan.FromSeconds(1)
};
```

**Testing Progress Reporting**:

```csharp
// Configure progress updates
fakeRunner.ProgressUpdatesToReport = new[]
{
    new FFmpegProgressUpdate { Timestamp = TimeSpan.FromSeconds(1), Percentage = 25.0 },
    new FFmpegProgressUpdate { Timestamp = TimeSpan.FromSeconds(2), Percentage = 50.0 },
    new FFmpegProgressUpdate { Timestamp = TimeSpan.FromSeconds(3), Percentage = 75.0 },
    new FFmpegProgressUpdate { Timestamp = TimeSpan.FromSeconds(4), Percentage = 100.0 }
};

// Enable progress parsing in request
var request = new FFmpegProcessRequest
{
    Arguments = "-i input.mp4 output.mp4",
    ParseProgressFromStdOut = true
};

// The fake runner will report progress updates through IProgress<T>
```

**Benefits of FakeFFmpegProcessRunner**:
- No FFmpeg binary required for testing
- Deterministic test results
- Ability to simulate any exit code or output
- Test progress reporting and cancellation
- Verify argument construction and service behavior
- Isolate service-level issues from FFmpeg-specific problems

## General Troubleshooting Tips

### Enable Detailed Logging

```csharp
services.AddLogging(builder =>
    builder.SetMinimumLevel(LogLevel.Debug)
           .AddConsole());

services.AddFFmpegWrapper(options =>
{
    options.EnableDetailedLogging = true;
    options.VerboseLogging = true;
});
```

### Check Service Health

```csharp
var ffmpeg = serviceProvider.GetRequiredService<IFFmpegService>();
bool isAvailable = await ffmpeg.IsFFmpegAvailableAsync();

if (!isAvailable)
{
    _logger.LogError("FFmpeg service is not available");
    // Handle unavailable service
}
```

### Examine FFmpeg Command

With detailed logging enabled, you can see the actual FFmpeg command being executed:

```
[Debug] FFmpeg command: ffmpeg -i input.mp4 -c:v libx264 -crf 23 output.mp4
```

### Common Issues Checklist

1. [ ] FFmpeg installed and in PATH (or FFmpegPath configured)
2. [ ] Sufficient disk space for temporary files
3. [ ] Write permissions to output directory
4. [ ] Correct file paths and names
5. [ ] Supported codecs for your FFmpeg build
6. [ ] Adequate timeout for file size/complexity
7. [ ] Rate limiting not blocking requests
8. [ ] Circuit breaker not open due to repeated failures
9. [ ] Webhook endpoints accessible and returning 2xx responses
10. [ ] Temporary directory accessible and writable

### Getting Help

If issues persist:
1. Check the [FAQ](faq.md) for common questions
2. Review the [API documentation](../api-reference.md)
3. Search existing issues: https://github.com/vladyslav-zaiets/ffmpeg-dotnet-wrapper/issues
4. Create a new issue with:
   - FFmpeg .NET Wrapper version
   - FFmpeg version (`ffmpeg -version`)
   - Operating system and .NET version
   - Detailed steps to reproduce
   - Full exception details including stack trace
   - Relevant code snippets
   - Logs with detailed logging enabled