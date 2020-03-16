using FFmpegDotnetWrapper.Utilities;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Provides unit tests for the <see cref="FileUtilities"/> class.
/// Tests file path validation, file operations, and utility methods.
/// </summary>
public class FileUtilitiesTests
{
    /// <summary>
/// Gets or sets the temporary directory path used for test files.
/// </summary>
private string _tempDir = null!;
    /// <summary>
/// Gets or sets the test file path used for testing file operations.
/// </summary>
private string _testFile = null!;

    public FileUtilitiesTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ffmpeg_tests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _testFile = Path.Combine(_tempDir, "test.mp4");
        File.WriteAllText(_testFile, "test content");
    }

    /// <summary>
/// Finalizes an instance of the <see cref="FileUtilitiesTests"/> class.
/// Cleans up temporary directory and test files.
/// </summary>
~FileUtilitiesTests()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch { }
    }


/// <summary>
/// Tests that <see cref="FileUtilities.IsValidFilePath"/> returns true for absolute file paths.
/// </summary>
    [Fact]
    public void IsValidFilePath_WithAbsolutePath_ReturnsTrue()
    {
        var absolutePath = Path.GetFullPath(_testFile);

        FileUtilities.IsValidFilePath(absolutePath).Should().BeTrue();
    }

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidFilePath"/> returns false for relative file paths.
/// </summary>

    [Fact]
    public void IsValidFilePath_WithRelativePath_ReturnsFalse()
    {
        FileUtilities.IsValidFilePath("relative/path/file.mp4").Should().BeFalse();
    }

    [Fact]

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidFilePath"/> returns false for directory traversal paths.
/// </summary>
    public void IsValidFilePath_WithDirectoryTraversal_ReturnsFalse()
    {
        var path = Path.Combine(_tempDir, "..", "file.mp4");
        FileUtilities.IsValidFilePath(path).Should().BeFalse();
    }

    [Fact]

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidFilePath"/> returns false when null is passed.
/// </summary>
    public void IsValidFilePath_WithNull_ReturnsFalse()
    {
        FileUtilities.IsValidFilePath(null).Should().BeFalse();
    }

    [Fact]
    public void IsValidFilePath_WithEmptyString_ReturnsFalse()

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidFilePath"/> returns false when empty string is passed.
/// </summary>
    {
        FileUtilities.IsValidFilePath(string.Empty).Should().BeFalse();
    }

    [Fact]
    public void IsValidFilePath_WithWhitespace_ReturnsFalse()
    {

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidFilePath"/> returns false when whitespace string is passed.
/// </summary>
        FileUtilities.IsValidFilePath("   ").Should().BeFalse();
    }

    [Fact]
    public void IsValidFilePath_WithTildaExpansion_ReturnsFalse()
    {
        FileUtilities.IsValidFilePath("~/file.mp4").Should().BeFalse();

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidFilePath"/> returns false for paths with tilde expansion.
/// </summary>
    }

    [Fact]
    public void IsValidFilePath_WithEnvironmentVariable_ReturnsFalse()
    {
        FileUtilities.IsValidFilePath("$HOME/file.mp4").Should().BeFalse();
    }

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidFilePath"/> returns false for paths with environment variables.
/// </summary>

    [Fact]
    public void IsValidInputFile_WithValidFile_ReturnsTrue()
    {
        var absolutePath = Path.GetFullPath(_testFile);

        FileUtilities.IsValidInputFile(absolutePath).Should().BeTrue();

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidInputFile"/> returns true for valid existing files.
/// </summary>
    }

    [Fact]
    public void IsValidInputFile_WithNonexistentFile_ReturnsFalse()
    {
        var path = Path.GetFullPath(Path.Combine(_tempDir, "nonexistent.mp4"));


/// <summary>
/// Tests that <see cref="FileUtilities.IsValidInputFile"/> returns false for nonexistent files.
/// </summary>
        FileUtilities.IsValidInputFile(path).Should().BeFalse();
    }

    [Fact]
    public void IsValidInputFile_WithRelativePath_ReturnsFalse()
    {
        FileUtilities.IsValidInputFile("relative/path/file.mp4").Should().BeFalse();

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidInputFile"/> returns false for relative file paths.
/// </summary>
    }

    [Fact]
    public void IsValidInputFile_WithNull_ReturnsFalse()
    {
        FileUtilities.IsValidInputFile(null).Should().BeFalse();
    }

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidInputFile"/> returns false when null is passed.
/// </summary>

    [Fact]
    public void IsValidOutputPath_WithValidDirectory_ReturnsTrue()
    {
        var outputPath = Path.Combine(_tempDir, "output.mp4");

        FileUtilities.IsValidOutputPath(outputPath).Should().BeTrue();

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidOutputPath"/> returns true for valid output paths in existing directories.
/// </summary>
    }

    [Fact]
    public void IsValidOutputPath_WithNonexistentDirectoryAndCreateFlag_ReturnsTrue()
    {
        var newDir = Path.Combine(_tempDir, "subdir", "output.mp4");


/// <summary>
/// Tests that <see cref="FileUtilities.IsValidOutputPath"/> returns true and creates directory when createDirectoryIfNeeded is true.
/// </summary>
        FileUtilities.IsValidOutputPath(newDir, createDirectoryIfNeeded: true).Should().BeTrue();
    }

    [Fact]
    public void IsValidOutputPath_WithNonexistentDirectoryNoCreateFlag_ReturnsFalse()
    {
        var newDir = Path.Combine(_tempDir, "nonexistent_subdir", "output.mp4");

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidOutputPath"/> returns false when directory does not exist and createDirectoryIfNeeded is false.
/// </summary>

        FileUtilities.IsValidOutputPath(newDir, createDirectoryIfNeeded: false).Should().BeFalse();
    }

    [Fact]
    public void IsValidOutputPath_WithRelativePath_ReturnsFalse()
    {

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidOutputPath"/> returns false for relative output paths.
/// </summary>
        FileUtilities.IsValidOutputPath("relative/output.mp4").Should().BeFalse();
    }

    [Fact]
    public void IsValidOutputPath_WithNull_ReturnsFalse()
    {
        FileUtilities.IsValidOutputPath(null).Should().BeFalse();

/// <summary>
/// Tests that <see cref="FileUtilities.IsValidOutputPath"/> returns false when null is passed.
/// </summary>
    }

    [Fact]
    public void GetFileExtension_WithValidFile_ReturnsExtensionWithoutDot()
    {
        var extension = FileUtilities.GetFileExtension(_testFile);


/// <summary>
/// Tests that <see cref="FileUtilities.GetFileExtension"/> returns the file extension without the dot for valid files.
/// </summary>
        extension.Should().Be("mp4");
    }

    [Fact]
    public void GetFileExtension_WithDifferentExtension_ReturnsCorrectExtension()
    {
        var filePath = "/path/to/video.mkv";

/// <summary>
/// Tests that <see cref="FileUtilities.GetFileExtension"/> returns the correct extension for different file types.
/// </summary>

        var extension = FileUtilities.GetFileExtension(filePath);

        extension.Should().Be("mkv");
    }

    [Fact]

