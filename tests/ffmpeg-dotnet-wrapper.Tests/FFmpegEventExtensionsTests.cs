using System;
using System.Collections.Generic;
using FFmpegDotnetWrapper.Events;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class FFmpegEventExtensionsTests
{
    [Fact]
    public void IsSuccess_ShouldIdentifySuccessAndFailureEvents()
    {
        // Success events
        new OperationStartedEvent().IsSuccess().Should().BeTrue();
        new OperationCompletedEvent().IsSuccess().Should().BeTrue();
        new ProgressReportedEvent().IsSuccess().Should().BeTrue();

        // Failure event
        new OperationFailedEvent().IsSuccess().Should().BeFalse();
    }

    [Fact]
    public void IsFailure_ShouldIdentifyFailureEvent()
    {
        // Failure event
        new OperationFailedEvent().IsFailure().Should().BeTrue();

        // Non‑failure events
        new OperationStartedEvent().IsFailure().Should().BeFalse();
        new OperationCompletedEvent().IsFailure().Should().BeFalse();
        new ProgressReportedEvent().IsFailure().Should().BeFalse();
    }

    [Fact]
    public void GetOperationType_ShouldReturnSetValueOrEmpty()
    {
        var started = new OperationStartedEvent { OperationType = "transcode" };
        var completed = new OperationCompletedEvent { OperationType = "encode" };
        var failed = new OperationFailedEvent { OperationType = "decode" };
        var progress = new ProgressReportedEvent { OperationType = "merge" };
        var unknown = new OperationStartedEvent(); // no operation type set

        started.GetOperationType().Should().Be("transcode");
        completed.GetOperationType().Should().Be("encode");
        failed.GetOperationType().Should().Be("decode");
        progress.GetOperationType().Should().Be("merge");
        unknown.GetOperationType().Should().BeEmpty();
    }

    [Fact]
    public void GetInputFile_ShouldReturnValueOrNullWhenEmpty()
    {
        var withInput = new OperationStartedEvent { InputFile = "input.mp4" };
        var emptyInput = new OperationStartedEvent { InputFile = string.Empty };
        var nullInput = new OperationStartedEvent { InputFile = null };

        withInput.GetInputFile().Should().Be("input.mp4");
        emptyInput.GetInputFile().Should().BeNull();
        nullInput.GetInputFile().Should().BeNull();
    }

    [Fact]
    public void GetOutputFile_ShouldReturnValueOrNullWhenEmpty()
    {
        var started = new OperationStartedEvent { OutputFile = "out1.mp4" };
        var completed = new OperationCompletedEvent { OutputFile = "out2.mp4" };
        var empty = new OperationCompletedEvent { OutputFile = "" };
        var nullVal = new OperationStartedEvent { OutputFile = null };

        started.GetOutputFile().Should().Be("out1.mp4");
        completed.GetOutputFile().Should().Be("out2.mp4");
        empty.GetOutputFile().Should().BeNull();
        nullVal.GetOutputFile().Should().BeNull();
    }

    [Fact]
    public void GetErrorMessage_ShouldReturnErrorMessageOrNullWhenEmpty()
    {
        var failed = new OperationFailedEvent { ErrorMessage = "boom!" };
        var empty = new OperationFailedEvent { ErrorMessage = "" };
        var nullMsg = new OperationFailedEvent { ErrorMessage = null };

        failed.GetErrorMessage().Should().Be("boom!");
        empty.GetErrorMessage().Should().BeNull();
        nullMsg.GetErrorMessage().Should().BeNull();
    }

    [Fact]
    public void GetProgressPercentage_ShouldReturnValueOrNull()
    {
        var progress = new ProgressReportedEvent { ProgressPercentage = 73.5 };
        var noProgress = new ProgressReportedEvent();

        progress.GetProgressPercentage().Should().Be(73.5);
        noProgress.GetProgressPercentage().Should().BeNull();
    }

    [Fact]
    public void GetDuration_ShouldReturnDurationOrElapsedTimeOrNull()
    {
        var completed = new OperationCompletedEvent { Duration = TimeSpan.FromSeconds(42) };
        var progress = new ProgressReportedEvent { ElapsedTime = TimeSpan.FromSeconds(15) };
        var none = new OperationStartedEvent();

        completed.GetDuration().Should().Be(TimeSpan.FromSeconds(42));
        progress.GetDuration().Should().Be(TimeSpan.FromSeconds(15));
        none.GetDuration().Should().BeNull();
    }

    [Fact]
    public void GetOutputFileSize_ShouldReturnSizeWhenPositiveOrNull()
    {
        var completedPositive = new OperationCompletedEvent { OutputFileSize = 1024 };
        var completedZero = new OperationCompletedEvent { OutputFileSize = 0 };
        var completedNegative = new OperationCompletedEvent { OutputFileSize = -10 };
        var other = new OperationStartedEvent();

        completedPositive.GetOutputFileSize().Should().Be(1024);
        completedZero.GetOutputFileSize().Should().BeNull();
        completedNegative.GetOutputFileSize().Should().BeNull();
        other.GetOutputFileSize().Should().BeNull();
    }

    [Fact]
    public void GetErrorCode_ShouldReturnErrorCodeOrNullWhenEmpty()
    {
        var failed = new OperationFailedEvent { ErrorCode = "ERR123" };
        var empty = new OperationFailedEvent { ErrorCode = "" };
        var nullCode = new OperationFailedEvent { ErrorCode = null };

        failed.GetErrorCode().Should().Be("ERR123");
        empty.GetErrorCode().Should().BeNull();
        nullCode.GetErrorCode().Should().BeNull();
    }

    [Fact]
    public void NullArgument_ShouldThrowArgumentNullException_ForAllExtensionMethods()
    {
        // Each method should throw ArgumentNullException when the source event is null.
        Action actIsSuccess = () => ((FFmpegEvent)null!).IsSuccess();
        Action actIsFailure = () => ((FFmpegEvent)null!).IsFailure();
        Action actGetOperationType = () => ((FFmpegEvent)null!).GetOperationType();
        Action actGetInputFile = () => ((FFmpegEvent)null!).GetInputFile();
        Action actGetOutputFile = () => ((FFmpegEvent)null!).GetOutputFile();
        Action actGetErrorMessage = () => ((FFmpegEvent)null!).GetErrorMessage();
        Action actGetProgressPercentage = () => ((FFmpegEvent)null!).GetProgressPercentage();
        Action actGetDuration = () => ((FFmpegEvent)null!).GetDuration();
        Action actGetOutputFileSize = () => ((FFmpegEvent)null!).GetOutputFileSize();
        Action actGetErrorCode = () => ((FFmpegEvent)null!).GetErrorCode();

        actIsSuccess.Should().Throw<ArgumentNullException>();
        actIsFailure.Should().Throw<ArgumentNullException>();
        actGetOperationType.Should().Throw<ArgumentNullException>();
        actGetInputFile.Should().Throw<ArgumentNullException>();
        actGetOutputFile.Should().Throw<ArgumentNullException>();
        actGetErrorMessage.Should().Throw<ArgumentNullException>();
        actGetProgressPercentage.Should().Throw<ArgumentNullException>();
        actGetDuration.Should().Throw<ArgumentNullException>();
        actGetOutputFileSize.Should().Throw<ArgumentNullException>();
        actGetErrorCode.Should().Throw<ArgumentNullException>();
    }
}
