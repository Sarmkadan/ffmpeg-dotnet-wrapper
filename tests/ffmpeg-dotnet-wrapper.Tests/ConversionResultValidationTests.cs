// =============================================================================
// Unit tests for ConversionResultValidation
// ===================================================================

using System;
using FFmpegDotnetWrapper.Models;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ConversionResultValidation"/> class.
/// </summary>
public class ConversionResultValidationTests
{
    private readonly string _tempFilePath;

    public ConversionResultValidationTests()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"test-media-{Guid.NewGuid()}.mp4");
        File.WriteAllText(_tempFilePath, "dummy video content");
    }

    #region Validate Method Tests

    [Fact]
    public void Validate_WithNullConversionResult_ThrowsArgumentNullException()
    {
        ConversionResult? nullResult = null;

        var exception = Assert.Throws<ArgumentNullException>(() => nullResult!.Validate());
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Validate_WithValidSuccessfulConversion_ReturnsEmptyList()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile()
            {
                Duration = TimeSpan.FromSeconds(10),
                Width = 1920,
                Height = 1080,
                FrameRate = 30.0,
                Bitrate = 5000000,
                VideoCodec = "h264",
                AudioCodec = "aac",
                AudioSampleRate = 44100,
                AudioChannels = 2
            },
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object> { { "bitrate", 5000000 } },
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Empty(validationErrors);
    }

    [Fact]
    public void Validate_WithValidFailedConversion_ReturnsEmptyList()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = false,
            ErrorMessage = "Conversion failed due to invalid codec",
            ErrorOutput = "Error: Invalid codec combination",
            ExitCode = 1,
            Duration = TimeSpan.FromSeconds(5),
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Empty(validationErrors);
    }

    [Fact]
    public void Validate_WithInvalidId_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = "invalid-id",
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.Id must be a valid GUID", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithEmptyId_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = "",
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.Id must not be null, empty, or whitespace", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithNullOutputFilePathOnSuccess_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = null,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.OutputFilePath must not be null or empty when IsSuccess is true", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithRelativeOutputFilePath_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = "relative/path/file.mp4",
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.OutputFilePath must be an absolute path when provided", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithNullOutputMediaOnSuccess_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = null,
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.OutputMedia must not be null when IsSuccess is true", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithNonExistentOutputMediaFilePath_ReturnsValidationError()
    {
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent-file.mp4");
        var mediaFile = new MediaFile(nonExistentFile);

        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = mediaFile,
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.OutputMedia.FilePath references non-existent file", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithZeroFileSize_ReturnsValidationError()
    {
        var mediaFile = new MediaFile(_tempFilePath);
        // FileSize is read-only and set by the constructor based on actual file size
        // Create a file with zero bytes to test this case
        var zeroByteFile = Path.Combine(Path.GetTempPath(), $"zero-byte-{Guid.NewGuid()}.mp4");
        File.WriteAllText(zeroByteFile, "");
        var zeroByteMediaFile = new MediaFile(zeroByteFile);

        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = zeroByteMediaFile,
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.OutputMedia.FileSize must be greater than zero", validationErrors[0]);

        // Cleanup
        File.Delete(zeroByteFile);
    }

    [Fact]
    public void Validate_WithNegativeDuration_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(-5),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.Duration must not be negative", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithErrorMessageOnSuccess_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = "Some error occurred",
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult has IsSuccess=true but contains an ErrorMessage", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithNullErrorMessageOnFailure_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = false,
            ErrorMessage = null,
            ErrorOutput = "Error output",
            ExitCode = 1,
            Duration = TimeSpan.FromSeconds(5),
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult has IsSuccess=false but ErrorMessage is null or empty", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithEmptyErrorMessageOnFailure_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = false,
            ErrorMessage = "",
            ErrorOutput = "Error output",
            ExitCode = 1,
            Duration = TimeSpan.FromSeconds(5),
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult has IsSuccess=false but ErrorMessage is null or empty", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithNullMetrics_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = null,
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.Metrics must not be null", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithTooManyMetrics_ReturnsValidationError()
    {
        var metrics = new Dictionary<string, object>();
        for (int i = 0; i < 1001; i++)
        {
            metrics.Add($"metric_{i}", i);
        }

        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = metrics,
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.Metrics contains too many entries", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithDefaultCreatedAt_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CreatedAt = default,
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.CreatedAt must be set to a non-default DateTime", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithFutureCreatedAt_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow.AddMinutes(10),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.CreatedAt cannot be in the future", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithDefaultCompletedAtOnSuccess_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = default
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.CompletedAt must be set when IsSuccess is true", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithCompletedAtBeforeCreatedAt_ReturnsValidationError()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-10);
        var completedAt = DateTime.UtcNow.AddMinutes(-15);

        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CreatedAt = createdAt,
            CompletedAt = completedAt
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.CompletedAt cannot be earlier than CreatedAt", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithNullFFmpegOutputOnSuccess_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = null,
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.FFmpegOutput must not be null or empty when IsSuccess is true", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithEmptyFFmpegOutputOnSuccess_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.FFmpegOutput must not be null or empty when IsSuccess is true", validationErrors[0]);
    }

    [Fact]
    public void Validate_WithWhitespaceFFmpegOutputOnSuccess_ReturnsValidationError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "   ",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Single(validationErrors);
        Assert.Contains("ConversionResult.FFmpegOutput must not be null or empty when IsSuccess is true", validationErrors[0]);
    }

    #endregion

    #region IsValid Method Tests

    [Fact]
    public void IsValid_WithNullConversionResult_ThrowsArgumentNullException()
    {
        ConversionResult? nullResult = null;

        var exception = Assert.Throws<ArgumentNullException>(() => nullResult!.IsValid());
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void IsValid_WithValidConversionResult_ReturnsTrue()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var isValid = result.IsValid();

        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WithInvalidConversionResult_ReturnsFalse()
    {
        var result = new ConversionResult
        {
            Id = "invalid-id",
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var isValid = result.IsValid();

        Assert.False(isValid);
    }

    #endregion

    #region EnsureValid Method Tests

    [Fact]
    public void EnsureValid_WithNullConversionResult_ThrowsArgumentNullException()
    {
        ConversionResult? nullResult = null;

        var exception = Assert.Throws<ArgumentNullException>(() => nullResult!.EnsureValid());
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void EnsureValid_WithValidConversionResult_DoesNotThrow()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        // Should not throw
        result.EnsureValid();
    }

    [Fact]
    public void EnsureValid_WithInvalidConversionResult_ThrowsArgumentException()
    {
        var result = new ConversionResult
        {
            Id = "invalid-id",
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var exception = Assert.Throws<ArgumentException>(() => result.EnsureValid());
        Assert.Equal("value", exception.ParamName);
        Assert.Contains("ConversionResult is invalid", exception.Message);
        Assert.Contains("ConversionResult.Id must be a valid GUID", exception.Message);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Validate_WithMultipleValidationErrors_ReturnsAllErrors()
    {
        var result = new ConversionResult
        {
            Id = "invalid-id",
            IsSuccess = true,
            OutputFilePath = "relative/path",
            OutputMedia = null,
            Duration = TimeSpan.FromSeconds(-5),
            ErrorMessage = "Some error",
            FFmpegOutput = null,
            Metrics = null,
            CreatedAt = default,
            CompletedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        var validationErrors = result.Validate();

        Assert.Equal(6, validationErrors.Count);
    }

    [Fact]
    public void Validate_WithValidMinimalConversionResult_ReturnsEmptyList()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = false,
            ErrorMessage = "Minimal error message",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Empty(validationErrors);
    }

    [Fact]
    public void Validate_WithWarningMessage_DoesNotAddError()
    {
        var result = new ConversionResult
        {
            Id = Guid.NewGuid().ToString(),
            IsSuccess = true,
            OutputFilePath = _tempFilePath,
            OutputMedia = new MediaFile(),
            Duration = TimeSpan.FromSeconds(15),
            ErrorMessage = null,
            WarningMessage = "This is a warning",
            FFmpegOutput = "FFmpeg output log",
            Metrics = new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        var validationErrors = result.Validate();

        Assert.Empty(validationErrors);
    }

    #endregion
}