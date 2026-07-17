# FFmpegServiceBenchmarksExtensions

Provides a comprehensive benchmarking framework for FFmpeg transcoding operations, enabling batch creation, validation, statistical analysis, and comparative evaluation of benchmark runs. This type encapsulates both the orchestration of benchmark batches and the detailed comparison metrics between individual benchmark results and their baselines.

## API

### Static Methods

#### CreateBenchmarkBatch
```csharp
public static IReadOnlyList<BenchmarkResult> CreateBenchmarkBatch(
    int iteration,
    string outputPath,
    VideoCodec videoCodec,
    AudioCodec audioCodec)
```
Creates and executes a batch of benchmark runs with the specified encoding parameters. Each run in the batch uses the same codec configuration but produces independent timing and resource measurements.

- **Parameters**:
  - `iteration`: The number of benchmark runs to execute in the batch.
  - `outputPath`: The file system path where transcoded output files will be written.
  - `videoCodec`: The video codec to use for all runs in the batch.
  - `audioCodec`: The audio codec to use for all runs in the batch.
- **Returns**: A read-only list of `BenchmarkResult` instances, one per iteration, each containing timing and resource usage data.
- **Throws**:
  - `ArgumentNullException` when `outputPath` is null.
  - `ArgumentException` when `iteration` is less than 1.
  - `DirectoryNotFoundException` when the directory portion of `outputPath` does not exist.
  - `InvalidOperationException` when FFmpeg is not available or the codec combination is unsupported.

#### ValidateAllBenchmarksAsync
```csharp
public static async Task<bool> ValidateAllBenchmarksAsync(
    IReadOnlyList<BenchmarkResult> results)
```
Asynchronously validates a collection of benchmark results to ensure all runs completed successfully and produced measurable output.

- **Parameters**:
  - `results`: The collection of benchmark results to validate.
- **Returns**: `true` if all benchmark results pass validation; `false` if any result indicates failure or missing output.
- **Throws**:
  - `ArgumentNullException` when `results` is null.
  - `AggregateException` wrapping individual validation failures when multiple results fail concurrently.

#### GetBenchmarkStatistics
```csharp
public static IReadOnlyDictionary<string, object> GetBenchmarkStatistics(
    IReadOnlyList<BenchmarkResult> results)
```
Computes aggregate statistics across a batch of benchmark results, including mean, median, standard deviation, and percentile values for timing metrics.

- **Parameters**:
  - `results`: The collection of benchmark results to analyze.
- **Returns**: A read-only dictionary mapping statistic names (e.g., "MeanDuration", "P95Duration") to their computed values.
- **Throws**:
  - `ArgumentNullException` when `results` is null.
  - `ArgumentException` when `results` is empty.

#### CompareBenchmarks
```csharp
public static BenchmarkComparison CompareBenchmarks(
    IReadOnlyList<BenchmarkResult> current,
    IReadOnlyList<BenchmarkResult> baseline)
```
Performs a detailed comparison between two benchmark batches, identifying regressions and improvements across all measured metrics.

- **Parameters**:
  - `current`: The current benchmark batch to evaluate.
  - `baseline`: The reference benchmark batch to compare against.
- **Returns**: A `BenchmarkComparison` instance containing per-metric comparisons and overall regression/improvement flags.
- **Throws**:
  - `ArgumentNullException` when either parameter is null.
  - `ArgumentException` when the two batches have mismatched iteration counts or codec configurations.

### Instance Properties (BenchmarkComparison)

#### Iteration
```csharp
public int Iteration { get; }
```
The iteration index within the current benchmark batch that this comparison entry corresponds to.

#### OutputPath
```csharp
public string OutputPath { get; }
```
The file system path where the transcoded output for this benchmark iteration was written.

#### VideoCodec
```csharp
public VideoCodec VideoCodec { get; }
```
The video codec used during this benchmark iteration.

#### AudioCodec
```csharp
public AudioCodec AudioCodec { get; }
```
The audio codec used during this benchmark iteration.

#### Timestamp
```csharp
public DateTime Timestamp { get; }
```
The UTC timestamp when this benchmark iteration was executed.

#### Metrics
```csharp
public IReadOnlyList<BenchmarkMetricComparison> Metrics { get; }
```
The collection of per-metric comparison results between the current and baseline values for this iteration.

### Instance Properties (BenchmarkMetricComparison)

#### MetricName
```csharp
public string MetricName { get; }
```
The name of the measured metric (e.g., "TranscodeDurationMs", "PeakMemoryBytes").

#### CurrentValue
```csharp
public double CurrentValue { get; }
```
The measured value from the current benchmark run.

