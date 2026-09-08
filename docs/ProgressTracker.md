# ProgressTracker and ObservableProgressTracker

`ProgressTracker` records the progress and timing of a long-running operation. It can track completed items, an absolute byte position, an explicit percentage, or processed media duration, and produces snapshot [`ProgressReport`](ProgressReport.md) objects containing percentage, elapsed time, ETA, status, and throughput values. Updates and report generation are protected by an internal lock.

`ObservableProgressTracker` derives from `ProgressTracker` and adds the `ProgressChanged` event. It raises the event after item, byte, or percentage updates when the percentage has changed by at least the configured reporting threshold, or whenever progress reaches 100 percent.

Both types are in the `FFmpegDotnetWrapper.Utilities` namespace.

## API

### ProgressTracker

| Member | Description |
| --- | --- |
| `ProgressTracker(int totalItems = 0, long totalBytes = 0)` | Starts a tracker and its elapsed-time stopwatch with optional item and byte totals. |
| `void ReportItemProgress(string? statusMessage = null)` | Increments the completed-item count by one and stores the status message. The current implementation throws `ArgumentException` when the message is null or empty. |
| `void ReportBytesProgress(long bytesProcessed, string? statusMessage = null)` | Sets the absolute number of processed bytes and stores the status message. The current implementation throws `ArgumentException` when the message is null or empty. |
| `void ReportPercentageProgress(double percentage, string? statusMessage = null)` | Clamps the value to 0–100 and, when an item total exists, derives the completed-item count from it. |
| `void ReportDurationProgress(TimeSpan processedDuration, TimeSpan totalDuration, string? statusMessage = null)` | Records processed and total durations. Duration determines percentage when no item or byte total takes precedence. |
| `ProgressReport GetProgressReport()` | Returns a snapshot containing percentage, counts, elapsed time, ETA, status, and item/byte throughput. See [`ProgressReport`](ProgressReport.md). |
| `void Reset(int totalItems = 0, long totalBytes = 0)` | Clears counters, duration, and status; replaces the totals; and restarts elapsed timing. |
| `string GetFormattedProgress()` | Returns display text such as `45% (90/200 items) - ETA: 2m 15s`. |
| `double PercentComplete { get; }` | Gets the calculated percentage, clamped to 0–100. |
| `void Dispose()` | Stops the elapsed-time stopwatch. |

Progress calculation gives priority to an item total, then a byte total, then a duration total. Callers should use one primary tracking mode for an operation. In the current implementation, byte-based percentage calculation does not use the stored byte count; use item, percentage, or duration tracking when a reliable completion percentage is required.

### ObservableProgressTracker

| Member | Description |
| --- | --- |
| `ObservableProgressTracker(int totalItems = 0, long totalBytes = 0, double reportingThreshold = 1.0)` | Creates a tracker with the minimum percentage-point change required to notify subscribers. |
| `event ProgressChangedEventHandler? ProgressChanged` | Publishes a `ProgressReport` snapshot when the threshold is met or progress is at least 100 percent. The delegate signature is `void ProgressChangedEventHandler(ProgressReport report)`. |
| `new void ReportItemProgress(string? statusMessage = null)` | Reports one completed item through the base tracker, then evaluates whether to raise `ProgressChanged`. |
| `new void ReportBytesProgress(long bytesProcessed, string? statusMessage = null)` | Reports the absolute byte position, then evaluates whether to raise `ProgressChanged`. |
| `new void ReportPercentageProgress(double percentage, string? statusMessage = null)` | Reports explicit percentage progress, then evaluates whether to raise `ProgressChanged`. |

The observable reporting methods hide, rather than override, the base methods. Use an `ObservableProgressTracker` reference when event notification is required. `ReportDurationProgress` is inherited as-is and does not raise `ProgressChanged`.

## Example

```csharp
using FFmpegDotnetWrapper.Utilities;

using var progress = new ObservableProgressTracker(
    totalItems: 4,
    reportingThreshold: 25.0);

progress.ProgressChanged += report =>
{
    Console.WriteLine(
        $"{report.ProgressPercentage:F0}%: {report.StatusMessage} " +
        $"({report.ItemsCompleted}/{report.TotalItems})");
};

for (var item = 1; item <= 4; item++)
{
    // Perform one unit of work here.
    progress.ReportItemProgress($"Completed item {item}");
}

ProgressReport finalReport = progress.GetProgressReport();
Console.WriteLine(progress.GetFormattedProgress());
```

With four total items, each update advances progress by 25 percentage points, so the example's event handler runs after every item.
