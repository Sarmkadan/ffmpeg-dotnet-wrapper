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

    #region FilterPending

    [Fact]
    public void FilterPending_ReturnsJobsWithNoDueDate()
    {
        // Arrange
        var jobs = new List<QueuedJob>
        {
            new QueuedJob { DueAt = null },
            new QueuedJob { DueAt = DateTime.UtcNow.AddHours(1) }, // Future
            new QueuedJob { DueAt = DateTime.UtcNow.AddHours(-1) } // Past
        };

        // Act
        var result = jobs.FilterPending().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Null(result[0].DueAt);
        Assert.True(result[1].DueAt <= DateTime.UtcNow);
    }

    [Fact]
    public void FilterPending_ReturnsEmpty_WhenAllJobsHaveFutureDueDate()
    {
        // Arrange
        var jobs = new List<QueuedJob>
        {
            new QueuedJob { DueAt = DateTime.UtcNow.AddHours(1) },
            new QueuedJob { DueAt = DateTime.UtcNow.AddHours(2) }
        };

        // Act
        var result = jobs.FilterPending().ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FilterPending_ReturnsAll_WhenAllJobsHaveNoDueDate()
    {
        // Arrange
        var jobs = new List<QueuedJob>
        {
            new QueuedJob { DueAt = null },
            new QueuedJob { DueAt = null }
        };

        // Act
        var result = jobs.FilterPending().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, j => Assert.Null(j.DueAt));
    }

    [Fact]
    public void FilterPending_ThrowsArgumentNullException_WhenJobsIsNull()
    {
        // Arrange
        IEnumerable<QueuedJob> jobs = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => jobs.FilterPending());
    }

    #endregion

    #region GroupByPriority

    [Fact]
    public void GroupByPriority_GroupsJobsByPriorityLevel()
    {
        // Arrange
        var jobs = new List<QueuedJob>
        {
            new QueuedJob { Priority = 1 },
            new QueuedJob { Priority = 1 },
            new QueuedJob { Priority = 5 },
            new QueuedJob { Priority = 10 }
        };

        // Act
        var result = jobs.GroupByPriority().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].Key);
        Assert.Equal(2, result[0].Count());
        Assert.Equal(5, result[1].Key);
        Assert.Equal(1, result[1].Count());
        Assert.Equal(10, result[2].Key);
        Assert.Equal(1, result[2].Count());
    }

    [Fact]
    public void GroupByPriority_ReturnsEmpty_WhenNoJobs()
    {
        // Arrange
        var jobs = new List<QueuedJob>();

        // Act
        var result = jobs.GroupByPriority().ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GroupByPriority_OrdersGroupsByPriorityAscending()
    {
        // Arrange
        var jobs = new List<QueuedJob>
        {
            new QueuedJob { Priority = 10 },
            new QueuedJob { Priority = 1 },
            new QueuedJob { Priority = 5 }
        };

        // Act
        var result = jobs.GroupByPriority().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].Key);
        Assert.Equal(5, result[1].Key);
        Assert.Equal(10, result[2].Key);
    }

    [Fact]
    public void GroupByPriority_ThrowsArgumentNullException_WhenJobsIsNull()
    {
        // Arrange
        IEnumerable<QueuedJob> jobs = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => jobs.GroupByPriority());
    }

    #endregion
}