#### BaselineValue
```csharp
public double BaselineValue { get; }
```
The measured value from the corresponding baseline benchmark run.

#### Difference
```csharp
public double Difference { get; }
```
The absolute difference between the current and baseline values (`CurrentValue - BaselineValue`).

#### PercentageChange
```csharp
public double PercentageChange { get; }
```
The relative change expressed as a percentage (`(Difference / BaselineValue) * 100`).

#### IsRegression
```csharp
public bool IsRegression { get; }
```
`true` when the metric has degraded beyond a predefined threshold relative to the baseline; `false` otherwise.

#### IsImprovement
```csharp
public bool IsImprovement { get; }
```
`true` when the metric has improved beyond a predefined threshold relative to the baseline; `false` otherwise. A metric cannot simultaneously be both a regression and an improvement.

## Usage

### Example 1: Creating and Validating a Benchmark Batch

```csharp
// Define benchmark parameters
int iterations = 5;
string outputDir = @"C:\Benchmarks\Output";
VideoCodec videoCodec = VideoCodec.H264;
AudioCodec audioCodec = AudioCodec.AAC;

// Create the benchmark batch
IReadOnlyList<BenchmarkResult> results = FFmpegServiceBenchmarksExtensions.CreateBenchmarkBatch(
    iterations, outputDir, videoCodec, audioCodec);

// Validate all results asynchronously
bool allValid = await FFmpegServiceBenchmarksExtensions.ValidateAllBenchmarksAsync(results);

if (!allValid)
{
    Console.WriteLine("One or more benchmark runs failed validation.");
    return;
}

// Compute and display statistics
IReadOnlyDictionary<string, object> stats = FFmpegServiceBenchmarksExtensions.GetBenchmarkStatistics(results);
foreach (var kvp in stats)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
```

### Example 2: Comparing Current Benchmarks Against a Baseline

```csharp
// Load previously saved baseline results
IReadOnlyList<BenchmarkResult> baselineResults = LoadBaselineFromStorage();

// Run current benchmarks with identical parameters
IReadOnlyList<BenchmarkResult> currentResults = FFmpegServiceBenchmarksExtensions.CreateBenchmarkBatch(
    5, @"C:\Benchmarks\Current", VideoCodec.H264, AudioCodec.AAC);

// Perform comparison
BenchmarkComparison comparison = FFmpegServiceBenchmarksExtensions.CompareBenchmarks(
    currentResults, baselineResults);

// Inspect per-metric comparisons
foreach (var metricComparison in comparison.Metrics)
{
    Console.WriteLine($"Metric: {metricComparison.MetricName}");
    Console.WriteLine($"  Current:  {metricComparison.CurrentValue:F2}");
    Console.WriteLine($"  Baseline: {metricComparison.BaselineValue:F2}");
    Console.WriteLine($"  Change:   {metricComparison.PercentageChange:F1}%");

    if (metricComparison.IsRegression)
        Console.WriteLine("  [REGRESSION DETECTED]");
    else if (metricComparison.IsImprovement)
        Console.WriteLine("  [IMPROVEMENT DETECTED]");
}
```

## Notes

- **Output path management**: The `outputPath` parameter must point to an existing directory. Files from previous benchmark runs are not automatically cleaned up; callers should purge the directory between batches to avoid disk space exhaustion.
- **Empty results handling**: `GetBenchmarkStatistics` throws `ArgumentException` when passed an empty list. Always validate that at least one benchmark iteration completed before calling this method.
- **Comparison consistency**: `CompareBenchmarks` requires that the current and baseline batches have identical iteration counts and codec configurations. Mismatched batches will result in an `ArgumentException`.
- **Thread safety**: All static methods are thread-safe and can be called concurrently from multiple threads. The returned collections (`IReadOnlyList<T>`, `IReadOnlyDictionary<TKey, TValue>`) are immutable snapshots and safe for concurrent read access. Instance properties on `BenchmarkComparison` and `BenchmarkMetricComparison` are read-only and do not mutate after construction.
- **Regression and improvement thresholds**: `IsRegression` and `IsImprovement` are determined by internal thresholds. A metric with a negligible change may have both flags set to `false`. These two flags are mutually exclusive for any given metric.
- **Async validation**: `ValidateAllBenchmarksAsync` validates results concurrently. If multiple results fail, the exceptions are aggregated into an `AggregateException`. Callers should catch this type to inspect individual failures.
- **Timestamp precision**: The `Timestamp` property records the UTC time of benchmark execution at the moment the run completes, not when it was enqueued.
