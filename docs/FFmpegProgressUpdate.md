# FFmpegProgressUpdate

Represents a snapshot of encoding progress emitted by an FFmpeg process during media conversion. This type aggregates timing metrics, frame statistics, output size, and optional raw console output into a single structured object, enabling real-time monitoring of long-running operations.

## API

### `public string OperationId`
A unique identifier for the encoding operation that produced this update. This value correlates the progress event with a specific task in multi-operation workflows.

### `public double ProgressPercentage`
The current progress expressed as a percentage (0–100). Derived from the ratio of processed duration to total duration when the total duration is known; otherwise, it may be estimated from frame counts or remain at zero.

### `public TimeSpan ProcessedDuration`
The amount of media duration that has been processed so far, measured in wall-clock time of the source content. For audio-only streams, this reflects audio time; for video, it reflects the video timeline.

### `public TimeSpan TotalDuration`
The total duration of the input media, if detectable from the source file metadata. When the input duration is unknown (e.g., live streams or piped input), this property may be `TimeSpan.Zero`.

### `public TimeSpan EstimatedTimeRemaining`
An estimate of the time remaining until completion, calculated from the current processing speed and the unprocessed duration. This value is recalculated on each update and may fluctuate significantly during early stages of encoding.

### `public TimeSpan ElapsedWallTime`
The actual wall-clock time elapsed since the encoding operation started. This is measured independently of the source media timeline and reflects real system time consumed.

### `public int FramesProcessed`
The total number of video frames that have been processed so far. For audio-only operations, this value remains zero.

### `public double FramesPerSecond`
The average frame processing rate over the elapsed wall time, expressed in frames per second. This is a rolling average and stabilizes as the operation progresses.

### `public double EncodingSpeed`
The encoding speed relative to real-time playback, expressed as a multiplier. A value of `1.0` indicates real-time encoding; values greater than `1.0` indicate faster-than-real-time processing; values less than `1.0` indicate slower-than-real-time processing.

### `public long OutputSizeBytes`
The current size of the output file in bytes. This value increases as encoded data is written and may be zero during initialization or when output is streamed rather than written to disk.

### `public double BitrateKbps`
The effective output bitrate in kilobits per second, calculated from the output size and processed duration. This value becomes meaningful only after sufficient data has been written.

### `public DateTime Timestamp`
The UTC timestamp at which this progress update was captured. This reflects the moment the underlying FFmpeg output line was parsed, not the moment the event was consumed.

### `public string? RawOutput`
The raw, unparsed console output line from FFmpeg that produced this update, or `null` if the update was synthesized rather than parsed from output. This is useful for debugging or extracting additional metrics not covered by the structured properties.

### `public override string ToString()`
Returns a human-readable string summarizing the progress update, typically including the operation ID, progress percentage, and estimated time remaining. The exact format is implementation-defined and subject to change; do not rely on it for machine parsing.

## Usage

### Example 1: Reporting Progress to a Console UI

```csharp
async Task RunEncodingWithProgress(FFmpegWrapper wrapper, string inputPath, string outputPath)
{
    var operation = wrapper.CreateOperation(inputPath, outputPath);

    operation.ProgressUpdated += (sender, update) =>
    {
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(
            $"[{update.OperationId}] {update.ProgressPercentage:F1}% | " +
            $"Elapsed: {update.ElapsedWallTime:hh\\:mm\\:ss} | " +
            $"Remaining: {update.EstimatedTimeRemaining:hh\\:mm\\:ss} | " +
            $"Speed: {update.EncodingSpeed:F2}x | " +
            $"Size: {update.OutputSizeBytes / 1024.0 / 1024.0:F1} MB  ");
    };

    await operation.RunAsync();
    Console.WriteLine();
}
```

### Example 2: Aggregating Statistics Across Multiple Operations

```csharp
async Task<Dictionary<string, EncodingStats>> BatchEncode(
    FFmpegWrapper wrapper,
    IEnumerable<(string Id, string Input, string Output)> jobs)
{
    var stats = new Dictionary<string, EncodingStats>();
    var tasks = new List<Task>();

    foreach (var (id, input, output) in jobs)
    {
        var operation = wrapper.CreateOperation(input, output);
        stats[id] = new EncodingStats();

        operation.ProgressUpdated += (sender, update) =>
        {
            var s = stats[id];
            s.LastProgress = update.ProgressPercentage;
            s.TotalFrames = update.FramesProcessed;
            s.AverageSpeed = update.EncodingSpeed;
            s.FinalSizeBytes = update.OutputSizeBytes;
        };

        tasks.Add(operation.RunAsync());
    }

    await Task.WhenAll(tasks);
    return stats;
}

class EncodingStats
{
    public double LastProgress { get; set; }
    public int TotalFrames { get; set; }
    public double AverageSpeed { get; set; }
    public long FinalSizeBytes { get; set; }
}
```

## Notes

- **Duration availability**: `TotalDuration` and `ProgressPercentage` depend on the input file containing duration metadata. For live streams, piped input, or formats lacking index information, `TotalDuration` remains `TimeSpan.Zero` and `ProgressPercentage` may stay at zero throughout the operation. Consumers should handle this gracefully by falling back to frame-based or size-based progress indicators.

- **Fluctuating estimates**: `EstimatedTimeRemaining` and `EncodingSpeed` are computed from rolling averages. During the first few seconds of encoding, these values can swing dramatically. Avoid displaying them to end users until `ElapsedWallTime` exceeds a reasonable stabilization threshold (e.g., 5–10 seconds).

- **Thread safety**: `FFmpegProgressUpdate` is an immutable snapshot object. All properties are read-only after construction. Instances are typically delivered on event handler threads; consumers that update shared state from progress handlers must implement their own synchronization.

- **`RawOutput` nullability**: `RawOutput` is `null` when the update is synthesized internally (e.g., a final 100% completion update) rather than parsed from FFmpeg's stderr. Code that inspects `RawOutput` must perform null checks.

- **`ToString()` stability**: The output of `ToString()` is intended for diagnostic logging and developer inspection. Its format is not contractual and may vary across library versions. For structured logging, access the individual properties directly.
