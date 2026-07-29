// SPDX-License-Identifier: MIT
// © 2024 RedRocket

using System;
using Xunit;
using FFmpegDotnetWrapper.Utilities;

namespace FFmpegDotnetWrapper.Tests
{
    public class ProgressReportTests
    {
        [Fact]
        public void ProgressReport_DefaultValues_AreInitializedCorrectly()
        {
            var report = new ProgressReport();

            Assert.Equal(0.0, report.ProgressPercentage);
            Assert.Equal(0, report.ItemsCompleted);
            Assert.Equal(0, report.TotalItems);
            Assert.Equal(TimeSpan.Zero, report.ElapsedTime);
            Assert.Equal(TimeSpan.Zero, report.EstimatedTimeRemaining);
            Assert.Equal(string.Empty, report.StatusMessage);
            Assert.Equal(0.0, report.ThroughputItemsPerSecond);
            Assert.Equal(0.0, report.ThroughputBytesPerSecond);
        }

        [Fact]
        public void ProgressTracker_ReportItemProgress_UpdatesReport()
        {
            var tracker = new ProgressTracker(totalItems: 3);
            tracker.ReportItemProgress("first");
            var report1 = tracker.GetProgressReport();

            Assert.Equal(1, report1.ItemsCompleted);
            Assert.InRange(report1.ProgressPercentage, 33.0, 34.0); // ~33.33%

            tracker.ReportItemProgress("second");
            var report2 = tracker.GetProgressReport();

            Assert.Equal(2, report2.ItemsCompleted);
            Assert.InRange(report2.ProgressPercentage, 66.0, 67.0); // ~66.66%

            tracker.ReportItemProgress("third");
            var report3 = tracker.GetProgressReport();

            Assert.Equal(3, report3.ItemsCompleted);
            Assert.Equal(100.0, report3.ProgressPercentage);
            Assert.Equal("third", report3.StatusMessage);
        }

        [Fact]
        public void ProgressTracker_ReportPercentageProgress_ClampsValues()
        {
            var tracker = new ProgressTracker(totalItems: 4);

            // Below 0%
            tracker.ReportPercentageProgress(-10);
            var reportLow = tracker.GetProgressReport();
            Assert.Equal(0.0, reportLow.ProgressPercentage);
            Assert.Equal(0, reportLow.ItemsCompleted);

            // Above 100%
            tracker.ReportPercentageProgress(150, "over");
            var reportHigh = tracker.GetProgressReport();
            Assert.Equal(100.0, reportHigh.ProgressPercentage);
            Assert.Equal(4, reportHigh.ItemsCompleted);
            Assert.Equal("over", reportHigh.StatusMessage);
        }

        [Fact]
        public void ProgressTracker_ReportDurationProgress_CalculatesPercentageAndItems()
        {
            var tracker = new ProgressTracker(totalItems: 4);
            var processed = TimeSpan.FromSeconds(30);
            var total = TimeSpan.FromSeconds(100);

            tracker.ReportDurationProgress(processed, total, "duration");
            var report = tracker.GetProgressReport();

            // 30 / 100 = 30%
            Assert.InRange(report.ProgressPercentage, 29.9, 30.1);
            // 30% of 4 items => 1 (int truncation)
            Assert.Equal(1, report.ItemsCompleted);
            Assert.Equal("duration", report.StatusMessage);
        }

        [Fact]
        public void ProgressTracker_EstimatedTimeRemaining_IsZeroWhenNoProgress()
        {
            var tracker = new ProgressTracker(totalItems: 10);
            var report = tracker.GetProgressReport();

            // No progress made yet, ETA should be zero
            Assert.Equal(TimeSpan.Zero, report.EstimatedTimeRemaining);
        }

        [Fact]
        public void ObservableProgressTracker_RaisesEventWhenThresholdExceeded()
        {
            var tracker = new ObservableProgressTracker(totalItems: 2, reportingThreshold: 10);
            ProgressReport? capturedReport = null;
            tracker.ProgressChanged += r => capturedReport = r;

            // First report: 50% progress (threshold 10% -> event should fire)
            tracker.ReportItemProgress("first");

            Assert.NotNull(capturedReport);
            Assert.InRange(capturedReport!.ProgressPercentage, 49.0, 51.0);
            Assert.Equal("first", capturedReport.StatusMessage);
        }
    }
}
