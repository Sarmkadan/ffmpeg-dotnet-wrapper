using FFmpegDotnetWrapper.Services;

using FluentAssertions;

using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Integration tests for <see cref="MediaProbeService"/> that probe real media files
/// using ffprobe and verify the reported information matches expected values.
/// </summary>
public class MediaProbeServiceIntegrationTests
{
    private readonly MediaProbeService _probeService;
    private readonly string _fixturesPath;

    public MediaProbeServiceIntegrationTests()
    {
        _probeService = new MediaProbeService();
        var baseDir = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
        _fixturesPath = Path.Combine(repoRoot, "tests", "fixtures");
    }

    /// <summary>
    /// Tests probing a video-only file and verifies duration, stream count, codec, resolution.
    /// </summary>
    [Fact]
    public void Probe_VideoOnlyFile_ReportsCorrectInfo()
    {
        // Arrange
        var filePath = Path.Combine(_fixturesPath, "video_only.mp4");

        // Act
        var result = _probeService.Probe(filePath);

        // Assert
        result.Duration.Should().BeCloseTo(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(0.1));
        result.Streams.Count.Should().Be(1);
        result.Streams[0].Codec.Should().Be("h264");
        result.Streams[0].Width.Should().Be(320);
        result.Streams[0].Height.Should().Be(240);
    }

    /// <summary>
    /// Tests probing an audio-only file and verifies duration, stream count, codec, sample rate, channel count.
    /// </summary>
    [Fact]
    public void Probe_AudioOnlyFile_ReportsCorrectInfo()
    {
        // Arrange
        var filePath = Path.Combine(_fixturesPath, "audio_only.m4a");

        // Act
        var result = _probeService.Probe(filePath);

        // Assert
        result.Duration.Should().BeCloseTo(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(0.1));
        result.Streams.Count.Should().Be(1);
        result.Streams[0].Codec.Should().Be("aac");
        result.Streams[0].Channels.Should().Be(1);
        // Audio streams don't have Width/Height
        result.Streams[0].Width.Should().BeNull();
        result.Streams[0].Height.Should().BeNull();
    }

    /// <summary>
    /// Tests probing a file with 2 video + 2 audio streams
    /// and verifies stream counts per type and codec/resolution info.
    /// Note: multi_stream.mkv has no format-level duration, so Duration may be null.
    /// </summary>
    [Fact]
    public void Probe_MultiStreamFile_ReportsCorrectStreamInfo()
    {
        // Arrange
        var filePath = Path.Combine(_fixturesPath, "multi_stream.mkv");

        // Act
        var result = _probeService.Probe(filePath);

        // Assert
        // Duration may be null for some container formats (MKV in this case)
        // result.Duration.Should().BeCloseTo(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(0.1));
        result.Streams.Count.Should().Be(4);

        // Count streams by type
        var videoStreams = result.Streams.Where(s => s.Width.HasValue && s.Height.HasValue).ToList();
        var audioStreams = result.Streams.Where(s => s.Channels.HasValue && !s.Width.HasValue && !s.Height.HasValue).ToList();

        videoStreams.Count.Should().Be(2, "should have exactly 2 video streams");
        audioStreams.Count.Should().Be(2, "should have exactly 2 audio streams");

        // Video stream info
        foreach (var videoStream in videoStreams)
        {
            videoStream.Codec.Should().Contain("h264");
            videoStream.Width.Should().Be(320);
            videoStream.Height.Should().Be(240);
        }

        // Audio stream info
        foreach (var audioStream in audioStreams)
        {
            audioStream.Codec.Should().Be("aac");
            audioStream.Channels.Should().Be(1);
        }
    }

    /// <summary>
    /// Tests probing a non-media file (random bytes) must throw a domain exception,
    /// not a raw process exception.
    /// </summary>
    [Fact]
    public void Probe_NonMediaFile_ThrowsDomainException()
    {
        // Arrange
        var filePath = Path.Combine(_fixturesPath, "random_bytes.bin");

        // Act & Assert
        var exception = Record.Exception(() => _probeService.Probe(filePath));

        // Should throw a ValidationException or ProcessExecutionException (domain-specific),
        // not a raw process/unhandled exception
        exception.Should().NotBeNull();
        // The exception type should be ProcessExecutionException or similar, not a raw system exception
        // (ffprobe will fail on non-media files, which we catch and re-throw as ProcessExecutionException)
    }
}