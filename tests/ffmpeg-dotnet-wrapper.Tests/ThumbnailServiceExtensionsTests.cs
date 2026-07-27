// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using System.Threading;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using Moq;
using Xunit;

namespace FFmpegDotnetWrapper.Tests.Services;

public class ThumbnailServiceExtensionsTests
{
    private static readonly CancellationToken DefaultToken = CancellationToken.None;

    private static Mock<ThumbnailService> CreateMockService(ThumbnailResult? result = null)
    {
        var mock = new Mock<ThumbnailService>();
        mock.Setup(s => s.ExtractSingleAsync(
                It.IsAny<MediaFile>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result ?? new ThumbnailResult());

        return mock;
    }

    #region Happy paths

    [Fact]
    public async Task ExtractFirstFrameAsync_ReturnsResult_FromUnderlyingService()
    {
        // Arrange
        var expected = new ThumbnailResult();
        var mock = new Mock<ThumbnailService>();
        mock.Setup(s => s.ExtractSingleAsync(
                It.IsAny<MediaFile>(),
                It.IsAny<string>(),
                TimeSpan.Zero,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var media = new MediaFile { Duration = TimeSpan.FromSeconds(5) };
        var outputPath = "first.jpg";

        // Act
        var actual = await mock.Object.ExtractFirstFrameAsync(media, outputPath, DefaultToken);

        // Assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task ExtractLastFrameAsync_ReturnsResult_AtLastMillisecond()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(10);
        var media = new MediaFile { Duration = duration };
        var expected = new ThumbnailResult();

        var mock = new Mock<ThumbnailService>();
        mock.Setup(s => s.ExtractSingleAsync(
                It.IsAny<MediaFile>(),
                It.IsAny<string>(),
                It.Is<TimeSpan>(ts => ts == duration.Subtract(TimeSpan.FromMilliseconds(1))),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var outputPath = "last.jpg";

        // Act
        var actual = await mock.Object.ExtractLastFrameAsync(media, outputPath, DefaultToken);

        // Assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task ExtractMiddleFrameAsync_ReturnsResult_AtHalfDuration()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(8);
        var media = new MediaFile { Duration = duration };
        var expected = new ThumbnailResult();

        var mock = new Mock<ThumbnailService>();
        mock.Setup(s => s.ExtractSingleAsync(
                It.IsAny<MediaFile>(),
                It.IsAny<string>(),
                It.Is<TimeSpan>(ts => ts == TimeSpan.FromSeconds(duration.TotalSeconds / 2)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var outputPath = "middle.jpg";

        // Act
        var actual = await mock.Object.ExtractMiddleFrameAsync(media, outputPath, DefaultToken);

        // Assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task ExtractAtPercentageAsync_ReturnsResult_AtCorrectPosition()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(20);
        var media = new MediaFile { Duration = duration };
        var percentage = 25.0;
        var expected = new ThumbnailResult();

        var mock = new Mock<ThumbnailService>();
        mock.Setup(s => s.ExtractSingleAsync(
                It.IsAny<MediaFile>(),
                It.IsAny<string>(),
                It.Is<TimeSpan>(ts => Math.Abs(ts.TotalSeconds - duration.TotalSeconds * (percentage / 100)) < 0.001),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var outputPath = "quarter.jpg";

        // Act
        var actual = await mock.Object.ExtractAtPercentageAsync(media, outputPath, percentage, DefaultToken);

        // Assert
        Assert.Same(expected, actual);
    }

    #endregion

    #region Argument validation

    [Fact]
    public async Task ExtractFirstFrameAsync_Throws_WhenServiceIsNull()
    {
        // Arrange
        ThumbnailService? service = null;
        var media = new MediaFile { Duration = TimeSpan.FromSeconds(1) };
        var outputPath = "out.jpg";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service!.ExtractFirstFrameAsync(media, outputPath, DefaultToken));
    }

    [Fact]
    public async Task ExtractFirstFrameAsync_Throws_WhenMediaIsNull()
    {
        // Arrange
        var mock = CreateMockService();
        MediaFile? media = null;
        var outputPath = "out.jpg";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mock.Object.ExtractFirstFrameAsync(media!, outputPath, DefaultToken));
    }

    [Fact]
    public async Task ExtractFirstFrameAsync_Throws_WhenOutputPathIsNullOrEmpty()
    {
        // Arrange
        var mock = CreateMockService();
        var media = new MediaFile { Duration = TimeSpan.FromSeconds(1) };

        // Null
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await mock.Object.ExtractFirstFrameAsync(media, null!, DefaultToken));

        // Empty
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await mock.Object.ExtractFirstFrameAsync(media, string.Empty, DefaultToken));
    }

    [Fact]
    public async Task ExtractLastFrameAsync_Throws_WhenDurationMissing()
    {
        // Arrange
        var mock = CreateMockService();
        var media = new MediaFile { Duration = null };
        var outputPath = "out.jpg";

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await mock.Object.ExtractLastFrameAsync(media, outputPath, DefaultToken));
    }

    [Fact]
    public async Task ExtractMiddleFrameAsync_Throws_WhenDurationMissing()
    {
        // Arrange
        var mock = CreateMockService();
        var media = new MediaFile { Duration = null };
        var outputPath = "out.jpg";

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await mock.Object.ExtractMiddleFrameAsync(media, outputPath, DefaultToken));
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(150)]
    public async Task ExtractAtPercentageAsync_Throws_WhenPercentageOutOfRange(double percentage)
    {
        // Arrange
        var mock = CreateMockService();
        var media = new MediaFile { Duration = TimeSpan.FromSeconds(10) };
        var outputPath = "out.jpg";

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await mock.Object.ExtractAtPercentageAsync(media, outputPath, percentage, DefaultToken));
    }

    [Fact]
    public async Task ExtractAtPercentageAsync_Throws_WhenDurationMissing()
    {
        // Arrange
        var mock = CreateMockService();
        var media = new MediaFile { Duration = null };
        var outputPath = "out.jpg";

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await mock.Object.ExtractAtPercentageAsync(media, outputPath, 50, DefaultToken));
    }

    #endregion
}
