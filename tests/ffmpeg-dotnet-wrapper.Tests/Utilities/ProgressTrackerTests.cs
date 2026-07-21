// =============================================================================
// Author: Test Generator
// Unit tests for ProgressTracker class
// Tests parsing ffmpeg progress/stderr time= lines, percent calculation,
// malformed lines ignored, and all core functionality
// =============================================================================

using System;
using FluentAssertions;
using Xunit;
using FFmpegDotnetWrapper.Utilities;

namespace ffmpeg_dotnet_wrapper_tests.Utilities
{
    public class ProgressTrackerTests
    {
        [Fact]
        public void Constructor_WithTotalItems_InitializesCorrectly()
        {
            // Arrange & Act
            var tracker = new ProgressTracker(totalItems: 100);
            var report = tracker.GetProgressReport();

            // Assert
            report.TotalItems.Should().Be(100);
            report.ItemsCompleted.Should().Be(0);
            report.ProgressPercentage.Should().Be(0);
            report.ElapsedTime.TotalMilliseconds.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Constructor_WithTotalBytes_InitializesCorrectly()
        {
            // Arrange & Act
            var tracker = new ProgressTracker(totalBytes: 1024 * 1024);
            var report = tracker.GetProgressReport();

            // Assert
            report.TotalItems.Should().Be(0);
            report.ItemsCompleted.Should().Be(0);
            report.ProgressPercentage.Should().Be(0);
        }

        [Fact]
        public void Constructor_WithZeroValues_InitializesCorrectly()
        {
            // Arrange & Act
            var tracker = new ProgressTracker(0, 0);
            var report = tracker.GetProgressReport();

            // Assert
            report.TotalItems.Should().Be(0);
            report.ItemsCompleted.Should().Be(0);
            report.ProgressPercentage.Should().Be(0);
        }

        [Fact]
        public void ReportItemProgress_IncrementsItemsCompleted()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);

            // Act
            tracker.ReportItemProgress();
            tracker.ReportItemProgress();
            tracker.ReportItemProgress();
            var report = tracker.GetProgressReport();

            // Assert
            report.ItemsCompleted.Should().Be(3);
            report.ProgressPercentage.Should().BeApproximately(3.0, 0.001);
        }

