using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the ThumbnailSettings class.
/// </summary>
public class ThumbnailSettingsTests : IDisposable
{
    private readonly string _tempVideo;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailSettingsTests"/> class.
    /// </summary>
    public ThumbnailSettingsTests()
    {
        _tempVideo = Path.Combine(Path.GetTempPath(), $"test_video_{Guid.NewGuid()}.mp4");
        File.WriteAllText(_tempVideo, "fake video data");
    }

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="ThumbnailSettingsTests"/> is reclaimed by garbage collection.
    /// </summary>
    public void Dispose()
    {
        if (File.Exists(_tempVideo)) File.Delete(_tempVideo);
    }

    /// <summary>
    /// Creates a new MediaFile instance with the specified duration.
    /// </summary>
    /// <param name="duration">The duration of the media file.</param>
    /// <returns>A new MediaFile instance.</returns>
    private MediaFile CreateMediaFileWithDuration(TimeSpan duration)
    {
        var media = new MediaFile(_tempVideo);
        media.Duration = duration;
        media.Width = 1920;
        media.Height = 1080;
        return media;
    }

    /// <summary>
    /// Verifies that the default values of the ThumbnailSettings instance are correct.
    /// </summary>
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

    /// <summary>
    /// Verifies that the Count property can be set to a valid value.
    /// </summary>
    /// <param name="count">The value to set the Count property to.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(500)]
    public void Count_WithValidValue_AcceptsValue(int count)
    {
        var settings = new ThumbnailSettings { Count = count };

        settings.Count.Should().Be(count);
    }

    /// <summary>
    /// Verifies that setting the Count property to an invalid value throws an exception.
    /// </summary>
    /// <param name="count">The value to set the Count property to.</param>
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

    /// <summary>
    /// Verifies that the JpegQuality property can be set to a valid value.
    /// </summary>
    /// <param name="quality">The value to set the JpegQuality property to.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(31)]
    public void JpegQuality_WithValidValue_AcceptsValue(int quality)
    {
        var settings = new ThumbnailSettings { JpegQuality = quality };

        settings.JpegQuality.Should().Be(quality);
    }

    /// <summary>
    /// Verifies that setting the JpegQuality property to an invalid value throws an exception.
    /// </summary>
    /// <param name="quality">The value to set the JpegQuality property to.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void JpegQuality_OutsideValidRange_ThrowsException(int quality)
    {
        var settings = new ThumbnailSettings();

        var act = () => settings.JpegQuality = quality;

        act.Should().Throw<InvalidOperationConfigurationException>();
    }

    /// <summary>
    /// Verifies that the Validate method throws an exception when a timestamp beyond the video duration is specified.
    /// </summary>
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

    /// <summary>
    /// Verifies that the Validate method throws an exception when a negative timestamp is specified.
    /// </summary>
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

    /// <summary>
    /// Verifies that the Validate method does not throw an exception when valid explicit timestamps are specified.
    /// </summary>
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

    /// <summary>
    /// Verifies that the Validate method throws an exception when an invalid width is specified.
    /// </summary>
    [Fact]
    public void Validate_WithInvalidWidth_ThrowsException()
    {
        var media = CreateMediaFileWithDuration(TimeSpan.FromSeconds(60));
        var settings = new ThumbnailSettings { Width = 0 };

        var act = () => settings.Validate(media);

        act.Should().Throw<InvalidOperationConfigurationException>()
           .WithMessage("*Width*");
    }

    /// <summary>
    /// Verifies that the Validate method does not throw an exception when an auto width is specified.
    /// </summary>
    [Fact]
    public void Validate_WithAutoWidth_DoesNotThrow()
    {
        var media = CreateMediaFileWithDuration(TimeSpan.FromSeconds(60));
        var settings = new ThumbnailSettings { Width = -1, Height = 720 };

        var act = () => settings.Validate(media);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that the Clone method produces an independent copy of the ThumbnailSettings instance.
    /// </summary>
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
