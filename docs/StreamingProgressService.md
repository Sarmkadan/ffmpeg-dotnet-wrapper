# StreamingProgressService

## Purpose

`IStreamingProgressService` streams progress snapshots from a running FFmpeg process. `StreamingProgressService` reads the process's redirected standard-error output line by line, parses FFmpeg progress fields, and yields an `FFmpegProgressUpdate` for each recognized progress line.

Each call keeps its own timing and parsing state, so the singleton implementation can be used by concurrent operations. The supplied total duration is used to calculate a percentage and estimated remaining wall-clock time. If the duration is unknown, pass `TimeSpan.Zero`; the reported percentage and ETA remain zero.

## Public API

| Type | Member | Description |
|---|---|---|
| `IStreamingProgressService` | `IAsyncEnumerable<FFmpegProgressUpdate> StreamProgressAsync(string operationId, Process ffmpegProcess, TimeSpan totalDuration, CancellationToken cancellationToken = default)` | Reads redirected FFmpeg stderr until end of stream or cancellation and asynchronously yields parsed progress snapshots. |
| `StreamingProgressService` | `StreamingProgressService(ILogger<StreamingProgressService> logger)` | Creates the default implementation with its required logger. |
| `StreamingProgressService` | `StreamProgressAsync(string operationId, Process ffmpegProcess, TimeSpan totalDuration, CancellationToken cancellationToken = default)` | Implements the progress stream. The `operationId` is copied to every yielded update. |
| `StreamingProgressExtensions` | `IServiceCollection AddStreamingProgress(this IServiceCollection services)` | Registers `IStreamingProgressService` with `StreamingProgressService` as a singleton and returns the service collection. |
| `StreamingProgressExtensions` | `IServiceCollection AddFFmpegWrapperWithStreaming(this IServiceCollection services, Action<FFmpegWrapperOptions>? configureOptions = null)` | Registers the standard FFmpeg wrapper services and streaming progress service, optionally configuring `FFmpegWrapperOptions`. |

## Parsed FFmpeg stderr format

The parser targets FFmpeg status lines shaped like this:

```text
frame=  150 fps= 30 q=28.0 size=    1024kB time=00:00:05.00 bitrate=1677.7kbits/s speed=2.00x
```

A line must contain `time=` and its value must match `HH:mm:ss` with an optional fractional-seconds part. A missing, invalid, or zero time causes the line to be skipped. The other parsed fields are optional; a missing or invalid field is reported as zero.

| stderr token | `FFmpegProgressUpdate` property | Parsing behavior |
|---|---|---|
| `frame=<integer>` | `FramesProcessed` | Parsed as an integer. |
| `fps=<number>` | `FramesPerSecond` | Parsed as an invariant-culture number. |
| `size=<integer>kB` | `OutputSizeBytes` | Parsed in kB and multiplied by 1,024. |
| `time=<HH:mm:ss[.fraction]>` | `ProcessedDuration` | Required for an update to be emitted. |
| `bitrate=<number>kbits/s` | `BitrateKbps` | Parsed as an invariant-culture number. |
| `speed=<number>x` | `EncodingSpeed` | Parsed as an invariant-culture number. |

`ProgressPercentage` is the processed duration divided by `totalDuration`, clamped to 0–100. `EstimatedTimeRemaining` is based on media time processed per elapsed wall-clock second. Each update also contains the supplied operation ID and total duration, elapsed wall time, a UTC timestamp, and the original stderr line in `RawOutput`.

## Usage

The process must be started with standard error redirected. Consume the returned `IAsyncEnumerable<FFmpegProgressUpdate>` with `await foreach` while FFmpeg is running:

```csharp
using System.Diagnostics;
using FFmpegDotnetWrapper.Services;
using Microsoft.Extensions.DependencyInjection;

var startInfo = new ProcessStartInfo
{
    FileName = "ffmpeg",
    UseShellExecute = false,
    RedirectStandardError = true
};
startInfo.ArgumentList.Add("-i");
startInfo.ArgumentList.Add("input.mp4");
startInfo.ArgumentList.Add("output.mp4");

using var process = Process.Start(startInfo)
    ?? throw new InvalidOperationException("FFmpeg could not be started.");

IStreamingProgressService progressService = serviceProvider
    .GetRequiredService<IStreamingProgressService>();

await foreach (var update in progressService.StreamProgressAsync(
    operationId: "transcode-42",
    ffmpegProcess: process,
    totalDuration: TimeSpan.FromMinutes(5),
    cancellationToken: cancellationToken))
{
    Console.WriteLine($"{update.ProgressPercentage:F1}% ({update.EncodingSpeed:F2}x)");
}

await process.WaitForExitAsync(cancellationToken);
```

Register the service with `services.AddStreamingProgress()`, or use `services.AddFFmpegWrapperWithStreaming(options => { ... })` when the standard wrapper services are also needed.

## Exceptions

- `ArgumentNullException` is thrown by the `StreamingProgressService` constructor when `logger` is null.
- `ArgumentNullException` is thrown when `ffmpegProcess` is null.
- `ArgumentException` is thrown when `operationId` is null, empty, or whitespace.
- `ArgumentNullException` is thrown when the `services` receiver passed to either registration extension is null.
- `InvalidOperationException` can propagate from `Process.StandardError` if standard error was not redirected or the process configuration does not permit reading it.

Cancellation while awaiting the next stderr line ends the async stream without propagating the internally caught `OperationCanceledException`. Exceptions raised by process I/O, logging, or dependency registration are otherwise not caught by these APIs.