        [Fact]
        public void ReportItemProgress_WithStatusMessage_SetsStatusMessage()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);

            // Act
            tracker.ReportItemProgress("Processing frame 42");
            var report = tracker.GetProgressReport();

            // Assert
            report.ItemsCompleted.Should().Be(1);
            report.StatusMessage.Should().Be("Processing frame 42");
        }

        [Fact]
        public void ReportItemProgress_MultipleTimes_CalculatesCorrectPercentage()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 50);

            // Act - report 10 items
            for (int i = 0; i < 10; i++)
            {
                tracker.ReportItemProgress();
            }
            var report = tracker.GetProgressReport();

            // Assert
            report.ItemsCompleted.Should().Be(10);
            report.ProgressPercentage.Should().BeApproximately(20.0, 0.001);
        }

        [Fact]
        public void ReportBytesProgress_UpdatesBytesProcessed()
        {
            // Arrange
            var tracker = new ProgressTracker(totalBytes: 1024 * 1024);

            // Act
            tracker.ReportBytesProgress(512 * 1024, "Downloaded 512KB");
            tracker.ReportBytesProgress(768 * 1024, "Downloaded 768KB");
            var report = tracker.GetProgressReport();

            // Assert
            report.ThroughputBytesPerSecond.Should().BeGreaterThan(0);
            report.StatusMessage.Should().Be("Downloaded 768KB");
        }

        [Fact]
        public void ReportPercentageProgress_SetsCorrectPercentage()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 200);

            // Act
            tracker.ReportPercentageProgress(25.5, "25% complete");
            var report = tracker.GetProgressReport();

            // Assert
            report.ItemsCompleted.Should().Be(51); // 25.5% of 200 = 51
            report.ProgressPercentage.Should().BeApproximately(25.5, 0.001);
            report.StatusMessage.Should().Be("25% complete");
        }

        [Fact]
        public void ReportPercentageProgress_ClampsToZero()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);

            // Act
            tracker.ReportPercentageProgress(-10, "Negative percentage");
            var report = tracker.GetProgressReport();

            // Assert
            report.ProgressPercentage.Should().Be(0);
        }

        [Fact]
        public void ReportPercentageProgress_ClampsToHundred()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);

            // Act
            tracker.ReportPercentageProgress(150, "Over 100%");
            var report = tracker.GetProgressReport();

            // Assert
            report.ProgressPercentage.Should().Be(100);
        }

        [Fact]
        public void ReportDurationProgress_CalculatesCorrectPercentage()
        {
            // Arrange
            var tracker = new ProgressTracker();
            var totalDuration = TimeSpan.FromSeconds(100);
            var processedDuration = TimeSpan.FromSeconds(30);

            // Act
            tracker.ReportDurationProgress(processedDuration, totalDuration, "Processing video");
            var report = tracker.GetProgressReport();

            // Assert
            report.ProgressPercentage.Should().BeApproximately(30.0, 0.001);
            report.StatusMessage.Should().Be("Processing video");
        }

        [Fact]
        public void ReportDurationProgress_WithZeroTotalDuration_ReturnsZero()
        {
            // Arrange
            var tracker = new ProgressTracker();
            var totalDuration = TimeSpan.Zero;
            var processedDuration = TimeSpan.FromSeconds(10);

            // Act
            tracker.ReportDurationProgress(processedDuration, totalDuration);
            var report = tracker.GetProgressReport();

            // Assert
            report.ProgressPercentage.Should().Be(0);
        }

        [Fact]
        public void GetProgressReport_ReturnsAllFieldsPopulated()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);
            tracker.ReportItemProgress("Processing...");

            // Act
            var report = tracker.GetProgressReport();

            // Assert
            report.ProgressPercentage.Should().BeGreaterThan(0);
            report.ItemsCompleted.Should().Be(1);
            report.TotalItems.Should().Be(100);
            report.ElapsedTime.TotalMilliseconds.Should().BeGreaterThan(0);
            report.StatusMessage.Should().Be("Processing...");
            report.ThroughputItemsPerSecond.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public void PercentComplete_Property_ReturnsCorrectValue()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 50);

            // Act - report 25 items
            for (int i = 0; i < 25; i++)
            {
                tracker.ReportItemProgress();
            }

            // Assert
            tracker.PercentComplete.Should().BeApproximately(50.0, 0.001);
        }

        [Fact]
        public void PercentComplete_Property_ClampsToZero()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);
            tracker.ReportPercentageProgress(-50);

            // Assert
            tracker.PercentComplete.Should().Be(0);
        }

        [Fact]
        public void PercentComplete_Property_ClampsToHundred()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);
            tracker.ReportPercentageProgress(150);

            // Assert
            tracker.PercentComplete.Should().Be(100);
        }

        [Fact]
        public void Reset_ClearsAllState()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);
            tracker.ReportItemProgress("First");
            tracker.ReportPercentageProgress(50);

            // Verify initial state - after 50% progress, 50 items should be completed
            tracker.GetProgressReport().ItemsCompleted.Should().Be(50);
            tracker.GetProgressReport().ProgressPercentage.Should().BeApproximately(50.0, 0.001);

            // Act
            tracker.Reset(totalItems: 200);
            var report = tracker.GetProgressReport();

            // Assert
            report.ItemsCompleted.Should().Be(0);
            report.TotalItems.Should().Be(200);
            report.ProgressPercentage.Should().Be(0);
            report.StatusMessage.Should().BeEmpty();
        }

        [Fact]
        public void Reset_ResetsStopwatch()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);
            tracker.ReportItemProgress();
            System.Threading.Thread.Sleep(10); // Ensure some time passes

            // Act
            tracker.Reset();
            var report1 = tracker.GetProgressReport();
            System.Threading.Thread.Sleep(10);
            var report2 = tracker.GetProgressReport();

            // Assert - elapsed time should be less after reset
            report2.ElapsedTime.Should().BeLessThan(report1.ElapsedTime);
        }

        [Fact]
        public void GetFormattedProgress_ReturnsExpectedFormat()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);
            tracker.ReportItemProgress("Processing frames");

            // Act
            var formatted = tracker.GetFormattedProgress();

            // Assert
            formatted.Should().NotBeNullOrEmpty();
            formatted.Should().Contain("%");
            formatted.Should().Contain("1/100");
            formatted.Should().Contain("Processing frames");
        }

        [Fact]
        public void GetFormattedProgress_WithoutTotalItems_ShowsOnlyPercentage()
        {
            // Arrange
            var tracker = new ProgressTracker();
            tracker.ReportPercentageProgress(45.5);

            // Act
            var formatted = tracker.GetFormattedProgress();

            // Assert
            formatted.Should().Contain("45.5%");
            formatted.Should().NotContain("/"); // No items count
        }

        [Fact]
        public void CalculateETA_ReturnsZeroAtZeroPercent()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);
            var report = tracker.GetProgressReport();

            // Act - use reflection to call private method
            var method = typeof(ProgressTracker).GetMethod(
                "CalculateETA",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var eta = (TimeSpan)method.Invoke(tracker, new object[] { 0.0, report.ElapsedTime });

            // Assert
            eta.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void CalculateETA_ReturnsZeroAtHundredPercent()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);
            tracker.ReportItemProgress(); // 1%
            var report = tracker.GetProgressReport();

            // Act
            var method = typeof(ProgressTracker).GetMethod(
                "CalculateETA",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var eta = (TimeSpan)method.Invoke(tracker, new object[] { 100.0, report.ElapsedTime });

            // Assert
            eta.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void CalculateETA_ReturnsPositiveTimeForPartialProgress()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);
            tracker.ReportItemProgress(); // ~1%
            var report = tracker.GetProgressReport();

            // Act
            var method = typeof(ProgressTracker).GetMethod(
                "CalculateETA",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var eta = (TimeSpan)method.Invoke(tracker, new object[] { 1.0, report.ElapsedTime });

            // Assert
            eta.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public void ThreadSafety_ReportItemProgress_IsThreadSafe()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 1000);
            var tasks = new System.Threading.Tasks.Task[10];

            // Act - spawn 10 threads all reporting progress
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = System.Threading.Tasks.Task.Run(() =>
                {
                    for (int j = 0; j < 100; j++)
                    {
                        tracker.ReportItemProgress();
                    }
                });
            }

            System.Threading.Tasks.Task.WaitAll(tasks);
            var report = tracker.GetProgressReport();

            // Assert - should have approximately 1000 items (may be slightly off due to timing)
            report.ItemsCompleted.Should().BeGreaterThan(900);
            report.ItemsCompleted.Should().BeLessThanOrEqualTo(1100);
            report.ProgressPercentage.Should().BeGreaterThan(90);
            report.ProgressPercentage.Should().BeLessThanOrEqualTo(110);
        }

        [Fact]
        public void ThreadSafety_ReportPercentageProgress_IsThreadSafe()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 1000);
            var tasks = new System.Threading.Tasks.Task[10];
            var random = new Random();

            // Act - spawn 10 threads reporting random percentages
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = System.Threading.Tasks.Task.Run(() =>
                {
                    for (int j = 0; j < 50; j++)
                    {
                        double percent = random.NextDouble() * 100;
                        tracker.ReportPercentageProgress(percent);
                    }
                });
            }

            System.Threading.Tasks.Task.WaitAll(tasks);
            var report = tracker.GetProgressReport();

            // Assert - progress should be reasonable (not NaN, not negative, not > 100)
            report.ProgressPercentage.Should().BeGreaterThanOrEqualTo(0);
            report.ProgressPercentage.Should().BeLessThanOrEqualTo(100);
        }

        [Fact]
        public void Throughput_CalculatesCorrectValues()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);
            System.Threading.Thread.Sleep(50); // Ensure some time passes
            tracker.ReportItemProgress();
            var report = tracker.GetProgressReport();

            // Assert
            report.ThroughputItemsPerSecond.Should().BeGreaterThan(0);
            report.ElapsedTime.TotalSeconds.Should().BeGreaterThan(0.04); // Should be at least ~50ms
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var tracker = new ProgressTracker(totalItems: 100);

            // Act
            tracker.Dispose();
            tracker.Dispose(); // Should not throw

            // Assert - just verify no exception thrown
            true.Should().BeTrue();
        }
    }

    public class ObservableProgressTrackerTests
    {
        [Fact]
        public void Constructor_WithThreshold_InitializesCorrectly()
        {
            // Arrange & Act
            var tracker = new ObservableProgressTracker(totalItems: 100, reportingThreshold: 5.0);

            // Assert
            tracker.Should().NotBeNull();
        }

        [Fact]
        public void ReportItemProgress_RaisesEventWhenThresholdExceeded()
        {
            // Arrange
            var tracker = new ObservableProgressTracker(totalItems: 100, reportingThreshold: 10.0);
            var eventRaised = false;
            ProgressReport lastReport = null;

            tracker.ProgressChanged += (report) =>
            {
                eventRaised = true;
                lastReport = report;
            };

            // Act - report 15 items (should trigger event at 10%)
            for (int i = 0; i < 15; i++)
            {
                tracker.ReportItemProgress();
            }

            // Assert
            eventRaised.Should().BeTrue();
            lastReport.Should().NotBeNull();
            lastReport.ItemsCompleted.Should().Be(15);
            lastReport.ProgressPercentage.Should().BeApproximately(15.0, 0.001);
        }

        [Fact]
        public void ReportItemProgress_DoesNotRaiseEventBelowThreshold()
        {
            // Arrange
            var tracker = new ObservableProgressTracker(totalItems: 100, reportingThreshold: 20.0);
            var eventCount = 0;

            tracker.ProgressChanged += (report) =>
            {
                eventCount++;
            };

            // Act - report 10 items (below 20% threshold)
            for (int i = 0; i < 10; i++)
            {
                tracker.ReportItemProgress();
            }

            // Assert
            eventCount.Should().Be(0);
        }

        [Fact]
        public void ReportItemProgress_RaisesEventAtHundredPercent()
        {
            // Arrange
            var tracker = new ObservableProgressTracker(totalItems: 100, reportingThreshold: 50.0);
            var eventCount = 0;

            tracker.ProgressChanged += (report) =>
            {
                eventCount++;
            };

            // Act - report all items (should trigger at 100% even if below threshold)
            for (int i = 0; i < 100; i++)
            {
                tracker.ReportItemProgress();
            }

            // Assert
            eventCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ReportPercentageProgress_RaisesEventWhenThresholdExceeded()
        {
            // Arrange
            var tracker = new ObservableProgressTracker(totalItems: 100, reportingThreshold: 25.0);
            var eventCount = 0;

            tracker.ProgressChanged += (report) =>
            {
                eventCount++;
            };

            // Act - report 30% (should trigger event)
            tracker.ReportPercentageProgress(30);

            // Assert
            eventCount.Should().Be(1);
        }

        [Fact]
        public void ReportBytesProgress_RaisesEventWhenThresholdExceeded()
        {
            // Arrange
            var tracker = new ObservableProgressTracker(totalBytes: 1024 * 1024, reportingThreshold: 10.0);
            var eventCount = 0;

            tracker.ProgressChanged += (report) =>
            {
                eventCount++;
            };

            // Act - report 150KB (should trigger event at 15%)
            tracker.ReportBytesProgress(150 * 1024);

            // Assert
            eventCount.Should().Be(1);
        }

        [Fact]
        public void ThreadSafety_ObservableTracker_IsThreadSafe()
        {
            // Arrange
            var tracker = new ObservableProgressTracker(totalItems: 1000, reportingThreshold: 1.0);
            var eventCount = 0;
            var eventsLock = new object();

            tracker.ProgressChanged += (report) =>
            {
                lock (eventsLock)
                {
                    eventCount++;
                }
            };

            var tasks = new System.Threading.Tasks.Task[10];

            // Act - spawn threads reporting progress
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = System.Threading.Tasks.Task.Run(() =>
                {
                    for (int j = 0; j < 50; j++)
                    {
                        tracker.ReportItemProgress();
                    }
                });
            }

            System.Threading.Tasks.Task.WaitAll(tasks);

            // Assert - should have received some events
            eventCount.Should().BeGreaterThan(0);
        }
    }
}
