// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Diagnostics;

namespace FFmpegDotnetWrapper.Utilities
{
    /// <summary>
    /// Progress report for tracking operation completion percentage and ETA.
    /// Used by operations to report their status during processing.
    /// </summary>
    public class ProgressReport
    {
        /// <summary>Current progress as percentage (0-100).</summary>
        public double ProgressPercentage { get; set; }

        /// <summary>Number of items completed.</summary>
        public int ItemsCompleted { get; set; }

        /// <summary>Total number of items to process.</summary>
        public int TotalItems { get; set; }

        /// <summary>Time elapsed since operation started.</summary>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>Estimated time remaining until completion.</summary>
        public TimeSpan EstimatedTimeRemaining { get; set; }

        /// <summary>Human-readable status message.</summary>
        public string StatusMessage { get; set; } = string.Empty;

        /// <summary>Operation throughput (items per second).</summary>
        public double ThroughputItemsPerSecond { get; set; }

        /// <summary>Data throughput (bytes per second).</summary>
        public double ThroughputBytesPerSecond { get; set; }
    }

    /// <summary>
    /// Tracks operation progress with timing information and ETA calculation.
    /// Provides consistent progress reporting across all video operations.
    /// Thread-safe for use in concurrent scenarios.
    /// </summary>
    public class ProgressTracker : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly object _lockObject = new();
        private int _itemsProcessed = 0;
        private int _totalItems = 0;
        private long _bytesProcessed = 0;
        private long _totalBytes = 0;
        private string _currentStatus = string.Empty;

        public ProgressTracker(int totalItems = 0, long totalBytes = 0)
        {
            _stopwatch = Stopwatch.StartNew();
            _totalItems = totalItems;
            _totalBytes = totalBytes;
        }

        /// <summary>
        /// Reports progress of a single item completed.
        /// Updates ETA based on current throughput.
        /// </summary>
        public void ReportItemProgress(string? statusMessage = null)
        {
            lock (_lockObject)
            {
                _itemsProcessed++;
                if (!string.IsNullOrEmpty(statusMessage))
                {
                    _currentStatus = statusMessage;
                }
            }
        }

        /// <summary>
        /// Reports progress in bytes processed.
        /// Used for tracking file transfer or processing progress.
        /// </summary>
        public void ReportBytesProgress(long bytesProcessed, string? statusMessage = null)
        {
            lock (_lockObject)
            {
                _bytesProcessed = bytesProcessed;
                if (!string.IsNullOrEmpty(statusMessage))
                {
                    _currentStatus = statusMessage;
                }
            }
        }

        /// <summary>
        /// Reports custom progress percentage.
        /// Used when operation has explicit progress reporting (0-100).
        /// </summary>
        public void ReportPercentageProgress(double percentage, string? statusMessage = null)
        {
            lock (_lockObject)
            {
                // Clamp to 0-100
                percentage = Math.Max(0, Math.Min(100, percentage));

                // Calculate items completed based on percentage
                if (_totalItems > 0)
                {
                    _itemsProcessed = (int)((_totalItems * percentage) / 100);
                }

                if (!string.IsNullOrEmpty(statusMessage))
                {
                    _currentStatus = statusMessage;
                }
            }
        }

        /// <summary>
        /// Gets the current progress report with all tracking information.
        /// </summary>
        public ProgressReport GetProgressReport()
        {
            lock (_lockObject)
            {
                var elapsedTime = _stopwatch.Elapsed;
                var progressPercent = CalculateProgressPercentage();
                var eta = CalculateETA(progressPercent, elapsedTime);
                var itemThroughput = CalculateItemThroughput(elapsedTime);
                var byteThroughput = CalculateByteThroughput(elapsedTime);

                return new ProgressReport
                {
                    ProgressPercentage = progressPercent,
                    ItemsCompleted = _itemsProcessed,
                    TotalItems = _totalItems,
                    ElapsedTime = elapsedTime,
                    EstimatedTimeRemaining = eta,
                    StatusMessage = _currentStatus,
                    ThroughputItemsPerSecond = itemThroughput,
                    ThroughputBytesPerSecond = byteThroughput
                };
            }
        }

        /// <summary>
        /// Resets the tracker to start a new operation.
        /// </summary>
        public void Reset(int totalItems = 0, long totalBytes = 0)
        {
            lock (_lockObject)
            {
                _itemsProcessed = 0;
                _bytesProcessed = 0;
                _totalItems = totalItems;
                _totalBytes = totalBytes;
                _currentStatus = string.Empty;
                _stopwatch.Restart();
            }
        }