/// <summary>
/// Tests that <see cref="FileUtilities.GetFileExtension"/> returns lowercase extension regardless of input case.
/// </summary>
    public void GetFileExtension_WithUppercaseExtension_ReturnsLowercase()
    {
        var filePath = "/path/to/video.MP4";

        var extension = FileUtilities.GetFileExtension(filePath);

        extension.Should().Be("mp4");

/// <summary>
/// Tests that <see cref="FileUtilities.GetFileExtension"/> returns empty string when file has no extension.
/// </summary>
    }

    [Fact]
    public void GetFileExtension_WithoutExtension_ReturnsEmptyString()
    {
        var filePath = "/path/to/noextension";


/// <summary>
/// Tests that <see cref="FileUtilities.GetFileExtension"/> returns empty string when null is passed.
/// </summary>
        var extension = FileUtilities.GetFileExtension(filePath);

        extension.Should().BeEmpty();
    }

    [Fact]
    public void GetFileExtension_WithNull_ReturnsEmptyString()

/// <summary>
/// Tests that <see cref="FileUtilities.GetHumanReadableFileSize"/> returns correct label for byte sizes.
/// </summary>
    {
        var extension = FileUtilities.GetFileExtension(null!);

        extension.Should().BeEmpty();
    }

    [Fact]

/// <summary>
/// Tests that <see cref="FileUtilities.GetHumanReadableFileSize"/> returns correct label for kilobyte sizes.
/// </summary>
    public void GetHumanReadableFileSize_WithBytes_ReturnsBytesLabel()
    {
        var size = FileUtilities.GetHumanReadableFileSize(512);

        size.Should().Be("512 B");
    }


