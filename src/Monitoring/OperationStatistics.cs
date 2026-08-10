// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using FFmpegDotnetWrapper.Constants;

namespace FFmpegDotnetWrapper.Monitoring
{
    /// <summary>
    /// Statistics for a single operation type (transcode, trim, etc).
    /// Tracks success/failure rates, execution times, and throughput.
    /// </summary>
    public class OperationStats
    {
        /// <summary>Operation type (transcode, watermark, etc).</summary>
        public OperationType Type { get; set; }

        /// <summary>Total number of operations attempted.</summary>
        public int TotalAttempts { get; set; }

        /// <summary>Number of successful operations.</summary>
        public int SuccessfulOperations { get; set; }

        /// <summary>Number of failed operations.</summary>
        public int FailedOperations { get; set; }

        /// <summary>Success rate as percentage (0-100).</summary>
        public double SuccessRate => TotalAttempts > 0
            ? (SuccessfulOperations * 100.0) / TotalAttempts
            : 0;

        /// <summary>Total bytes processed across all operations.</summary>
        public long TotalBytesProcessed { get; set; }

        /// <summary>Total execution time across all operations.</summary>
        public TimeSpan TotalExecutionTime { get; set; }

        /// <summary>Average execution time per operation.</summary>
        public TimeSpan AverageExecutionTime => TotalAttempts > 0
            ? TimeSpan.FromMilliseconds(TotalExecutionTime.TotalMilliseconds / TotalAttempts)
            : TimeSpan.Zero;

        /// <summary>Minimum execution time recorded.</summary>
        public TimeSpan MinimumExecutionTime { get; set; } = TimeSpan.MaxValue;

        /// <summary>Maximum execution time recorded.</summary>
        public TimeSpan MaximumExecutionTime { get; set; } = TimeSpan.Zero;

        /// <summary>Timestamp when statistics were last updated.</summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>Throughput in MB/s (average).</summary>
        public double AverageThroughputMBps
        {
            get
            {
                if (TotalExecutionTime.TotalSeconds <= 0 || TotalBytesProcessed <= 0)
                    return 0;

                return (TotalBytesProcessed / 1024 / 1024) / TotalExecutionTime.TotalSeconds;
            }
        }
    }

    /// <summary>
    /// Comprehensive statistics tracker for all operations.
    /// Collects metrics on success rates, performance, and resource usage.
    /// Thread-safe for concurrent updates from multiple operations.
    /// </summary>
    public class OperationStatistics
    {
        private readonly Dictionary<OperationType, OperationStats> _statistics = new();
        private readonly object _lockObject = new();

