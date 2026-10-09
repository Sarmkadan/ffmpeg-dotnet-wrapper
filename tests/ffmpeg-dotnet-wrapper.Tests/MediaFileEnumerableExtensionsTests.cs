using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Unit tests for the <see cref="MediaFileEnumerableExtensions"/> class.
/// Tests aggregate and filter helpers over sequences of <see cref="MediaFile"/> instances.
/// </summary>
public class MediaFileEnumerableExtensionsTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), $"media_enum_{Guid.NewGuid()}");

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaFileEnumerableExtensionsTests"/> class.
    /// Creates a temporary directory that holds the fake media files used by the tests.
    /// </summary>
    public MediaFileEnumerableExtensionsTests()
    {
        Directory.CreateDirectory(_tempDirectory);
    }

    /// <summary>
    /// Deletes the temporary directory and all files created during the test run.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }

    /// <summary>
    /// Creates a fake media file with the given name on disk and returns it as a <see cref="MediaFile"/>.
    /// </summary>
    private MediaFile CreateMediaFile(string fileName, TimeSpan? duration = null)
    {
        var path = Path.Combine(_tempDirectory, fileName);
        File.WriteAllText(path, "fake media data");

        return new MediaFile(path) { Duration = duration };
    }

    /// <summary>
    /// Tests that GetTotalDuration sums the durations of all files that have one.
    /// </summary>
    [Fact]
    public void GetTotalDuration_WithDurations_ReturnsSum()
    {
        var files = new[]
        {
            CreateMediaFile("a.mp4", TimeSpan.FromSeconds(30)),
            CreateMediaFile("b.mp4", TimeSpan.FromMinutes(1.5))
        };

        var total = files.GetTotalDuration();

        total.Should().Be(TimeSpan.FromSeconds(120));
    }

    /// <summary>
    /// Tests that GetTotalDuration ignores files without a known duration and null elements.
    /// </summary>
    [Fact]
    public void GetTotalDuration_WithMissingDurationsAndNulls_SkipsThem()
    {
        var files = new MediaFile?[]
        {
            CreateMediaFile("a.mp4", TimeSpan.FromSeconds(10)),
            CreateMediaFile("b.mp4"),
            null
        };

        var total = files!.GetTotalDuration();

        total.Should().Be(TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Tests that GetTotalDuration returns zero for an empty sequence.
    /// </summary>
    [Fact]
    public void GetTotalDuration_EmptySequence_ReturnsZero()
    {
        var files = Array.Empty<MediaFile>();

        files.GetTotalDuration().Should().Be(TimeSpan.Zero);
    }

    /// <summary>
    /// Tests that GetTotalDuration throws when the sequence is null.
    /// </summary>
    [Fact]
    public void GetTotalDuration_NullSequence_ThrowsArgumentNullException()
    {
        IEnumerable<MediaFile> files = null!;

        var act = () => files.GetTotalDuration();

        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that WithExtension returns only files whose extension matches, ignoring case.
    /// </summary>
    [Fact]
    public void WithExtension_MatchingFiles_ReturnsOnlyMatchesIgnoringCase()
    {
        var mp4 = CreateMediaFile("a.MP4");
        var mkv = CreateMediaFile("b.mkv");
        var another = CreateMediaFile("c.mp4");

        var result = new[] { mp4, mkv, another }.WithExtension(".mp4").ToList();

        result.Should().BeEquivalentTo(new[] { mp4, another });
    }

    /// <summary>
    /// Tests that WithExtension accepts an extension without a leading dot.
    /// </summary>
    [Fact]
    public void WithExtension_WithoutLeadingDot_MatchesSameAsWithDot()
    {
        var mp4 = CreateMediaFile("a.mp4");
        var mkv = CreateMediaFile("b.mkv");

        var result = new[] { mp4, mkv }.WithExtension("mp4").ToList();

        result.Should().ContainSingle().Which.Should().BeSameAs(mp4);
    }

    /// <summary>
    /// Tests that WithExtension skips null elements instead of throwing.
    /// </summary>
    [Fact]
    public void WithExtension_WithNullElements_SkipsThem()
    {
        var mp4 = CreateMediaFile("a.mp4");
        var files = new MediaFile?[] { null, mp4 };

        var result = files!.WithExtension(".mp4").ToList();

        result.Should().ContainSingle().Which.Should().BeSameAs(mp4);
    }

    /// <summary>
    /// Tests that WithExtension throws when the sequence is null.
    /// </summary>
    [Fact]
    public void WithExtension_NullSequence_ThrowsArgumentNullException()
    {
        IEnumerable<MediaFile> files = null!;

        var act = () => files.WithExtension(".mp4");

        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that WithExtension throws when the extension is null, empty, or whitespace.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithExtension_InvalidExtension_ThrowsArgumentException(string? extension)
    {
        var files = Array.Empty<MediaFile>();

        var act = () => files.WithExtension(extension!);

        act.Should().Throw<ArgumentException>();
    }
}
