# BatchOperationService

The `BatchOperationService` coordinates batch processing of media files, providing asynchronous methods to transcode, analyze, or apply custom functions to a collection of inputs. It aggregates results and exposes progress‑ and completion‑state properties for monitoring the operation lifecycle.

## API

### Constructors

| Member | Description |
|--------|-------------|
| `BatchOperationService()` | Creates a new instance. The service is initialized with `CreatedAt` set to the current UTC time and all counters reset to zero. No parameters are required. |

### Methods

| Member | Description |
|--------|-------------|
| `public async Task<BatchOperationResult> TranscodeMultipleAsync(IEnumerable<string> sourceFiles, EncodingOptions options = null)` | Asynchronously transcodes each file in `sourceFiles` using the supplied `options`. Returns a `Task<BatchOperationResult>` that completes when all files have been processed or the operation is cancelled. Throws `ArgumentNullException` if `sourceFiles` is null, `ArgumentException` if the enumerable contains null or empty paths, and `OperationCanceledException` if the operation is cancelled via the internal cancellation token. |
| `public async Task<BatchAnalysisResult> AnalyzeMultipleAsync(IEnumerable<string> sourceFiles, AnalysisOptions options = null)` | Asynchronously analyzes each file in `sourceFiles` (e.g., probing streams, duration, codec info) according to `options`. Returns a `Task<BatchAnalysisResult>` when analysis finishes or is cancelled. Throws `ArgumentNullException` for a null source list, `ArgumentException` for invalid paths, and `OperationCanceledException` on cancellation. |
| `public async Task<BatchOperationResult> ProcessWithCustomFunctionAsync(IEnumerable<string> sourceFiles, Func<MediaFile, Task<ProcessingResult>> processor)` | Asynchronously invokes the user‑provided `processor` delegate for each file in `sourceFiles`. The delegate receives a `MediaFile` instance and must return a `Task<ProcessingResult>` indicating success or failure. Returns a `Task<BatchOperationResult>` aggregating the outcomes. Throws `ArgumentNullException` if `sourceFiles` or `processor` is null, `ArgumentException` for invalid file paths, and `OperationCanceledException` if cancellation is requested. |

### Properties

| Member | Description |
|--------|-------------|
| `public string OperationType { get; }` | Gets a string identifying the kind of batch operation performed (e.g., `"Transcode"`, `"Analysis"`, `"Custom"`). Set internally by the invoking method; remains constant for the lifetime of the instance. |
| `public int TotalFiles { get; }` | Gets the total number of files supplied to the batch operation. Updated when the operation starts; reflects the count of input items regardless of success or failure. |
| `public int SuccessfulCount { get; }` | Gets the number of files that completed without error. Incremented after each successful processing step. |
| `public int FailedCount { get; }` | Gets the number of files that encountered an error during processing. Incremented when a file’s result indicates failure. |
| `public bool IsCancelled { get; }` | Gets whether the batch operation has been cancelled. Returns `true` if cancellation was requested before completion; otherwise `false`. |
| `public DateTime CreatedAt { get; }` | Gets the UTC timestamp when the `BatchOperationService` instance was instantiated. |
| `public DateTime? CompletedAt { get; }` | Gets the UTC timestamp when the batch operation finished (successfully, with errors, or cancelled). Returns `null` while the operation is still running. |
| `public List<ConversionResult> Results { get; }` | Gets a read‑only list of `ConversionResult` objects, one per input file, detailing output paths, error messages, and processing metrics. Populated after each file is processed; empty until the operation begins. |
| `public TimeSpan GetDuration { get; }` | Gets the elapsed time of the batch operation. While running, returns the span from `CreatedAt` to the current time; after completion, returns the span from `CreatedAt` to `CompletedAt`. |
| `public double GetSuccessRate { get; }` | Gets the percentage of files processed successfully, calculated as `(SuccessfulCount / (double)TotalFiles) * 100`. Returns `0` when `TotalFiles` is zero. |
| `public List<MediaFile> AnalyzedFiles { get; }` | Gets a read‑only list of `MediaFile` objects representing the files that were analyzed during an `AnalyzeMultipleAsync` call. Populated only after analysis; empty for other operation types. |

## Usage

### Transcoding a batch of files

```csharp
using var service = new BatchOperationService();

var files = new[] { "input1.mp4", "input2.mkv", "input3.avi" };
var options = new EncodingOptions { VideoCodec = "libx264", AudioBitrate = 128 };

BatchOperationResult result = await service.TranscodeMultipleAsync(files, options);

Console.WriteLine($"Operation type: {service.OperationType}");
Console.WriteLine($"Total files: {service.TotalFiles}");
Console.WriteLine($"Successful: {service.SuccessfulCount}");
Console.WriteLine($"Failed: {service.FailedCount}");
Console.WriteLine($"Duration: {service.GetDuration}");
Console.WriteLine($"Success rate: {service.GetSuccessRate:F1}%");

foreach (var res in service.Results)
{
    Console.WriteLine($"{res.InputFile} -> {res.OutputFile} ({(res.Success ? "OK" : res.ErrorMessage)})");
}
```

### Analyzing media files and inspecting results

```csharp
var service = new BatchOperationService();

string[] media = { "song.wav", "video.mp4", "image.png" };
BatchAnalysisResult analysis = await service.AnalyzeMultipleAsync(media);

Console.WriteLine($"Analyzed {service.TotalFiles} files.");
Console.WriteLine($"Analysis took {service.GetDuration}.");

foreach (var mf in service.AnalyzedFiles)
{
    Console.WriteLine($"{mf.FilePath}: {mf.Duration}, {mf.VideoCodec}, {mf.AudioChannels}ch");
}
```

## Notes

- **Empty input** – If the source enumerable contains no items, the operation completes immediately with `TotalFiles` = 0, `SuccessfulCount` = 0, `FailedCount` = 0, and `GetSuccessRate` returns 0. No files are processed and `Results`/`AnalyzedFiles` remain empty.
- **Cancellation** – Cancellation is cooperative; invoking the internal cancellation token (exposed via the service’s internal mechanisms) sets `IsCancelled` to `true` and causes the asynchronous method to throw `OperationCanceledException`. Any already‑processed files retain their results in `Results` or `AnalyzedFiles`.
- **Thread‑safety** – The instance is **not** thread‑safe. Concurrent calls to any of the asynchronous methods on the same `BatchOperationService` instance may lead to corrupted state (e.g., duplicated counters, mismatched `Results`). For parallel batches, create separate service instances.
- **State mutation** – Properties such as `SuccessfulCount`, `FailedCount`, `IsCancelled`, `CompletedAt`, and the result lists are updated only by the executing operation. Reading them while an operation is in progress yields a snapshot of the current progress.
- **Exception propagation** – Exceptions thrown by user‑supplied delegates in `ProcessWithCustomFunctionAsync` are caught and recorded as failures for the corresponding file; they do not abort the whole batch unless cancellation is triggered. However, fatal exceptions (e.g., `OutOfMemoryException`) are propagated outward.