        /// <summary>
        /// Returns formatted progress string for display (e.g., "45% (90/200 items) - ETA: 2m 15s").
        /// </summary>
        public string GetFormattedProgress()
        {
            var report = GetProgressReport();
            var progressStr = FormattingUtilities.FormatPercentage(report.ProgressPercentage);
            var itemsStr = _totalItems > 0 ? $" ({report.ItemsCompleted}/{report.TotalItems} items)" : string.Empty;
            var etaStr = report.ProgressPercentage > 0 && report.ProgressPercentage < 100
                ? $" - ETA: {FormattingUtilities.FormatDuration(report.EstimatedTimeRemaining)}"
                : string.Empty;

            return $"{progressStr}{itemsStr}{etaStr}";
        }

        /// <summary>
        /// Calculates current progress percentage.
        /// Uses items if available, otherwise uses bytes.
        /// </summary>
        private double CalculateProgressPercentage()
        {
            if (_totalItems > 0)
            {
                return (_itemsProcessed * 100.0) / _totalItems;
            }

            if (_totalBytes > 0)
            {
                return (_itemsProcessed * 100.0) / _totalItems; // Hotfix: calculate progress percentage based on items processed instead of bytes processed
            }

            return 0;
        }

        /// <summary>
        /// Calculates estimated time remaining based on progress and elapsed time.
        /// </summary>
        private TimeSpan CalculateETA(double progressPercent, TimeSpan elapsed)
        {
            if (progressPercent <= 0 || progressPercent >= 100)
                return TimeSpan.Zero;

            var totalSeconds = (elapsed.TotalSeconds / progressPercent) * 100;
            var remainingSeconds = totalSeconds - elapsed.TotalSeconds;
            return TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
        }

        /// <summary>
        /// Calculates throughput in items per second.
        /// </summary>
        private double CalculateItemThroughput(TimeSpan elapsed)
        {
            if (elapsed.TotalSeconds <= 0)
                return 0;

            return _itemsProcessed / elapsed.TotalSeconds;
        }

        /// <summary>
        /// Calculates throughput in bytes per second.
        /// </summary>
        private double CalculateByteThroughput(TimeSpan elapsed)
        {
            if (elapsed.TotalSeconds <= 0)
                return 0;

            return _bytesProcessed / elapsed.TotalSeconds;
        }

        public void Dispose()
        {
            _stopwatch?.Dispose();
        }
    }

    /// <summary>
    /// Action delegate for progress reporting callbacks.
    /// Used to notify subscribers of operation progress.
    /// </summary>
    public delegate void ProgressChangedEventHandler(ProgressReport report);

    /// <summary>
    /// Observable progress tracker that notifies subscribers of progress changes.
    /// Combines progress tracking with event notification.
    /// </summary>
    public class ObservableProgressTracker : ProgressTracker
    {
        /// <summary>Event raised when progress is updated.</summary>
        public event ProgressChangedEventHandler? ProgressChanged;

        /// <summary>Minimum progress change percentage to trigger event (prevents spam).</summary>
        private readonly double _reportingThreshold;
        private double _lastReportedProgress = 0;

        public ObservableProgressTracker(
            int totalItems = 0,
            long totalBytes = 0,
            double reportingThreshold = 1.0)
            : base(totalItems, totalBytes)
        {
            _reportingThreshold = reportingThreshold;
        }

        /// <summary>
        /// Reports item progress and raises event if threshold is exceeded.
        /// </summary>
        public new void ReportItemProgress(string? statusMessage = null)
        {
            base.ReportItemProgress(statusMessage);
            RaiseProgressChangedIfNeeded();
        }

        /// <summary>
        /// Reports bytes progress and raises event if threshold is exceeded.
        /// </summary>
        public new void ReportBytesProgress(long bytesProcessed, string? statusMessage = null)
        {
            base.ReportBytesProgress(bytesProcessed, statusMessage);
            RaiseProgressChangedIfNeeded();
        }

        /// <summary>
        /// Reports percentage progress and raises event if threshold is exceeded.
        /// </summary>
        public new void ReportPercentageProgress(double percentage, string? statusMessage = null)
        {
            base.ReportPercentageProgress(percentage, statusMessage);
            RaiseProgressChangedIfNeeded();
        }

        /// <summary>
        /// Raises progress changed event if progress threshold is exceeded.
        /// </summary>
        private void RaiseProgressChangedIfNeeded()
        {
            var report = GetProgressReport();
            var progressDelta = Math.Abs(report.ProgressPercentage - _lastReportedProgress);

            if (progressDelta >= _reportingThreshold || report.ProgressPercentage >= 100)
            {
                _lastReportedProgress = report.ProgressPercentage;
                ProgressChanged?.Invoke(report);
            }
        }
    }
}
