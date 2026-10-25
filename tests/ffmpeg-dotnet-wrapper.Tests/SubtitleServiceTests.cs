namespace FFmpegDotnetWrapper.Tests;

using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

/// <summary>
/// Provides unit tests for the <see cref="SubtitleService"/> class.
/// </summary>
public class SubtitleServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _videoPath;
    private readonly string _subtitlePath;
    private readonly Mock<IFFmpegService> _ffmpeg = new();
    private readonly SubtitleService _sut;
    private SubtitleSettings? _captured;

    /// <summary>
    /// Creates a temporary input video and subtitle file, and a service wired to a mocked <see cref="IFFmpegService"/>.
    /// </summary>
    public SubtitleServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "subtitle-service-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _videoPath = Path.Combine(_tempDir, "input.mp4");
        _subtitlePath = Path.Combine(_tempDir, "subs.srt");
        File.WriteAllBytes(_videoPath, new byte[] { 0 });
        File.WriteAllText(_subtitlePath, "1\n00:00:00,000 --> 00:00:01,000\nHello\n");

        _sut = new SubtitleService(_ffmpeg.Object, Mock.Of<ILogger<SubtitleService>>());
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void Constructor_NullFFmpegService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SubtitleService(null!, Mock.Of<ILogger<SubtitleService>>()));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SubtitleService(_ffmpeg.Object, null!));
    }

    [Fact]
    public async Task EmbedSoftSubtitlesAsync_ValidInput_PassesSoftSettingsAndReturnsServiceResult()
    {
        // Arrange
        var input = new MediaFile(_videoPath);
        var expected = new ConversionResult();
        expected.MarkAsSuccess(Path.Combine(_tempDir, "out.mp4"));
        SetupEmbed(expected);

        // Act
        var result = await _sut.EmbedSoftSubtitlesAsync(input, _subtitlePath, Path.Combine(_tempDir, "out.mp4"), "en");

        // Assert
        result.Should().BeSameAs(expected);
        _captured.Should().NotBeNull();
        _captured!.HardEmbed.Should().BeFalse();
        _captured.SubtitlePath.Should().Be(Path.GetFullPath(_subtitlePath));
        _captured.Language.Should().Be("en");
    }

    [Fact]
    public async Task EmbedSoftSubtitlesAsync_NullInputMedia_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.EmbedSoftSubtitlesAsync(null!, _subtitlePath, "out.mp4"));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.BurnSubtitlesAsync(null!, _subtitlePath, "out.mp4"));
    }

    [Fact]
    public async Task EmbedSoftSubtitlesAsync_MissingSubtitleFile_ThrowsBeforeCallingService()
    {
        // Arrange
        var input = new MediaFile(_videoPath);
        var missing = Path.Combine(_tempDir, "missing.srt");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationConfigurationException>(() =>
            _sut.EmbedSoftSubtitlesAsync(input, missing, "out.mp4"));
        _ffmpeg.Verify(f => f.EmbedSubtitlesAsync(
            It.IsAny<MediaFile>(), It.IsAny<string>(), It.IsAny<SubtitleSettings>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EmbedSoftSubtitlesAsync_ServiceFailure_ReturnsFailedResultWithoutThrowing()
    {
        // Arrange
        var input = new MediaFile(_videoPath);
        var failed = new ConversionResult();
        failed.MarkAsFailed("ffmpeg exited with error", 1);
        SetupEmbed(failed);

        // Act
        var result = await _sut.EmbedSoftSubtitlesAsync(input, _subtitlePath, "out.mp4");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("ffmpeg exited with error");
    }

    [Fact]
    public async Task BurnSubtitlesAsync_DefaultStyle_UsesArialSize24HardEmbed()
    {
        // Arrange
        var input = new MediaFile(_videoPath);
        SetupEmbed(new ConversionResult());

        // Act
        await _sut.BurnSubtitlesAsync(input, _subtitlePath, "out.mp4");

        // Assert
        _captured.Should().NotBeNull();
        _captured!.HardEmbed.Should().BeTrue();
        _captured.FontName.Should().Be("Arial");
        _captured.FontSize.Should().Be(24);
    }

    [Fact]
    public async Task BurnSubtitlesAsync_CustomStyle_PassesFontAndCancellationToken()
    {
        // Arrange
        var input = new MediaFile(_videoPath);
        var cts = new CancellationTokenSource();
        SetupEmbed(new ConversionResult());

        // Act
        await _sut.BurnSubtitlesAsync(input, _subtitlePath, "out.mp4", "Verdana", 36, cts.Token);

        // Assert
        _captured!.FontName.Should().Be("Verdana");
        _captured.FontSize.Should().Be(36);
        _ffmpeg.Verify(f => f.EmbedSubtitlesAsync(
            input, "out.mp4", It.IsAny<SubtitleSettings>(), cts.Token), Times.Once);
    }

    /// <summary>
    /// Configures the mocked engine to capture the settings it receives and return <paramref name="result"/>.
    /// </summary>
    private void SetupEmbed(ConversionResult result)
    {
        _ffmpeg.Setup(f => f.EmbedSubtitlesAsync(
                It.IsAny<MediaFile>(), It.IsAny<string>(), It.IsAny<SubtitleSettings>(), It.IsAny<CancellationToken>()))
            .Callback<MediaFile, string, SubtitleSettings, CancellationToken>((_, _, s, _) => _captured = s)
            .ReturnsAsync(result);
    }
}
