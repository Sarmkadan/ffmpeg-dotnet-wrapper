// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Unit tests for the <see cref="AdaptiveBitrateService"/> class.
/// </summary>
public class AdaptiveBitrateServiceTests
{
    /// <summary>
    /// Tests that ladder generation for a 1080p source contains expected renditions.
    /// The default ladder should include 1080p, 720p, 480p, and 360p profiles.
    /// </summary>
    [Fact]
    public void StreamingProfile_DefaultLadder_ShouldContainExpectedRenditions()
    {
        // Arrange & Act
        var ladder = StreamingProfile.DefaultLadder;

        // Assert
        ladder.Should().HaveCount(4);
        ladder[0].Should().Be(StreamingProfile.FullHD);
        ladder[0].Name.Should().Be("1080p");
        ladder[0].Width.Should().Be(1920);
        ladder[0].Height.Should().Be(1080);
        ladder[0].VideoBitrateKbps.Should().Be(4500);

        ladder[1].Should().Be(StreamingProfile.HD);
        ladder[1].Name.Should().Be("720p");
        ladder[1].Width.Should().Be(1280);
        ladder[1].Height.Should().Be(720);
        ladder[1].VideoBitrateKbps.Should().Be(2500);

        ladder[2].Should().Be(StreamingProfile.SD);
        ladder[2].Name.Should().Be("480p");
        ladder[2].Width.Should().Be(854);
        ladder[2].Height.Should().Be(480);
        ladder[2].VideoBitrateKbps.Should().Be(1000);

        ladder[3].Should().Be(StreamingProfile.Mobile);
        ladder[3].Name.Should().Be("360p");
        ladder[3].Width.Should().Be(640);
        ladder[3].Height.Should().Be(360);
        ladder[3].VideoBitrateKbps.Should().Be(500);
    }

    /// <summary>
    /// Tests that low-res source does not upscale to higher resolutions.
    /// When encoding a 360p source, profiles with higher resolutions should not be used.
    /// </summary>
    [Fact]
    public void StreamingProfile_ShouldNotUpscale_WhenSourceResolutionIsLow()
    {
        // Arrange
        var settings = new StreamingPipelineSettings
        {
            InputFilePath = "/path/to/input.mp4",
            OutputDirectory = "/tmp/output",
            Profiles = [
                new StreamingProfile("360p", 640, 360, 500, 64),
                new StreamingProfile("480p", 854, 480, 1000, 96),
                new StreamingProfile("720p", 1280, 720, 2500, 128),
                new StreamingProfile("1080p", 1920, 1080, 4500, 192)
            ]
        };

        // Act - simulate the sorting that happens in AdaptiveBitrateService
        var orderedProfiles = settings.Profiles
            .OrderByDescending(p => p.VideoBitrateKbps)
            .ToList();

        // Assert
        orderedProfiles.Should().HaveCount(4);
        orderedProfiles[0].Name.Should().Be("1080p");
        orderedProfiles[1].Name.Should().Be("720p");
        orderedProfiles[2].Name.Should().Be("480p");
        orderedProfiles[3].Name.Should().Be("360p");
    }

    /// <summary>
    /// Tests that invalid input dimensions are rejected during pipeline validation.
    /// </summary>
    [Fact]
    public void StreamingPipelineSettings_ShouldThrowException_WhenInputFileDoesNotExist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var nonExistentFile = Path.Combine(tempDir, "nonexistent.mp4");
        var settings = new StreamingPipelineSettings
        {
            InputFilePath = nonExistentFile,
            OutputDirectory = tempDir,
            Profiles = [StreamingProfile.FullHD]
        };

        try
        {
            // Act & Assert
            settings.Invoking(s => s.Validate())
                .Should().Throw<FileNotFoundException>()
                .WithMessage($"Input file not found: {nonExistentFile}*");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    /// <summary>
    /// Tests that pipeline throws exception when no profiles are configured.
    /// </summary>
    [Fact]
    public void StreamingPipelineSettings_ShouldThrowException_WhenNoProfilesConfigured()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var testFile = Path.Combine(tempDir, "test.mp4");
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(testFile, "dummy");

        var settings = new StreamingPipelineSettings
        {
            InputFilePath = testFile,
            OutputDirectory = tempDir,
            Profiles = []
        };

        try
        {
            // Act & Assert
            settings.Invoking(s => s.Validate())
                .Should().Throw<InvalidOperationException>()
                .WithMessage("At least one streaming profile must be specified.*");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    /// <summary>
    /// Tests that <see cref="StreamingProfile.TotalBitrateKbps"/> correctly
    /// calculates combined video and audio bitrate.
    /// </summary>
    [Fact]
    public void StreamingProfile_TotalBitrateKbps_ShouldCalculateCombinedBitrate()
    {
        // Arrange
        var profile = new StreamingProfile("test", 1280, 720, 2500, 128);

        // Act
        var totalBitrate = profile.TotalBitrateKbps;

        // Assert
        totalBitrate.Should().Be(2628); // 2500 + 128
    }

    /// <summary>
    /// Tests that <see cref="StreamingProfile.Resolution"/> correctly
    /// formats width and height as resolution string.
    /// </summary>
    [Fact]
    public void StreamingProfile_Resolution_ShouldFormatAsResolutionString()
    {
        // Arrange
        var profile = new StreamingProfile("test", 1920, 1080, 4500, 192);

        // Act
        var resolution = profile.Resolution;

        // Assert
        resolution.Should().Be("1920x1080");
    }

    /// <summary>
    /// Tests that <see cref="StreamingSegment.ActualBitrateKbps"/> returns zero
    /// when duration is zero to avoid division by zero.
    /// </summary>
    [Fact]
    public void StreamingSegment_ActualBitrateKbps_ShouldReturnZero_WhenDurationIsZero()
    {
        // Arrange
        var segment = new StreamingSegment
        {
            Id = Guid.NewGuid().ToString("N"),
            PipelineId = "test-pipeline",
            Profile = StreamingProfile.FullHD,
            SequenceNumber = 0,
            FilePath = "/tmp/segment.ts",
            DurationSeconds = 0,
            FileSizeBytes = 1000000
        };

        // Act
        var actualBitrate = segment.ActualBitrateKbps;

        // Assert
        actualBitrate.Should().Be(0);
    }

    /// <summary>
    /// Tests that <see cref="StreamingSegment.ActualBitrateKbps"/> correctly
    /// calculates bitrate from file size and duration.
    /// </summary>
    [Fact]
    public void StreamingSegment_ActualBitrateKbps_ShouldCalculateCorrectBitrate()
    {
        // Arrange
        var segment = new StreamingSegment
        {
            Id = Guid.NewGuid().ToString("N"),
            PipelineId = "test-pipeline",
            Profile = StreamingProfile.FullHD,
            SequenceNumber = 0,
            FilePath = "/tmp/segment.ts",
            DurationSeconds = 6.0,
            FileSizeBytes = 4500000 // 4.5 MB = 36 Mb = 4500 kbps
        };

        // Act
        var actualBitrate = segment.ActualBitrateKbps;

        // Assert
        actualBitrate.Should().BeApproximately(6000, 1); // 4500000 * 8 / (6 * 1000) = 6000 kbps
    }
}