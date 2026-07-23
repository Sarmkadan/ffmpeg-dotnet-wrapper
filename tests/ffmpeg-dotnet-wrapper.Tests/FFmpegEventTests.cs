using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Events;
using FluentAssertions;
using Moq;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class FFmpegEventTests
{
    [Fact]
    public void OperationStartedEvent_Properties_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var @event = new OperationStartedEvent();

        // Assert
        @event.EventId.Should().NotBeNullOrEmpty();
        @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.InputFile.Should().BeEmpty();
        @event.OutputFile.Should().BeEmpty();
        @event.OperationType.Should().BeEmpty();
        @event.Metadata.Should().BeNull();
    }

    [Fact]
    public void OperationStartedEvent_WithParameters_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var inputFile = "input.mp4";
        var outputFile = "output.mp4";
        var operationType = "transcode";
        var metadata = new Dictionary<string, object> { { "bitrate", "1080p" }, { "codec", "h264" } };

        // Act
        var @event = new OperationStartedEvent
        {
            InputFile = inputFile,
            OutputFile = outputFile,
            OperationType = operationType,
            Metadata = metadata
        };

        // Assert
        @event.InputFile.Should().Be(inputFile);
        @event.OutputFile.Should().Be(outputFile);
        @event.OperationType.Should().Be(operationType);
        @event.Metadata.Should().BeEquivalentTo(metadata);
    }


    [Fact]
    public void OperationCompletedEvent_Properties_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var @event = new OperationCompletedEvent();

        // Assert
        @event.EventId.Should().NotBeNullOrEmpty();
        @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.InputFile.Should().BeEmpty();
        @event.OutputFile.Should().BeEmpty();
        @event.OperationType.Should().BeEmpty();
        @event.Duration.Should().Be(TimeSpan.Zero);
        @event.OutputFileSize.Should().Be(0);
    }

    [Fact]
    public void OperationCompletedEvent_WithParameters_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var inputFile = "input.mp4";
        var outputFile = "output.mp4";
        var operationType = "transcode";
        var duration = TimeSpan.FromSeconds(45);
        var fileSize = 1024 * 1024L; // 1MB

        // Act
        var @event = new OperationCompletedEvent
        {
            InputFile = inputFile,
            OutputFile = outputFile,
            OperationType = operationType,
            Duration = duration,
            OutputFileSize = fileSize
        };

        // Assert
        @event.InputFile.Should().Be(inputFile);
        @event.OutputFile.Should().Be(outputFile);
        @event.OperationType.Should().Be(operationType);
        @event.Duration.Should().Be(duration);
        @event.OutputFileSize.Should().Be(fileSize);
    }

    [Fact]
    public void OperationFailedEvent_Properties_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var @event = new OperationFailedEvent();

        // Assert
        @event.EventId.Should().NotBeNullOrEmpty();
        @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.InputFile.Should().BeEmpty();
        @event.OperationType.Should().BeEmpty();
        @event.ErrorMessage.Should().BeEmpty();
        @event.ErrorCode.Should().BeNull();
        @event.StackTrace.Should().BeNull();
    }

    [Fact]
    public void OperationFailedEvent_WithParameters_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var inputFile = "input.mp4";
        var operationType = "transcode";
        var errorMessage = "FFmpeg failed to process file";
        var errorCode = "FFMPEG_ERROR_001";
        var stackTrace = "at FFmpeg.Core.Transcode.Run()";

        // Act
        var @event = new OperationFailedEvent
        {
            InputFile = inputFile,
            OperationType = operationType,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            StackTrace = stackTrace
        };

        // Assert
        @event.InputFile.Should().Be(inputFile);
        @event.OperationType.Should().Be(operationType);
        @event.ErrorMessage.Should().Be(errorMessage);
        @event.ErrorCode.Should().Be(errorCode);
        @event.StackTrace.Should().Be(stackTrace);
    }


    [Fact]
    public void ProgressReportedEvent_Properties_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var @event = new ProgressReportedEvent();

        // Assert
        @event.EventId.Should().NotBeNullOrEmpty();
        @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.OperationType.Should().BeEmpty();
        @event.ProgressPercentage.Should().Be(0);
        @event.ElapsedTime.Should().Be(TimeSpan.Zero);
        @event.StatusMessage.Should().BeNull();
    }

    [Fact]
    public void ProgressReportedEvent_WithParameters_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var operationType = "transcode";
        var progress = 75.5;
        var elapsedTime = TimeSpan.FromSeconds(30);
        var statusMessage = "Processing frame 1500/2000";

        // Act
        var @event = new ProgressReportedEvent
        {
            OperationType = operationType,
            ProgressPercentage = progress,
            ElapsedTime = elapsedTime,
            StatusMessage = statusMessage
        };

        // Assert
        @event.OperationType.Should().Be(operationType);
        @event.ProgressPercentage.Should().Be(progress);
        @event.ElapsedTime.Should().Be(elapsedTime);
        @event.StatusMessage.Should().Be(statusMessage);
    }


    [Fact]
    public void FFmpegEvent_BaseProperties_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var @event = new OperationStartedEvent();

        // Assert
        @event.EventId.Should().NotBeNullOrEmpty();
        @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.CorrelationId.Should().BeNull();
        @event.Source.Should().BeNull();
    }

    [Fact]
    public void FFmpegEvent_WithCorrelationIdAndSource_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var source = "TranscodeService";

        // Act
        var @event = new OperationStartedEvent
        {
            CorrelationId = correlationId,
            Source = source
        };

        // Assert
        @event.CorrelationId.Should().Be(correlationId);
        @event.Source.Should().Be(source);
    }

}