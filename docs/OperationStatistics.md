# OperationStatistics

`OperationStatistics` is an in-memory aggregator for FFmpeg operation metrics. It groups statistics by `OperationType` and synchronizes access to its internal collection so that records, reads, reports, exports, and resets can be performed concurrently.

Each group is represented by [`OperationStats`](OperationStats.md), which exposes the counters and calculated values returned by this class.

## Recording operations

### `RecordSuccess(OperationType type, TimeSpan executionTime, long bytesProcessed)`

Records a successful operation for `type`. If the operation type has not been seen before, a new `OperationStats` entry is created.

The method:

- Increments `TotalAttempts` and `SuccessfulOperations`.
- Adds `executionTime` to `TotalExecutionTime`.
- Adds `bytesProcessed` to `TotalBytesProcessed`.
- Updates `MinimumExecutionTime` and `MaximumExecutionTime`.
- Sets `LastUpdated` to the current UTC time.

The implementation does not reject negative durations or byte counts.

### `RecordFailure(OperationType type)`

Records a failed operation for `type`, creating its entry when necessary. It increments `TotalAttempts` and `FailedOperations` and updates `LastUpdated` to the current UTC time. A failure does not add execution time or processed bytes and does not update the minimum or maximum execution time.

## Reading statistics

### `GetStatistics(OperationType type)`

Returns the `OperationStats` entry for `type`, or `null` when no entry exists. The returned value is the stored object, not a copy; modifying it changes the values subsequently observed by the aggregator, and those modifications are not synchronized by `OperationStatistics`.

### `GetAllStatistics()`

Returns a new list containing every recorded `OperationStats` entry. The list itself is a snapshot of the collection, but its elements are the stored objects rather than copies. The order is not specified. When nothing has been recorded, the method returns an empty list.

### `GetAggregateStatistics()`

Returns a new `OperationStats` whose `Type` is `OperationType.Unknown`. It sums the following values across all operation types:

- `TotalAttempts`
- `SuccessfulOperations`
- `FailedOperations`
- `TotalBytesProcessed`
- `TotalExecutionTime`

It also selects the lowest `MinimumExecutionTime` and highest `MaximumExecutionTime`. Calculated properties such as `SuccessRate`, `AverageExecutionTime`, and `AverageThroughputMBps` are then derived by `OperationStats` from the aggregated totals.

If there are no entries, the returned object contains its default values: zero counts, bytes, total and maximum times, while `MinimumExecutionTime` remains `TimeSpan.MaxValue`. `LastUpdated` is the construction time of the aggregate; it is not copied from the per-type entries.

## Reports and export

### `GetPerformanceReport()`

Returns a multiline text report with:

- An aggregate summary of attempts, successes, failures, and success rate.
- Aggregate average, minimum, maximum, and total execution times.
- Aggregate average throughput in MB/s.
- A breakdown by operation type, ordered by `OperationType`, with attempts, success rate, average time, and throughput.

Numeric values are formatted to one decimal place. Times are displayed in seconds. The method reports the values as stored; for example, an entry with failures but no successes still has `TimeSpan.MaxValue` as its minimum execution time.

### `ExportAsCSV()`

Returns CSV text containing a header and one row per recorded operation type. Rows follow the collection's enumeration order and contain:

```text
OperationType,TotalAttempts,Successful,Failed,SuccessRate,AverageTime,MinTime,MaxTime,ThroughputMBps
```

Time columns are expressed in seconds, and the success rate, time values, and throughput are formatted to one decimal place. The exporter performs no CSV escaping and uses the current culture when formatting numeric values.

## Resetting

### `Reset()`

Removes every operation-type entry. Previously returned `OperationStats` references are not modified, but they are no longer part of the aggregator.

## Example

```csharp
using System;
using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Monitoring;

var statistics = new OperationStatistics();

statistics.RecordSuccess(
    OperationType.Transcode,
    TimeSpan.FromSeconds(12.5),
    50L * 1024 * 1024);
statistics.RecordFailure(OperationType.Transcode);

OperationStats? transcode = statistics.GetStatistics(OperationType.Transcode);
OperationStats aggregate = statistics.GetAggregateStatistics();
string report = statistics.GetPerformanceReport();
string csv = statistics.ExportAsCSV();

statistics.Reset();
```
