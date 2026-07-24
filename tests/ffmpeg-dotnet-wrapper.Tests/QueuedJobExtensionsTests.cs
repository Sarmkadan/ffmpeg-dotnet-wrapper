using System;
using FFmpegDotnetWrapper.BackgroundJobs;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class QueuedJobExtensionsTests
{
    #region GetStatusString

    [Fact]
    public void GetStatusString_ReturnsNoDueDate_WhenDueAtIsNull()
    {
        // Arrange
        var job = new QueuedJob { DueAt = null };

        // Act
        var result = job.GetStatusString();

        // Assert
        Assert.Equal("No due date", result);
    }

    [Fact]
    public void GetStatusString_ReturnsFormattedDate_WhenDueAtHasValue()
    {
        // Arrange
        var due = new DateTime(2023, 01, 02, 03, 04, 05, DateTimeKind.Utc);
        var job = new QueuedJob { DueAt = due };

        // Act
        var result = job.GetStatusString();

        // Assert
        var expected = $"Due at {due.ToString("o", System.Globalization.CultureInfo.InvariantCulture)}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetStatusString_ThrowsArgumentNullException_WhenJobIsNull()
    {
        // Arrange
        QueuedJob? job = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => job!.GetStatusString());
    }

    #endregion

    #region IsOverdue

    [Fact]
    public void IsOverdue_ReturnsTrue_WhenDueAtIsInPast()
    {
        // Arrange
        var past = DateTime.UtcNow.AddHours(-1);
        var job = new QueuedJob { DueAt = past };

        // Act
        var result = job.IsOverdue();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOverdue_ReturnsFalse_WhenDueAtIsInFuture()
    {
        // Arrange
        var future = DateTime.UtcNow.AddHours(1);
        var job = new QueuedJob { DueAt = future };

        // Act
        var result = job.IsOverdue();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOverdue_ReturnsFalse_WhenDueAtIsNull()
    {
        // Arrange
        var job = new QueuedJob { DueAt = null };

        // Act
        var result = job.IsOverdue();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOverdue_ThrowsArgumentNullException_WhenJobIsNull()
    {
        // Arrange
        QueuedJob? job = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => job!.IsOverdue());
    }

    #endregion

    #region GetRetryInfoString

    [Fact]
    public void GetRetryInfoString_ReturnsCorrectFormat()
    {
        // Arrange
        var job = new QueuedJob { RetryCount = 2, MaxRetries = 5 };

        // Act
        var result = job.GetRetryInfoString();

        // Assert
        Assert.Equal("Retried 2 times out of 5", result);
    }

    [Fact]
    public void GetRetryInfoString_ThrowsArgumentNullException_WhenJobIsNull()
    {
        // Arrange
        QueuedJob? job = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => job!.GetRetryInfoString());
    }

    #endregion

    #region HasMaxRetries

    [Fact]
    public void HasMaxRetries_ReturnsTrue_WhenRetryCountEqualsMax()
    {
        // Arrange
        var job = new QueuedJob { RetryCount = 3, MaxRetries = 3 };

        // Act
        var result = job.HasMaxRetries();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasMaxRetries_ReturnsTrue_WhenRetryCountExceedsMax()
    {
        // Arrange
        var job = new QueuedJob { RetryCount = 5, MaxRetries = 3 };

        // Act
        var result = job.HasMaxRetries();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasMaxRetries_ReturnsFalse_WhenRetryCountBelowMax()
    {
        // Arrange
        var job = new QueuedJob { RetryCount = 1, MaxRetries = 4 };

        // Act
        var result = job.HasMaxRetries();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasMaxRetries_ThrowsArgumentOutOfRangeException_WhenMaxRetriesIsNegative()
    {
        // Arrange
        var job = new QueuedJob { RetryCount = 0, MaxRetries = -1 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => job.HasMaxRetries());
    }

    [Fact]
    public void HasMaxRetries_ThrowsArgumentNullException_WhenJobIsNull()
    {
        // Arrange
        QueuedJob? job = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => job!.HasMaxRetries());
    }

    #endregion
}
