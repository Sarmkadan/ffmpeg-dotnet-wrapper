# StreamingPipelineMetrics

Overview of the metrics collector for a streaming pipeline. This type aggregates events such as segment production, bitrate switches, and pipeline completion or failure, and provides queryable breakdowns and reports per profile.

## API

### RecordSegmentProduced
```csharp
public void RecordSegmentProduced()
```
Records that a media segment has been successfully produced.  
- **Parameters:** none.  
- **Return value:** none.  
- **Exceptions:** May throw `InvalidOperationException` if the collector has been reset or is in a terminal state (completed/failed). May throw `ObjectDisposedException` if the underlying resources have been released.

### RecordBitrateSwitch
```csharp
public void RecordBitrateSwitch()
```
Records a change in the selected bitrate during playback.  
- **Parameters:** none.  
- **Return value:** none.  
- **Exceptions:** Same as `RecordSegmentProduced`.

### RecordPipelineCompleted
```csharp
public void RecordPipelineCompleted()
```
Marks the pipeline as having finished successfully.  
- **Parameters:** none.  
- **Return value:** none.  
- **Exceptions:** May throw `InvalidOperationException` if called after the pipeline has already been marked completed or failed, or after a `Reset`. May throw `ObjectDisposedException` if the instance has been disposed.

### RecordPipelineFailed
```csharp
public void RecordPipelineFailed()
```
Marks the pipeline as having terminated with an error.  
- **Parameters:** none.  
- **Return value:** none.  
- **Exceptions:** Same as `RecordPipelineCompleted`.

### GetProfileBreakdown
```csharp
public IReadOnlyDictionary<string, ProfileMetrics> GetProfileBreakdown()
```
Retrieves a read‑only mapping of profile names to their respective metric objects.  
- **Parameters:** none.  
- **Return value:** A dictionary where each key is a profile identifier and each value contains detailed metrics for that profile. Returns an empty dictionary if no data has been recorded.  
- **Exceptions:** May throw `ObjectDisposedException` if the collector has been disposed.

### GetSummaryReport
```csharp
public string GetSummaryReport()
```
Produces a human‑readable summary of all recorded metrics.  
- **Parameters:** none.  
- **Return value:** A formatted string containing totals, averages, and per‑profile highlights. Returns an empty string when no data is available.  
- **Exceptions:** May throw `ObjectDisposedException` if the instance has been disposed.

### ExportProfilesAsCsv
```csharp
public string ExportProfilesAsCsv()
```
Exports the per‑profile metrics as a CSV‑formatted string.  
- **Parameters:** none.  
- **Return value:** A CSV string with a header row and one row per profile. Returns an empty string if no profile data exists.  
- **Exceptions:** May throw `ObjectDisposedException` if the collector has been disposed.

### Reset
```csharp
public void Reset()
```
Clears all accumulated metrics and restores the collector to its initial state.  
- **Parameters:** none.  
- **Return value:** none.  
- **Exceptions:** May throw `ObjectDisposedException` if the instance has been disposed.

### ProfileName
```csharp
public string ProfileName { get; }
```
Gets the name of the profile associated with the current metrics collection.  
- **Parameters:** none.  
- **Return value:** The profile name string; may be `null` or empty if no profile has been assigned.  
- **Exceptions:** None.

## Usage

### Example 1: Basic pipeline monitoring
```csharp
var metrics = new StreamingPipelineMetrics();
metrics.ProfileName = "high";

// Simulate pipeline activity
metrics.RecordSegmentProduced();
metrics.RecordBitrateSwitch();
metrics.RecordSegmentProduced();

// After successful completion
metrics.RecordPipelineCompleted();

string summary = metrics.GetSummaryReport();
Console.WriteLine(summary);

string csv = metrics.ExportProfilesAsCsv();
File.WriteAllText("profile_metrics.csv", csv);
```

### Example 2: Handling failure and resetting
```csharp
var metrics = new StreamingPipelineMetrics();
metrics.ProfileName = "low";

try
{
    // Process segments …
    metrics.RecordSegmentProduced();
    // … an error occurs …
    throw new IOException("Network failure");
}
catch (IOException)
{
    metrics.RecordPipelineFailed();
}
finally
{
    // Ensure metrics are cleared for next run
    metrics.Reset();
}
```

## Notes
- The collector is **not thread‑safe**. Concurrent calls to any of the recording methods or to `Reset` from multiple threads may result in lost or corrupted data. External synchronization (e.g., a `lock`) is required when shared across threads.
- After `RecordPipelineCompleted` or `RecordPipelineFailed` has been invoked, further recording methods (`RecordSegmentProduced`, `RecordBitrateSwitch`) should not be called; doing so will throw an `InvalidOperationException`.
- Calling `GetProfileBreakdown`, `GetSummaryReport`, or `ExportProfilesAsCsv` before any data has been recorded returns empty results rather than throwing.
- The `ProfileName` property reflects the profile identifier at the time of inspection; changing it after recording has begun will affect only subsequent breakdown entries, not previously recorded data.
- All methods may throw `ObjectDisposedException` if the instance has been disposed via a custom disposal pattern not shown in the public API. Users should avoid invoking members after disposal.