/// <summary>
/// Tests that <see cref="FileUtilities.GetHumanReadableFileSize"/> returns correct label for megabyte sizes.
/// </summary>
    [Fact]
    public void GetHumanReadableFileSize_WithKilobytes_ReturnsKBLabel()
    {
        var size = FileUtilities.GetHumanReadableFileSize(1024 * 2);

        size.Should().Be("2 KB");
    }

/// <summary>
/// Tests that <see cref="FileUtilities.GetHumanReadableFileSize"/> returns correct label for gigabyte sizes.
/// </summary>

    [Fact]
    public void GetHumanReadableFileSize_WithMegabytes_ReturnsMBLabel()
    {
        var size = FileUtilities.GetHumanReadableFileSize(1024 * 1024);

        size.Should().Be("1 MB");
    }

    [Fact]
    public void GetHumanReadableFileSize_WithGigabytes_ReturnsGBLabel()
    {
        var size = FileUtilities.GetHumanReadableFileSize(1024L * 1024 * 1024 * 2);

        size.Should().Be("2 GB");
    }

    [Fact]
    public void GetFileSize_WithValidFile_ReturnsSize()
    {
        var size = FileUtilities.GetFileSize(_testFile);

        size.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetFileSize_WithNonexistentFile_ReturnsNegativeOne()
    {
        var size = FileUtilities.GetFileSize("/nonexistent/file.mp4");

        size.Should().Be(-1);
    }

    [Fact]
    public void SafeDeleteFile_WithExistingFile_DeletesFile()
    {
        var fileToDelete = Path.Combine(_tempDir, "temp_delete.tmp");
        File.WriteAllText(fileToDelete, "temp");

        FileUtilities.SafeDeleteFile(fileToDelete).Should().BeTrue();

        File.Exists(fileToDelete).Should().BeFalse();
    }

    [Fact]
    public void SafeDeleteFile_WithNonexistentFile_ReturnsTrue()
    {
        var result = FileUtilities.SafeDeleteFile("/nonexistent/file.tmp");

        result.Should().BeTrue();
    }

    [Fact]
    public void GetTempFilePath_CreatesUniquePath()
    {
        var path1 = FileUtilities.GetTempFilePath();
        var path2 = FileUtilities.GetTempFilePath();

        path1.Should().NotBe(path2);
    }

    [Fact]
    public void GetTempFilePath_WithExtension_IncludesExtension()
    {
        var path = FileUtilities.GetTempFilePath(".mp4");

        Path.GetExtension(path).Should().Be(".mp4");
    }

    [Fact]
    public void GetTempFilePath_CreatesParentDirectory()
    {
        var path = FileUtilities.GetTempFilePath();

        Directory.Exists(Path.GetDirectoryName(path)).Should().BeTrue();
    }

    [Fact]
    public void SanitizeFileName_RemovesNullCharacter()
    {
        var invalidFileName = "test\0video.mp4";

        var sanitized = FileUtilities.SanitizeFileName(invalidFileName);

        sanitized.Should().NotContain("\0");
        sanitized.Should().Contain("test");
        sanitized.Should().Contain("video");
    }

    [Fact]
    public void SanitizeFileName_PreservesValidCharacters()
    {
        var validFileName = "my-video_2025.mp4";

        var sanitized = FileUtilities.SanitizeFileName(validFileName);

        sanitized.Should().Be("my-video_2025.mp4");
    }

    [Fact]
    public void SanitizeFileName_PreservesExtension()
    {
        var fileName = "video (copy).mp4";

        var sanitized = FileUtilities.SanitizeFileName(fileName);

        Path.GetExtension(sanitized).Should().Be(".mp4");
    }

    [Fact]
    public void AreFormatsCompatible_WithSameExtension_ReturnsTrue()
    {
        var file1 = "/path/to/video1.mp4";
        var file2 = "/path/to/video2.mp4";

        FileUtilities.AreFormatsCompatible(file1, file2).Should().BeTrue();
    }

    [Fact]
    public void AreFormatsCompatible_WithDifferentExtensions_ReturnsFalse()
    {
        var file1 = "/path/to/video.mp4";
        var file2 = "/path/to/video.mkv";

        FileUtilities.AreFormatsCompatible(file1, file2).Should().BeFalse();
    }

    [Fact]
    public void AreFormatsCompatible_WithMixedCase_ReturnsTrue()
    {
        var file1 = "/path/to/video.MP4";
        var file2 = "/path/to/video.mp4";

        FileUtilities.AreFormatsCompatible(file1, file2).Should().BeTrue();
    }
}
