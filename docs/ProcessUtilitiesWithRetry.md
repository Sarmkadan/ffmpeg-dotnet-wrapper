# ProcessUtilitiesWithRetry

## Purpose
Process execution utilities with retry policy support. Extends ProcessUtilities with retry logic for transient failures.

## Public API

| Method | Description | Parameters | Return Type |
|--------|-------------|------------|-------------|
| `ExecuteProcessWithRetryAsync(string fileName, string arguments, IRetryPolicy? retryPolicy = null, string? workingDirectory = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)` | Executes a process with retry policy support. | `fileName`: The executable to run.<br/>`arguments`: Command-line arguments.<br/>`retryPolicy`: Retry policy to use (null for no retry).<br/>`workingDirectory`: Working directory (optional).<br/>`timeout`: Process timeout (optional).<br/>`cancellationToken`: Cancellation token. | `Task<ProcessUtilities.ProcessResult>` |
| `ExecuteWithRetryAsync<T>(Func<CancellationToken, Task<T>> operation, IRetryPolicy? retryPolicy = null, CancellationToken cancellationToken = default)` | Executes a process with retry policy support and returns typed result. | `operation`: Process execution operation.<br/>`retryPolicy`: Retry policy to use (null for no retry).<br/>`cancellationToken`: Cancellation token. | `Task<T>` |
| `ExecuteWithRetryAsync(Func<CancellationToken, Task> operation, IRetryPolicy? retryPolicy = null, CancellationToken cancellationToken = default)` | Executes a process with retry policy support. | `operation`: Process execution operation.<br/>`retryPolicy`: Retry policy to use (null for no retry).<br/>`cancellationToken`: Cancellation token. | `Task` |

## Usage Example
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Policies;
using FFmpegDotnetWrapper.Utilities;

// Example retry policy (exponential backoff)
var retryPolicy = new ExponentialRetryPolicy(
    maxRetryCount: 3,
    delay: TimeSpan.FromSeconds(1),
    maxDelay: TimeSpan.FromSeconds(10),
    delta: TimeSpan.FromSeconds(2));

// Execute FFmpeg process with retry
var result = await ProcessUtilitiesWithRetry.ExecuteProcessWithRetryAsync(
    fileName: "ffmpeg",
    arguments: "-i input.mp4 output.avi",
    retryPolicy: retryPolicy,
    cancellationToken: CancellationToken.None);

if (result.ExitCode == 0)
{
    Console.WriteLine("FFmpeg executed successfully.");
}
else
{
    Console.WriteLine($"FFmpeg failed with exit code {result.ExitCode}");
}
```