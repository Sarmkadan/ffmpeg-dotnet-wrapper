using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class ThumbnailSettingsTests : IDisposable
{
    private readonly string _tempVideo;

    public ThumbnailSettingsTests()
    {
        _tempVideo = Path.Combine(Path.GetTempPath(), $"test_video_{Guid.NewGuid()}.mp4");
        File.WriteAllText(_tempVideo, "fake video data");
    }

    public void Dispose()
    {
        if (File.Exists(_tempVideo)) File.Delete(_tempVideo);
    }

    private MediaFile CreateMediaFileWithDuration(TimeSpan duration)
    {
        var media = new MediaFile(_tempVideo);
        media.Duration = duration;
        media.Width = 1920;
        media.Height = 1080;
        return media;
    }

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var settings = new ThumbnailSettings();

        settings.Count.Should().Be(1);
        settings.Format.Should().Be(ThumbnailFormat.Jpeg);
        settings.Times.Should().BeEmpty();
        settings.Width.Should().BeNull();
        settings.Height.Should().BeNull();
        settings.JpegQuality.Should().Be(2);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(500)]
    public void Count_WithValidValue_AcceptsValue(int count)
    {
        var settings = new ThumbnailSettings { Count = count };

        settings.Count.Should().Be(count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(501)]
    public void Count_OutsideValidRange_ThrowsException(int count)
    {
        var settings = new ThumbnailSettings();

        var act = () => settings.Count = count;

        act.Should().Throw<InvalidOperationConfigurationException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(31)]
    public void JpegQuality_WithValidValue_AcceptsValue(int quality)
    {
        var settings = new ThumbnailSettings { JpegQuality = quality };

        settings.JpegQuality.Should().Be(quality);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void JpegQuality_OutsideValidRange_ThrowsException(int quality)
    {
        var settings = new ThumbnailSettings();

        var act = () => settings.JpegQuality = quality;

        act.Should().Throw<InvalidOperationConfigurationException>();
    }

    [Fact]
    public void Validate_WithTimestampBeyondDuration_ThrowsException()
    {
        var media = CreateMediaFileWithDuration(TimeSpan.FromSeconds(60));
        var settings = new ThumbnailSettings();
        settings.Times.Add(TimeSpan.FromSeconds(90));

        var act = () => settings.Validate(media);

        act.Should().Throw<InvalidOperationConfigurationException>()
           .WithMessage("*exceeds video duration*");
    }

    [Fact]
    public void Validate_WithNegativeTimestamp_ThrowsException()
    {
        var media = CreateMediaFileWithDuration(TimeSpan.FromSeconds(60));
        var settings = new ThumbnailSettings();
        settings.Times.Add(TimeSpan.FromSeconds(-1));

        var act = () => settings.Validate(media);

        act.Should().Throw<InvalidOperationConfigurationException>()
           .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Validate_WithValidExplicitTimestamps_DoesNotThrow()
    {
        var media = CreateMediaFileWithDuration(TimeSpan.FromSeconds(120));
        var settings = new ThumbnailSettings();
        settings.Times.Add(TimeSpan.FromSeconds(10));
        settings.Times.Add(TimeSpan.FromSeconds(60));
        settings.Times.Add(TimeSpan.FromSeconds(110));

        var act = () => settings.Validate(media);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithInvalidWidth_ThrowsException()
    {
        var media = CreateMediaFileWithDuration(TimeSpan.FromSeconds(60));
        var settings = new ThumbnailSettings { Width = 0 };

        var act = () => settings.Validate(media);

        act.Should().Throw<InvalidOperationConfigurationException>()
           .WithMessage("*Width*");
    }

    [Fact]
    public void Validate_WithAutoWidth_DoesNotThrow()
    {
        var media = CreateMediaFileWithDuration(TimeSpan.FromSeconds(60));
        var settings = new ThumbnailSettings { Width = -1, Height = 720 };

        var act = () => settings.Validate(media);

        act.Should().NotThrow();
    }

    [Fact]
    public void Clone_ProducesIndependentCopy()
    {
        var original = new ThumbnailSettings
        {
            Count = 5,
            Format = ThumbnailFormat.Png,
            Width = 640,
            Height = 360
        };
        original.Times.Add(TimeSpan.FromSeconds(10));

        var clone = original.Clone();

        clone.Count.Should().Be(5);
        clone.Format.Should().Be(ThumbnailFormat.Png);
        clone.Width.Should().Be(640);
        clone.Times.Should().HaveCount(1);

        // Mutations on clone should not affect original
        clone.Times.Add(TimeSpan.FromSeconds(20));
        original.Times.Should().HaveCount(1);
    }
}
