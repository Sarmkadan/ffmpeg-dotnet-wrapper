using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class MediaFileTests
{
    private string _tempFile = null!;

    public MediaFileTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"test_media_{Guid.NewGuid()}.mp4");
        File.WriteAllText(_tempFile, "fake video data");
    }

    ~MediaFileTests()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    [Fact]
    public void Constructor_DefaultValues_CreatesNewInstance()
    {
        var mediaFile = new MediaFile();

        mediaFile.Id.Should().NotBeEmpty();
        mediaFile.Name.Should().BeEmpty();
        mediaFile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        mediaFile.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithFilePath_SetsPropertiesFromFile()
    {
        var mediaFile = new MediaFile(_tempFile);

        mediaFile.FilePath.Should().NotBeEmpty();
        mediaFile.Name.Should().NotBeEmpty();
        mediaFile.FileSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public void FilePath_WithValidFile_AcceptsPath()
    {
        var mediaFile = new MediaFile { FilePath = _tempFile };

        mediaFile.FilePath.Should().NotBeEmpty();
        File.Exists(mediaFile.FilePath).Should().BeTrue();
    }

    [Fact]
    public void FilePath_WithNonexistentFile_ThrowsException()
    {
        var mediaFile = new MediaFile();

        var act = () => mediaFile.FilePath = "/nonexistent/file.mp4";

        act.Should().Throw<InvalidMediaFileException>();
    }

    [Fact]
    public void FilePath_WithEmptyString_ThrowsException()
    {
        var mediaFile = new MediaFile();

        var act = () => mediaFile.FilePath = string.Empty;

        act.Should().Throw<InvalidMediaFileException>()
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void Extension_ReturnsFileExtension()
    {
        var mediaFile = new MediaFile(_tempFile);

        mediaFile.Extension.Should().Be(".mp4");
    }

    [Fact]
    public void Name_ReturnsFileNameWithoutExtension()
    {
        var mediaFile = new MediaFile(_tempFile);

        mediaFile.Name.Should().Be(Path.GetFileNameWithoutExtension(_tempFile));
    }

    [Fact]
    public void FileSize_ReturnsActualFileSize()
    {
        var mediaFile = new MediaFile(_tempFile);

        mediaFile.FileSize.Should().Be(new FileInfo(_tempFile).Length);
    }

    [Fact]
    public void ValidateAsVideo_WithValidDimensions_DoesNotThrow()
    {
        var mediaFile = new MediaFile
        {
            FilePath = _tempFile,
            Width = 1920,
            Height = 1080,
            Duration = TimeSpan.FromSeconds(120)
        };

        var act = () => mediaFile.ValidateAsVideo();

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateAsVideo_WithoutWidth_ThrowsException()
    {
        var mediaFile = new MediaFile
        {
            FilePath = _tempFile,
            Height = 1080,
            Duration = TimeSpan.FromSeconds(120)
        };

        var act = () => mediaFile.ValidateAsVideo();

        act.Should().Throw<InvalidMediaFileException>()
            .WithMessage("*missing dimensions*");
    }

    [Fact]
    public void ValidateAsVideo_WithoutHeight_ThrowsException()
    {
        var mediaFile = new MediaFile
        {
            FilePath = _tempFile,
            Width = 1920,
            Duration = TimeSpan.FromSeconds(120)
        };

        var act = () => mediaFile.ValidateAsVideo();

        act.Should().Throw<InvalidMediaFileException>()
            .WithMessage("*missing dimensions*");
    }

    [Fact]
    public void ValidateAsVideo_WithoutDuration_ThrowsException()
    {
        var mediaFile = new MediaFile
        {
            FilePath = _tempFile,
            Width = 1920,
            Height = 1080
        };

        var act = () => mediaFile.ValidateAsVideo();

        act.Should().Throw<InvalidMediaFileException>()
            .WithMessage("*invalid duration*");
    }

    [Fact]
    public void ValidateAsVideo_WithZeroDuration_ThrowsException()
    {
        var mediaFile = new MediaFile
        {
            FilePath = _tempFile,
            Width = 1920,
            Height = 1080,
            Duration = TimeSpan.Zero
        };

        var act = () => mediaFile.ValidateAsVideo();

        act.Should().Throw<InvalidMediaFileException>()
            .WithMessage("*invalid duration*");
    }

    [Fact]
    public void Metadata_CanStoreArbitraryKeyValuePairs()
    {
        var mediaFile = new MediaFile(_tempFile);

        mediaFile.Metadata["encoder"] = "libx264";
        mediaFile.Metadata["profile"] = "Main";

        mediaFile.Metadata["encoder"].Should().Be("libx264");
        mediaFile.Metadata["profile"].Should().Be("Main");
    }

    [Fact]
    public void Description_CanBeSet()
    {
        var mediaFile = new MediaFile(_tempFile);
        var description = "Test video file";

        mediaFile.Description = description;

        mediaFile.Description.Should().Be(description);
    }

    [Fact]
    public void ModifiedAt_CanBeSet()
    {
        var mediaFile = new MediaFile(_tempFile);
        var now = DateTime.UtcNow;

        mediaFile.ModifiedAt = now;

        mediaFile.ModifiedAt.Should().BeCloseTo(now, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void MediaProperties_CanBeSetIndependently()
    {
        var mediaFile = new MediaFile(_tempFile);

        mediaFile.VideoCodec = "h264";
        mediaFile.AudioCodec = "aac";
        mediaFile.FrameRate = 30.0;
        mediaFile.Bitrate = 5000000;
        mediaFile.AudioSampleRate = 48000;
        mediaFile.AudioChannels = 2;
        mediaFile.Duration = TimeSpan.FromSeconds(120);

        mediaFile.VideoCodec.Should().Be("h264");
        mediaFile.AudioCodec.Should().Be("aac");
        mediaFile.FrameRate.Should().Be(30.0);
        mediaFile.Bitrate.Should().Be(5000000);
        mediaFile.AudioSampleRate.Should().Be(48000);
        mediaFile.AudioChannels.Should().Be(2);
        mediaFile.Duration.Should().Be(TimeSpan.FromSeconds(120));
    }

    [Fact]
    public void Id_IsUniqueForEachInstance()
    {
        var media1 = new MediaFile(_tempFile);
        var media2 = new MediaFile(_tempFile);

        media1.Id.Should().NotBe(media2.Id);
    }

    [Fact]
    public void FilePath_NormalizesToAbsolutePath()
    {
        var mediaFile = new MediaFile(_tempFile);
        var absolutePath = Path.GetFullPath(_tempFile);

        mediaFile.FilePath.Should().Be(absolutePath);
    }
}
