using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Unit tests for the <see cref="MediaFile"/> class.
/// Tests various constructors, properties, and validation methods to ensure correct behavior.
/// </summary>
public class MediaFileTests
{
    private string _tempFile = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaFileTests"/> class.
    /// Creates a temporary test media file that is cleaned up after the test run.
    /// </summary>
    public MediaFileTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"test_media_{Guid.NewGuid()}.mp4");
        File.WriteAllText(_tempFile, "fake video data");
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="MediaFileTests"/> class.
    /// Deletes the temporary test media file created during test initialization.
    /// </summary>
    ~MediaFileTests()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    /// <summary>
    /// Tests that the default constructor creates a new <see cref="MediaFile"/> instance
    /// with empty name, empty metadata, current timestamp for creation, and a generated unique ID.
    /// </summary>
    [Fact]
    public void Constructor_DefaultValues_CreatesNewInstance()
    {
        var mediaFile = new MediaFile();

        mediaFile.Id.Should().NotBeEmpty();
        mediaFile.Name.Should().BeEmpty();
        mediaFile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        mediaFile.Metadata.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the constructor with a file path sets the FilePath, Name, and FileSize properties
    /// from the actual file information.
    /// </summary>
    [Fact]
    public void Constructor_WithFilePath_SetsPropertiesFromFile()
    {
        var mediaFile = new MediaFile(_tempFile);

        mediaFile.FilePath.Should().NotBeEmpty();
        mediaFile.Name.Should().NotBeEmpty();
        mediaFile.FileSize.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that setting FilePath to a valid file path accepts the path and validates file existence.
    /// </summary>
    [Fact]
    public void FilePath_WithValidFile_AcceptsPath()
    {
        var mediaFile = new MediaFile { FilePath = _tempFile };

        mediaFile.FilePath.Should().NotBeEmpty();
        File.Exists(mediaFile.FilePath).Should().BeTrue();
    }

    /// <summary>
    /// Tests that setting FilePath to a nonexistent file path throws <see cref="InvalidMediaFileException"/>.
    /// </summary>
    [Fact]
    public void FilePath_WithNonexistentFile_ThrowsException()
    {
        var mediaFile = new MediaFile();

        var act = () => mediaFile.FilePath = "/nonexistent/file.mp4";

        act.Should().Throw<InvalidMediaFileException>();
    }

    /// <summary>
    /// Tests that setting FilePath to an empty string throws <see cref="InvalidMediaFileException"/> with appropriate message.
    /// </summary>
    [Fact]
    public void FilePath_WithEmptyString_ThrowsException()
    {
        var mediaFile = new MediaFile();

        var act = () => mediaFile.FilePath = string.Empty;

        act.Should().Throw<InvalidMediaFileException>()
            .WithMessage("*cannot be null or empty*");
    }

    /// <summary>
    /// Tests that the Extension property returns the correct file extension from the FilePath.
    /// </summary>
    [Fact]
    public void Extension_ReturnsFileExtension()
    {
        var mediaFile = new MediaFile(_tempFile);

        mediaFile.Extension.Should().Be(".mp4");
    }

    /// <summary>
    /// Tests that the Name property returns the filename without the extension.
    /// </summary>
    [Fact]
    public void Name_ReturnsFileNameWithoutExtension()
    {
        var mediaFile = new MediaFile(_tempFile);

        mediaFile.Name.Should().Be(Path.GetFileNameWithoutExtension(_tempFile));
    }

    /// <summary>
    /// Tests that the FileSize property returns the actual file size in bytes.
    /// </summary>
    [Fact]
    public void FileSize_ReturnsActualFileSize()
    {
        var mediaFile = new MediaFile(_tempFile);

        mediaFile.FileSize.Should().Be(new FileInfo(_tempFile).Length);
    }

    /// <summary>
    /// Tests that ValidateAsVideo does not throw when all required video dimensions (Width, Height, Duration) are set.
    /// </summary>
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

    /// <summary>
    /// Tests that ValidateAsVideo throws <see cref="InvalidMediaFileException"/> when Width is not set.
    /// </summary>
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

    /// <summary>
    /// Tests that ValidateAsVideo throws <see cref="InvalidMediaFileException"/> when Height is not set.
    /// </summary>
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

    /// <summary>
    /// Tests that ValidateAsVideo throws <see cref="InvalidMediaFileException"/> when Duration is not set.
    /// </summary>
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

    /// <summary>
    /// Tests that ValidateAsVideo throws <see cref="InvalidMediaFileException"/> when Duration is set to TimeSpan.Zero.
    /// </summary>
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

    /// <summary>
    /// Tests that the Metadata dictionary can store arbitrary key-value pairs for additional media file properties.
    /// </summary>
    [Fact]
    public void Metadata_CanStoreArbitraryKeyValuePairs()
    {
        var mediaFile = new MediaFile(_tempFile);

        mediaFile.Metadata["encoder"] = "libx264";
        mediaFile.Metadata["profile"] = "Main";

        mediaFile.Metadata["encoder"].Should().Be("libx264");
        mediaFile.Metadata["profile"].Should().Be("Main");
    }

    /// <summary>
    /// Tests that the Description property can be set and retrieved correctly.
    /// </summary>
    [Fact]
    public void Description_CanBeSet()
    {
        var mediaFile = new MediaFile(_tempFile);
        var description = "Test video file";

        mediaFile.Description = description;

        mediaFile.Description.Should().Be(description);
    }

    /// <summary>
    /// Tests that the ModifiedAt property can be set and retrieved correctly.
    /// </summary>
    [Fact]
    public void ModifiedAt_CanBeSet()
    {
        var mediaFile = new MediaFile(_tempFile);
        var now = DateTime.UtcNow;

        mediaFile.ModifiedAt = now;

        mediaFile.ModifiedAt.Should().BeCloseTo(now, TimeSpan.FromMilliseconds(100));
    }

    /// <summary>
    /// Tests that various media properties (VideoCodec, AudioCodec, FrameRate, Bitrate, etc.) can be set independently.
    /// </summary>
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

    /// <summary>
    /// Tests that each <see cref="MediaFile"/> instance gets a unique ID.
    /// </summary>
    [Fact]
    public void Id_IsUniqueForEachInstance()
    {
        var media1 = new MediaFile(_tempFile);
        var media2 = new MediaFile(_tempFile);

        media1.Id.Should().NotBe(media2.Id);
    }

    /// <summary>
    /// Tests that the FilePath property normalizes to an absolute path.
    /// </summary>
    [Fact]
    public void FilePath_NormalizesToAbsolutePath()
    {
        var mediaFile = new MediaFile(_tempFile);
        var absolutePath = Path.GetFullPath(_tempFile);

        mediaFile.FilePath.Should().Be(absolutePath);
    }
}
