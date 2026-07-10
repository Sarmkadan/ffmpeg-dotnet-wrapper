# ProgressReport

The `ProgressReport` class serves as a comprehensive state container and tracker for monitoring long-running operations within the `ffmpeg-dotnet-wrapper` library. It aggregates real-time metrics such as completion percentages, item counts, elapsed time, and throughput statistics, while providing mechanisms to update this state and notify subscribers of changes. Designed for integration with asynchronous workflows, it supports both discrete item tracking and byte-level progress reporting, offering formatted output strings for immediate UI consumption or logging.

## API

### Properties

*   **`public double ProgressPercentage`**
    Gets the current completion status of the operation as a percentage value between 0.0 and 100.0.

*   **`public int ItemsCompleted`**
    Gets the total number of discrete items successfully processed so far.

*   **`public int TotalItems`**
    Gets the total count of items expected to be processed in the current operation.

*   **`public TimeSpan ElapsedTime`**
    Gets the duration elapsed since the progress tracking began.

*   **`public TimeSpan EstimatedTimeRemaining`**
    Gets the calculated time remaining until completion, derived from current throughput and remaining work.

*   **`public string StatusMessage`**
    Gets or sets a human-readable string describing the current operational state or context.

*   **`public double ThroughputItemsPerSecond`**
    Gets the calculated rate of item processing, expressed as items completed per second.

*   **`public double ThroughputBytesPerSecond`**
    Gets the calculated data transfer or processing rate, expressed as bytes per second.

*   **`public ProgressTracker ProgressTracker`**
    Gets the underlying `ProgressTracker` instance responsible for managing the internal state calculations.

*   **`public ObservableProgressTracker ObservableProgressTracker`**
    Gets the `ObservableProgressTracker` instance which facilitates event-based notifications for progress changes.

### Methods

*   **`public void ReportItemProgress(int count = 1)`**
    Updates the internal state to reflect the completion of a specific number of items.
    *   **Parameters**: `count` (optional, defaults to 1) – The number of items to add to the completed count.
    *   **Returns**: `void`.
    *   **Throws**: May throw if the underlying tracker is in an invalid state or disposed.

*   **`public void ReportBytesProgress(long bytes)`**
    Updates the internal state to reflect the processing of a specific number of bytes.
    *   **Parameters**: `bytes` – The number of bytes processed in this update.
    *   **Returns**: `void`.
    *   **Throws**: May throw if the underlying tracker is in an invalid state or disposed.

*   **`public void ReportPercentageProgress(double percentage)`**
    Directly sets the progress percentage, bypassing item or byte calculations.
    *   **Parameters**: `percentage` – The completion percentage (0.0 to 100.0).
    *   **Returns**: `void`.
    *   **Throws**: May throw if `percentage` is outside the valid range or if the tracker is disposed.

*   **`public ProgressReport GetProgressReport()`**
    Returns a snapshot of the current progress state.
    *   **Parameters**: None.
    *   **Returns**: A `ProgressReport` instance containing current metric values.
    *   **Throws**: None.

*   **`public void Reset()`**
    Resets all counters, timers, and status messages to their initial default values.
    *   **Parameters**: None.
    *   **Returns**: `void`.
    *   **Throws**: None.

*   **`public string GetFormattedProgress()`**
    Generates a pre-formatted string representation of the current progress suitable for logging or display.
    *   **Parameters**: None.
    *   **Returns**: A formatted `string`.
    *   **Throws**: None.

*   **`public void Dispose()`**
    Releases unmanaged resources and stops internal timers associated with the progress tracker.
    *   **Parameters**: None.
    *   **Returns**: `void`.
    *   **Throws**: None.

*   **`public new void ReportItemProgress(int count = 1)`**
    Hides the base implementation and provides a specific override for reporting item progress, typically used when inheriting to enforce specific threading or validation logic.
    *   **Parameters**: `count` – The number of items completed.
    *   **Returns**: `void`.

*   **`public new void ReportBytesProgress(long bytes)`**
    Hides the base implementation and provides a specific override for reporting byte progress.
    *   **Parameters**: `bytes` – The number of bytes processed.
    *   **Returns**: `void`.

### Delegates

*   **`public delegate void ProgressChangedEventHandler(object sender, ProgressReport e)`**
    Defines the signature for event handlers that respond to progress updates.
    *   **Parameters**: `sender` (the source of the event), `e` (the progress report data).

## Usage

### Example 1: Tracking File Conversion Items
This example demonstrates initializing a report for a batch of file conversions, updating progress as each file completes, and retrieving a formatted status string.

```csharp
using FFmpegWrapper; // Hypothetical namespace

var filesToConvert = GetMediaFiles();
var report = new ProgressReport();
report.TotalItems = filesToConvert.Count;
report.StatusMessage = "Starting batch conversion...";

// Subscribe to changes if using the observable tracker
report.ObservableProgressTracker.ProgressChanged += (sender, args) => 
{
    Console.WriteLine($"Update: {args.GetFormattedProgress()}");
};

foreach (var file in filesToConvert)
{
    ConvertFile(file);
    // Report one completed item
    report.ReportItemProgress(1); 
}

// Final status
Console.WriteLine(report.GetFormattedProgress());
report.Dispose();
```

### Example 2: Monitoring Stream Byte Throughput
This example illustrates tracking a continuous data stream by reporting bytes processed, utilizing the throughput calculations to estimate completion time.

```csharp
using FFmpegWrapper;
using System;
using System.Threading;

var report = new ProgressReport();
report.StatusMessage = "Downloading stream...";
report.TotalItems = 1; // Treating the whole stream as one logical item

using (var stream = GetMediaStream())
{
    var buffer = new byte[8192];
    int bytesRead;

    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
    {
        ProcessBuffer(buffer, bytesRead);
        
        // Report raw byte progress
        report.ReportBytesProgress(bytesRead);
        
        // Access calculated metrics
        Console.WriteLine(
            $"Speed: {report.ThroughputBytesPerSecond:F2} B/s | " +
            $"ETA: {report.EstimatedTimeRemaining}"
        );
        
        Thread.Sleep(10); // Simulate work
    }
}

report.ReportPercentageProgress(100.0);
report.Dispose();
```

## Notes

*   **Thread Safety**: The presence of `ObservableProgressTracker` and the `ProgressChangedEventHandler` delegate suggests that this class is designed to raise events across thread boundaries (e.g., from a background worker to a UI thread). However, the `Report...` methods themselves should be considered thread-safe only if the underlying `ProgressTracker` implementation guarantees atomic updates to the shared state. External synchronization may be required if multiple threads call `ReportItemProgress` and `ReportBytesProgress` simultaneously without internal locking.
*   **Member Hiding**: The class defines `new` modifiers for `ReportItemProgress` and `ReportBytesProgress`. This indicates that `ProgressReport` likely inherits from a base class (possibly `ProgressTracker` itself or a generic reporter) and is intentionally hiding base implementations to provide specialized behavior or return types. Callers referencing the instance as its base type will invoke the base method, while callers referencing it as `ProgressReport` will invoke the hidden member.
*   **Disposal**: The implementation of `IDisposable` via the `Dispose()` method implies the management of unmanaged resources or, more likely, the termination of background timers used to calculate `ElapsedTime` and `Throughput`. It is critical to call `Dispose()` when the tracking session ends to prevent resource leaks or orphaned timer callbacks.
*   **Calculation Latency**: Properties like `EstimatedTimeRemaining` and `ThroughputItemsPerSecond` are derived values. Immediately after calling `Reset()` or upon the very first `Report...` call, these values may be zero or undefined until sufficient data points exist to calculate a stable rate.
