namespace FFmpegDotnetWrapper.Tests;

using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using Xunit;

/// <summary>
/// Provides unit tests for the <see cref="ThumbnailService"/> class.
/// </summary>
public class ThumbnailServiceTests
{
    private readonly Mock<IFFmpegService> _ffmpeg = new();
    private readonly ThumbnailService _sut;
    private ThumbnailSettings? _captured;

    /// <summary>
    /// Wires a <see cref="ThumbnailService"/> to a mocked <see cref="IFFmpegService"/> that records the settings it receives.
    /// </summary>
    public ThumbnailServiceTests()
    {
        _ffmpeg
            .Setup(f => f.ExtractThumbnailsAsync(It.IsAny<MediaFile>(), It.IsAny<string>(), It.IsAny<ThumbnailSettings>(), It.IsAny<CancellationToken>()))
            .Callback<MediaFile, string, ThumbnailSettings, CancellationToken>((_, _, s, _) => _captured = s)
            .ReturnsAsync(new ThumbnailResult { IsSuccess = true, Thumbnails = { "frame.jpg" } });

        _sut = new ThumbnailService(_ffmpeg.Object, NullLogger<ThumbnailService>.Instance);
    }

    [Fact]
    public async Task ExtractSingleAsync_ShouldCaptureJpegAtFiveSeconds_WhenTimestampIsNull()
    {
        var media = new MediaFile();

        var result = await _sut.ExtractSingleAsync(media, "/out/frame.jpg");

        _captured!.Times.Should().Equal(TimeSpan.FromSeconds(5));
        _captured.Format.Should().Be(ThumbnailFormat.Jpeg);
        _ffmpeg.Verify(f => f.ExtractThumbnailsAsync(media, "/out/frame.jpg", It.IsAny<ThumbnailSettings>(), It.IsAny<CancellationToken>()), Times.Once);
        result.IsSuccess.Should().BeTrue();
        result.FirstThumbnail.Should().Be("frame.jpg");
    }

    [Fact]
    public async Task ExtractSingleAsync_ShouldThrowArgumentNullException_WhenInputMediaIsNull()
    {
        var act = () => _sut.ExtractSingleAsync(null!, "/out/frame.jpg");

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("inputMedia");
        _ffmpeg.Verify(f => f.ExtractThumbnailsAsync(It.IsAny<MediaFile>(), It.IsAny<string>(), It.IsAny<ThumbnailSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExtractStoryboardAsync_ShouldSpreadTimestampsEvenly_WhenDurationIsKnown()
    {
        var media = new MediaFile { Duration = TimeSpan.FromSeconds(100) };

        await _sut.ExtractStoryboardAsync(media, "/out/sb_%03d.jpg", count: 4, width: 320, height: -1);

        _captured!.Times.Should().Equal(
            TimeSpan.FromSeconds(20),
            TimeSpan.FromSeconds(40),
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(80));
        _captured.Count.Should().Be(4);
        _captured.Width.Should().Be(320);
        _captured.Height.Should().Be(-1);
    }

    [Fact]
    public async Task ExtractStoryboardAsync_ShouldLeaveTimesEmpty_WhenDurationIsUnknown()
    {
        await _sut.ExtractStoryboardAsync(new MediaFile(), "/out/sb_%03d.jpg", count: 7);

        _captured!.Times.Should().BeEmpty();
        _captured.Count.Should().Be(7);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(501)]
    public async Task ExtractStoryboardAsync_ShouldThrowInvalidOperationConfigurationException_WhenCountIsOutOfRange(int count)
    {
        var media = new MediaFile { Duration = TimeSpan.FromMinutes(10) };

        var act = () => _sut.ExtractStoryboardAsync(media, "/out/sb_%03d.jpg", count);

        await act.Should().ThrowAsync<InvalidOperationConfigurationException>();
        _ffmpeg.Verify(f => f.ExtractThumbnailsAsync(It.IsAny<MediaFile>(), It.IsAny<string>(), It.IsAny<ThumbnailSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExtractAtTimestampsAsync_ShouldForwardTimestampsInOrderAndFormat_WhenCalled()
    {
        var stamps = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(9) };

        await _sut.ExtractAtTimestampsAsync(new MediaFile(), "/out/t_%03d.png", stamps, ThumbnailFormat.Png);

        _captured!.Times.Should().Equal(stamps);
        _captured.Format.Should().Be(ThumbnailFormat.Png);
    }

    [Fact]
    public async Task ExtractAtTimestampsAsync_ShouldForwardEmptyTimes_WhenNoTimestampsProvided()
    {
        var result = await _sut.ExtractAtTimestampsAsync(new MediaFile(), "/out/t_%03d.jpg", Array.Empty<TimeSpan>());

        _captured!.Times.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
    }
}
