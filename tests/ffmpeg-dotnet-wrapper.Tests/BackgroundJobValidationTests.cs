// SPDX-License-Identifier: MIT
// Tests for BackgroundJobValidation
// -------------------------------------------------------------

using System;
using System.Collections.Generic;
using FFmpegDotnetWrapper.BackgroundJobs;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class BackgroundJobValidationTests
{
    private static BackgroundJob CreateValidJob()
    {
        // The exact constructor of BackgroundJob is not known; we assume a parameterless
        // constructor and public settable properties based on the validation logic.
        return new BackgroundJob
        {
            JobId = "job-123",
            JobName = "TestJob",
            State = JobState.Queued,
            ProgressPercentage = 0,
            StatusMessage = null, // allowed for Queued state
            CreatedAt = DateTime.UtcNow,
            StartedAt = null,
            CompletedAt = null,
            ErrorMessage = null,
            StackTrace = null,
            EstimatedTimeRemaining = null,
            Metadata = new Dictionary<string, object>()
        };
    }

    [Fact]
    public void Validate_ValidJob_ReturnsEmptyList()
    {
        var job = CreateValidJob();

        var result = BackgroundJobValidation.Validate(job);

        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_ValidJob_ReturnsTrue()
    {
        var job = CreateValidJob();

        var isValid = job.IsValid();

        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_ValidJob_DoesNotThrow()
    {
        var job = CreateValidJob();

        var exception = Record.Exception(() => job.EnsureValid());

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NullJob_ThrowsArgumentNullException()
    {
        BackgroundJob? job = null;

        Assert.Throws<ArgumentNullException>(() => BackgroundJobValidation.Validate(job!));
    }

    [Fact]
    public void IsValid_NullJob_ThrowsArgumentNullException()
    {
        BackgroundJob? job = null;

        Assert.Throws<ArgumentNullException>(() => job!.IsValid());
    }

    [Fact]
    public void EnsureValid_NullJob_ThrowsArgumentNullException()
    {
        BackgroundJob? job = null;

        Assert.Throws<ArgumentNullException>(() => job!.EnsureValid());
    }

    [Fact]
    public void Validate_InvalidJob_ReturnsExpectedErrors()
    {
        var job = new BackgroundJob
        {
            JobId = "",                     // invalid
            JobName = "   ",                // invalid
            State = (JobState)999,          // invalid enum
            ProgressPercentage = -5,       // out of range
            StatusMessage = null,          // invalid because state != Queued
            CreatedAt = default,           // invalid
            StartedAt = DateTime.UtcNow.AddHours(-2), // earlier than CreatedAt (default)
            CompletedAt = DateTime.UtcNow.AddHours(-3), // earlier than StartedAt
            ErrorMessage = "some error",   // should be null because state is not Failed
            StackTrace = "trace",          // should be null because state is not Failed
            EstimatedTimeRemaining = TimeSpan.FromMinutes(-1), // negative
            Metadata = null                // null metadata
        };

        var errors = BackgroundJobValidation.Validate(job);

        // We expect multiple errors; verify a few representative ones.
        Assert.Contains("JobId cannot be null or whitespace.", errors);
        Assert.Contains("JobName cannot be null or whitespace.", errors);
        Assert.Contains("State has an invalid value.", errors);
        Assert.Contains("ProgressPercentage must be between 0 and 100 inclusive.", errors);
        Assert.Contains("StatusMessage cannot be null or whitespace for non-queued jobs.", errors);
        Assert.Contains("CreatedAt cannot be the default DateTime value.", errors);
        Assert.Contains("EstimatedTimeRemaining cannot be negative.", errors);
        Assert.Contains("Metadata cannot be null.", errors);
    }

    [Fact]
    public void EnsureValid_InvalidJob_ThrowsArgumentExceptionWithMessages()
    {
        var job = new BackgroundJob
        {
            JobId = null,
            JobName = null,
            State = (JobState)0, // assume this is not a defined enum value
            ProgressPercentage = 101,
            StatusMessage = null,
            CreatedAt = default,
            Metadata = null
        };

        var ex = Assert.Throws<ArgumentException>(() => job.EnsureValid());

        // The message should contain the validation errors.
        Assert.Contains("JobId cannot be null or whitespace.", ex.Message);
        Assert.Contains("JobName cannot be null or whitespace.", ex.Message);
        Assert.Contains("State has an invalid value.", ex.Message);
        Assert.Contains("ProgressPercentage must be between 0 and 100 inclusive.", ex.Message);
        Assert.Contains("CreatedAt cannot be the default DateTime value.", ex.Message);
        Assert.Contains("Metadata cannot be null.", ex.Message);
    }
}
