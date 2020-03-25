using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Unit tests for the <see cref="TranscodeService"/> class.
/// </summary>
public class TranscodeServiceTests
{
    private readonly Mock<IFFmpegService> _ffmpegServiceMock;
    private readonly Mock<ILogger<TranscodeService>> _loggerMock;
    private readonly TranscodeService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodeServiceTests"/> class.
    /// Sets up the mocked dependencies and the system under test.
    /// </summary>
    public TranscodeServiceTests()
    {
        _ffmpegServiceMock = new Mock<IFFmpegService>();
        _loggerMock = new Mock<ILogger<TranscodeService>>();
        _service = new TranscodeService(_ffmpegServiceMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="TranscodeService.TranscodeToWebAsync"/> calls the FFmpeg service
    /// with the correct settings, specifically ensuring the video codec is set to H264.
    /// </summary>
    [Fact]
    public async Task TranscodeToWebAsync_ShouldCallFFmpegService_WithCorrectSettings()
    {
        // Arrange
        var input = new MediaFile { Id = "test" };
        var output = "/path/to/output.mp4";
        _ffmpegServiceMock.Setup(s => s.TranscodeAsync(It.IsAny<MediaFile>(), output, It.IsAny<TranscodeSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConversionResult { IsSuccess = true });

        // Act
        var result = await _service.TranscodeToWebAsync(input, output);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _ffmpegServiceMock.Verify(s => s.TranscodeAsync(
            input,
            output,
            It.Is<TranscodeSettings>(settings => settings.VideoCodec == VideoCodec.H264),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Ensures that when the underlying FFmpeg service throws an <see cref="FFmpegException"/>,
    /// the <see cref="TranscodeService.TranscodeToWebAsync"/> method propagates the exception correctly.
    /// </summary>
    [Fact]
    public async Task TranscodeToWebAsync_ShouldPropagateException_WhenFFmpegServiceThrows()
    {
        // Arrange
        var input = new MediaFile { Id = "test" };
        var output = "/path/to/output.mp4";
        _ffmpegServiceMock.Setup(s => s.TranscodeAsync(It.IsAny<MediaFile>(), output, It.IsAny<TranscodeSettings>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FFmpegException("FFmpeg failed"));

        // Act
        Func<Task> act = () => _service.TranscodeToWebAsync(input, output);

        // Assert
        await act.Should().ThrowAsync<FFmpegException>().WithMessage("FFmpeg failed");
    }

    /// <summary>
    /// Tests that <see cref="TranscodeService.TranscodeWithBitrateAsync"/> throws an
    /// <see cref="InvalidOperationConfigurationException"/> when the provided bitrate is out of the valid range (e.g., 0).
    /// </summary>
    [Fact]
    public async Task TranscodeWithBitrateAsync_ShouldThrowException_WhenBitrateIsOutOfRange()
    {
        // Arrange
        var input = new MediaFile { Id = "test" };
        var output = "/path/to/output.mp4";

        // Act
        // TranscodeSettings.Validate() is called inside TranscodeWithBitrateAsync
        Func<Task> act = () => _service.TranscodeWithBitrateAsync(input, output, 0, 128);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationConfigurationException>();
    }

    /// <summary>
    /// Tests that <see cref="TranscodeService.ResizeVideoAsync"/> throws an
    /// <see cref="InvalidOperationConfigurationException"/> when the target dimensions are zero.
    /// </summary>
    [Fact]
    public async Task ResizeVideoAsync_ShouldThrowException_WhenDimensionsAreZero()
    {
        // Arrange
        var input = new MediaFile { Id = "test" };
        var output = "/path/to/output.mp4";

        // Act
        Func<Task> act = () => _service.ResizeVideoAsync(input, output, 0, 0);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationConfigurationException>()
            .WithMessage("Width and height must be greater than 0");
    }

    /// <summary>
    /// Tests that <see cref="TranscodeService.ExtractAudioAsync"/> throws an
    /// <see cref="InvalidMediaFileException"/> when the input file is not a valid video file.
    /// </summary>
    [Fact]
    public async Task ExtractAudioAsync_ShouldThrowException_WhenInputIsNotVideo()
    {
        // Arrange
        // Width/Height are null, so IsVideo() is false, ValidateAsVideo() throws
        var input = new MediaFile { Id = "test" };
        var output = "/path/to/output.mp3";

        // Act
        Func<Task> act = () => _service.ExtractAudioAsync(input, output);

        // Assert
        await act.Should().ThrowAsync<InvalidMediaFileException>();
    }
}