        /// <summary>
        /// Records a successful operation.
        /// Updates success count, execution time, and throughput metrics.
        /// </summary>
        public void RecordSuccess(
            OperationType type,
            TimeSpan executionTime,
            long bytesProcessed)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (executionTime == null)
                throw new ArgumentNullException(nameof(executionTime));
            lock (_lockObject)
            {
                if (!_statistics.TryGetValue(type, out var stats))
                {
                    stats = new OperationStats { Type = type };
                    _statistics[type] = stats;
                }

                stats.TotalAttempts++;
                stats.SuccessfulOperations++;
                stats.TotalExecutionTime += executionTime;
                stats.TotalBytesProcessed += bytesProcessed;

                // Update min/max times
                if (executionTime < stats.MinimumExecutionTime)
                    stats.MinimumExecutionTime = executionTime;
                if (executionTime > stats.MaximumExecutionTime)
                    stats.MaximumExecutionTime = executionTime;

                stats.LastUpdated = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Records a failed operation.
        /// Only updates attempt and failure counts.
        /// </summary>
        public void RecordFailure(OperationType type)
        {
            lock (_lockObject)
            {
                if (!_statistics.TryGetValue(type, out var stats))
                {
                    stats = new OperationStats { Type = type };
                    _statistics[type] = stats;
                }

                stats.TotalAttempts++;
                stats.FailedOperations++;
                stats.LastUpdated = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets statistics for a specific operation type.
        /// Returns null if no statistics have been recorded.
        /// </summary>
        public OperationStats? GetStatistics(OperationType type)
        {
            lock (_lockObject)
            {
                _statistics.TryGetValue(type, out var stats);
                return stats;
            }
        }

        /// <summary>
        /// Gets all recorded statistics across all operation types.
        /// </summary>
        public List<OperationStats> GetAllStatistics()
        {
            lock (_lockObject)
            {
                return _statistics.Values.ToList();
            }
        }

        /// <summary>
        /// Gets overall system statistics aggregated from all operations.
        /// </summary>
        public OperationStats GetAggregateStatistics()
        {
            lock (_lockObject)
            {
                var aggregate = new OperationStats { Type = OperationType.Unknown };

                foreach (var stats in _statistics.Values)
                {
                    aggregate.TotalAttempts += stats.TotalAttempts;
                    aggregate.SuccessfulOperations += stats.SuccessfulOperations;
                    aggregate.FailedOperations += stats.FailedOperations;
                    aggregate.TotalBytesProcessed += stats.TotalBytesProcessed;
                    aggregate.TotalExecutionTime += stats.TotalExecutionTime;

                    if (stats.MinimumExecutionTime < aggregate.MinimumExecutionTime)
                        aggregate.MinimumExecutionTime = stats.MinimumExecutionTime;
                    if (stats.MaximumExecutionTime > aggregate.MaximumExecutionTime)
                        aggregate.MaximumExecutionTime = stats.MaximumExecutionTime;
                }

                return aggregate;
            }
        }

        /// <summary>
        /// Gets performance report with percentiles and distribution.
        /// Useful for understanding performance characteristics.
        /// </summary>
        public string GetPerformanceReport()
        {
            lock (_lockObject)
            {
                var report = new System.Text.StringBuilder();
                report.AppendLine("FFmpeg Operation Statistics Report");
                report.AppendLine("===================================");
                report.AppendLine();

                var aggregate = GetAggregateStatistics();

                // Summary
                report.AppendLine("Summary:");
                report.AppendLine($"  Total Operations: {aggregate.TotalAttempts}");
                report.AppendLine($"  Successful: {aggregate.SuccessfulOperations} ({aggregate.SuccessRate:0.0}%)");
                report.AppendLine($"  Failed: {aggregate.FailedOperations}");
                report.AppendLine();

                // Performance
                report.AppendLine("Performance:");
                report.AppendLine($"  Average Time: {aggregate.AverageExecutionTime.TotalSeconds:0.0}s");
                report.AppendLine($"  Min Time: {aggregate.MinimumExecutionTime.TotalSeconds:0.0}s");
                report.AppendLine($"  Max Time: {aggregate.MaximumExecutionTime.TotalSeconds:0.0}s");
                report.AppendLine($"  Total Time: {aggregate.TotalExecutionTime.TotalSeconds:0.0}s");
                report.AppendLine($"  Average Throughput: {aggregate.AverageThroughputMBps:0.0} MB/s");
                report.AppendLine();

                // Per-type breakdown
                report.AppendLine("By Operation Type:");
                foreach (var stats in _statistics.Values.OrderBy(s => s.Type))
                {
                    report.AppendLine($"  {stats.Type}:");
                    report.AppendLine($"    Attempts: {stats.TotalAttempts}, Success Rate: {stats.SuccessRate:0.0}%");
                    report.AppendLine($"    Avg Time: {stats.AverageExecutionTime.TotalSeconds:0.0}s, Throughput: {stats.AverageThroughputMBps:0.0} MB/s");
                }

                return report.ToString();
            }
        }

        /// <summary>
        /// Resets all statistics counters.
        /// Used for testing or starting a new measurement period.
        /// </summary>
        public void Reset()
        {
            lock (_lockObject)
            {
                _statistics.Clear();
            }
        }

        /// <summary>
        /// Exports statistics in CSV format for analysis.
        /// </summary>
        public string ExportAsCSV()
        {
            lock (_lockObject)
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("OperationType,TotalAttempts,Successful,Failed,SuccessRate,AverageTime,MinTime,MaxTime,ThroughputMBps");

                foreach (var stats in _statistics.Values)
                {
                    csv.AppendLine($"{stats.Type}," +
                        $"{stats.TotalAttempts}," +
                        $"{stats.SuccessfulOperations}," +
                        $"{stats.FailedOperations}," +
                        $"{stats.SuccessRate:0.0}," +
                        $"{stats.AverageExecutionTime.TotalSeconds:0.0}," +
                        $"{stats.MinimumExecutionTime.TotalSeconds:0.0}," +
                        $"{stats.MaximumExecutionTime.TotalSeconds:0.0}," +
                        $"{stats.AverageThroughputMBps:0.0}");
                }

                return csv.ToString();
            }
        }
    }
}
