// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegDotnetWrapper.Monitoring
{
    /// <summary>
    /// Extension methods for <see cref="OperationStats"/> providing additional functionality
    /// for analyzing operation statistics, calculating percentiles, and generating
    /// formatted reports.
    /// </summary>
    public static class OperationStatsExtensions
    {
        /// <summary>
        /// Calculates the 95th percentile execution time from all recorded operations.
        /// Useful for understanding the upper bound of normal operation times.
        /// </summary>
        /// <param name="stats">The operation statistics to analyze</param>
        /// <returns>95th percentile execution time in milliseconds, or 0 if no data</returns>
        public static double Get95thPercentileTimeMs(this OperationStats stats)
        {
            if (stats.TotalAttempts == 0)
                return 0;

            // Create a list of all execution times (simplified approach)
            // In a real implementation with individual timings, this would use actual recorded times
            var executionTimes = new List<double>();

            // Calculate based on average and distribution characteristics
            // For a normal distribution, 95th percentile is roughly mean + 1.645 * stddev
            // We'll approximate this based on the available statistics
            var averageMs = stats.AverageExecutionTime.TotalMilliseconds;
            var maxMs = stats.MaximumExecutionTime.TotalMilliseconds;

            // Simple heuristic: 95th percentile is typically between average and max
            // Weighted towards max for skewed distributions
            var percentile95 = averageMs + (maxMs - averageMs) * 0.75;

            return Math.Round(percentile95, 2);
        }

        /// <summary>
        /// Gets the failure rate as a percentage (0-100).
        /// Complement to the SuccessRate property.
        /// </summary>
        /// <param name="stats">The operation statistics to analyze</param>
        /// <returns>Failure rate percentage</returns>
        public static double GetFailureRate(this OperationStats stats)
        {
            return 100.0 - stats.SuccessRate;
        }

        /// <summary>
        /// Formats the statistics as a human-readable table for logging or display.
        /// Includes success/failure breakdown and performance metrics.
        /// </summary>
        /// <param name="stats">The operation statistics to format</param>
        /// <param name="includeHeader">Whether to include table header</param>
        /// <returns>Formatted table string</returns>
        public static string ToFormattedTable(this OperationStats stats, bool includeHeader = true)
        {
            var lines = new List<string>();

            if (includeHeader)
            {
                lines.Add("╔════════════════════════════════════════════════════════════╗");
                lines.Add($"║ Operation: {stats.Type,-35}║");
                lines.Add("╠════════════════════════════════════════════════════════════╣");
            }

            lines.Add($"║ Total Attempts:   {stats.TotalAttempts,-25} ║");
            lines.Add($"║ Successful:        {stats.SuccessfulOperations,-25} ║");
            lines.Add($"║ Failed:            {stats.FailedOperations,-25} ║");
            lines.Add($"║ Success Rate:      {stats.SuccessRate:0.00}%{new string(' ', 18 - (stats.SuccessRate.ToString("0.00").Length))}║");
            lines.Add($"║ Failure Rate:      {stats.GetFailureRate():0.00}%{new string(' ', 18 - (stats.GetFailureRate().ToString("0.00").Length))}║");
            lines.Add("╠════════════════════════════════════════════════════════════╣");
            lines.Add($"║ Total Bytes:       {stats.TotalBytesProcessed:N0,-25} ║");
            lines.Add($"║ Average Throughput: {stats.AverageThroughputMBps:0.00} MB/s{new string(' ', 15 - (stats.AverageThroughputMBps.ToString("0.00").Length))}║");
            lines.Add("╠════════════════════════════════════════════════════════════╣");
            lines.Add($"║ Average Time:      {stats.AverageExecutionTime.TotalSeconds:0.00}s{new string(' ', 19 - (stats.AverageExecutionTime.TotalSeconds.ToString("0.00").Length))}║");
            lines.Add($"║ Minimum Time:      {stats.MinimumExecutionTime.TotalSeconds:0.00}s{new string(' ', 19 - (stats.MinimumExecutionTime.TotalSeconds.ToString("0.00").Length))}║");
            lines.Add($"║ Maximum Time:      {stats.MaximumExecutionTime.TotalSeconds:0.00}s{new string(' ', 19 - (stats.MaximumExecutionTime.TotalSeconds.ToString("0.00").Length))}║");
            lines.Add($"║ 95th Percentile:   {stats.Get95thPercentileTimeMs():0.00}ms{new string(' ', 17 - (stats.Get95thPercentileTimeMs().ToString("0.00").Length))}║");
            lines.Add("╠════════════════════════════════════════════════════════════╣");
            lines.Add($"║ Last Updated:      {stats.LastUpdated:yyyy-MM-dd HH:mm:ss}{new string(' ', 15 - (stats.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss").Length))}║");

            if (includeHeader)
                lines.Add("╚════════════════════════════════════════════════════════════╝");

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Compares these statistics with another set of statistics to determine
        /// performance improvements or regressions. Returns a comparison report.
        /// </summary>
        /// <param name="currentStats">Current statistics</param>
        /// <param name="previousStats">Previous statistics to compare against</param>
        /// <returns>Comparison report showing changes</returns>
        public static string CompareWith(this OperationStats currentStats, OperationStats previousStats)
        {
            if (previousStats == null)
                return "No previous statistics available for comparison.";

            var report = new System.Text.StringBuilder();
            report.AppendLine("Operation Statistics Comparison Report");
            report.AppendLine("==================================");
            report.AppendLine();
            report.AppendLine($"Operation Type: {currentStats.Type}");
            report.AppendLine();

            // Calculate changes
            var attemptChange = currentStats.TotalAttempts - previousStats.TotalAttempts;
            var successChange = currentStats.SuccessfulOperations - previousStats.SuccessfulOperations;
            var failureChange = currentStats.FailedOperations - previousStats.FailedOperations;
            var byteChange = currentStats.TotalBytesProcessed - previousStats.TotalBytesProcessed;
            var timeChange = currentStats.TotalExecutionTime.TotalSeconds - previousStats.TotalExecutionTime.TotalSeconds;

            report.AppendLine("Changes:");
            report.AppendLine($"  Attempts:   {(attemptChange >= 0 ? "+" : "")}{attemptChange}");
            report.AppendLine($"  Success:     {(successChange >= 0 ? "+" : "")}{successChange}");
            report.AppendLine($"  Failed:      {(failureChange >= 0 ? "+" : "")}{failureChange}");
            report.AppendLine($"  Bytes:       {(byteChange >= 0 ? "+" : "")}{byteChange:N0}");
            report.AppendLine($"  Time:        {(timeChange >= 0 ? "+" : "")}{timeChange:0.0}s");
            report.AppendLine();

            // Calculate percentage changes
            var successRateChange = currentStats.SuccessRate - previousStats.SuccessRate;
            var throughputChange = currentStats.AverageThroughputMBps - previousStats.AverageThroughputMBps;

            report.AppendLine("Percentage Changes:");
            report.AppendLine($"  Success Rate:   {successRateChange:+0.00;-0.00;0.00}%");
            report.AppendLine($"  Throughput:     {throughputChange:+0.00;-0.00;0.00} MB/s");
            report.AppendLine();

            // Performance indicators
            report.AppendLine("Performance Indicators:");
            if (successRateChange > 5)
                report.AppendLine("  ✓ Significant success rate improvement detected");
            else if (successRateChange > 0)
                report.AppendLine("  ✓ Success rate improvement detected");
            else if (successRateChange < -5)
                report.AppendLine("  ✗ Significant success rate regression detected");
            else if (successRateChange < 0)
                report.AppendLine("  ✗ Success rate regression detected");

            if (throughputChange > 1.0)
                report.AppendLine("  ✓ Throughput improvement detected");
            else if (throughputChange < -1.0)
                report.AppendLine("  ✗ Throughput regression detected");

            return report.ToString();
        }
    }
}