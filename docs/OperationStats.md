# OperationStats

`OperationStats` is a utility class designed to track and analyze the performance and outcomes of FFmpeg operations. It records success/failure metrics, execution times, and data throughput, and provides reporting and aggregation capabilities for monitoring batch processing jobs.

## API

### `public OperationType Type`
Gets or sets the type of FFmpeg operation being tracked (e.g., conversion, thumbnail generation). This categorizes the statistics for filtering and aggregation.

### `public int TotalAttempts`
Gets the total number of operation attempts recorded, including both successful and failed operations.

### `public int SuccessfulOperations`
Gets the number of operations that completed successfully.

### `public int FailedOperations`
Gets the number of operations that failed.

### `public long TotalBytesProcessed`
Gets the total number of bytes processed across all successful operations.

### `public TimeSpan TotalExecutionTime`
Gets the cumulative duration of all operation executions.

### `public TimeSpan MinimumExecutionTime`
Gets the shortest execution time observed for any single operation.

### `public TimeSpan MaximumExecutionTime`
Gets the longest execution time observed for any single operation.

### `public DateTime LastUpdated`
Gets the timestamp of the last update to this statistics instance.

### `public void RecordSuccess(long bytesProcessed, TimeSpan executionTime)`
Records a successful operation, updating success count, bytes processed, and execution time metrics.

- **Parameters**:
  - `bytesProcessed`: The number of bytes processed in the operation.
  - `executionTime`: The duration of the operation.
- **Throws**: `ArgumentOutOfRangeException` if `executionTime` is negative or `bytesProcessed` is negative.

### `public void RecordFailure(TimeSpan executionTime)`
Records a failed operation, updating failure count and execution time metrics.

- **Parameters**:
  - `executionTime`: The duration of the operation.
- **Throws**: `ArgumentOutOfRangeException` if `executionTime` is negative.

### `public OperationStats? GetStatistics()`
Returns a deep copy of the current statistics. Returns `null` if no operations have been recorded.

- **Returns**: A new `OperationStats` instance with the same values, or `null`.

### `public List<OperationStats> GetAllStatistics()`
Returns a list of all recorded statistics. Returns an empty list if no operations have been recorded.

- **Returns**: A `List<OperationStats>` containing all recorded statistics.

### `public OperationStats GetAggregateStatistics()`
Aggregates all recorded statistics into a single instance, summing totals and averaging times.

- **Returns**: A new `OperationStats` with aggregated values.
- **Throws**: `InvalidOperationException` if no statistics are available.

### `public string GetPerformanceReport()`
Generates a human-readable performance report summarizing success rates, throughput, and execution times.

- **Returns**: A formatted string containing the performance report.

### `public void Reset()`
Resets all statistics to their initial values (zero counts, zero times, etc.).

### `public string ExportAsCSV()`
Exports the current statistics as a CSV-formatted string with headers.

- **Returns**: A string containing the CSV data.

## Usage

### Example 1: Tracking a Batch of Conversions
