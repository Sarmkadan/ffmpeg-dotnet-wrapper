using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

/// <summary>
/// Provides unit tests for the <see cref="SubtitleSettings"/> class.
/// </summary>
public class SubtitleSettingsTests : IDisposable
{
    private readonly string _tempSrt;
    private readonly string _tempAss;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleSettingsTests"/> class.
    /// </summary>
    public SubtitleSettingsTests()
    {
        _tempSrt = Path.Combine(Path.GetTempPath(), $"test_sub_{Guid.NewGuid()}.srt");
        _tempAss = Path.Combine(Path.GetTempPath(), $"test_sub_{Guid.NewGuid()}.ass");

        File.WriteAllText(_tempSrt, "1\n00:00:01,000 --> 00:00:02,000\nHello World\n");
        File.WriteAllText(_tempAss, "[Script Info]\nTitle: Test\n");
    }

    /// <summary>
    /// Releases all resources used by the <see cref="SubtitleSettingsTests"/> class.
    /// </summary>
    public void Dispose()
    {
        if (File.Exists(_tempSrt)) File.Delete(_tempSrt);
        if (File.Exists(_tempAss)) File.Delete(_tempAss);
    }

    /// <summary>
    /// Verifies that the default values of a <see cref="SubtitleSettings"/> instance are correct.
    /// </summary>
    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var settings = new SubtitleSettings();

        settings.HardEmbed.Should().BeFalse();
        settings.CharEncoding.Should().Be("UTF-8");
        settings.FontName.Should().Be("Arial");
        settings.FontSize.Should().Be(24);
        settings.SubtitleStreamIndex.Should().Be(0);
        settings.Language.Should().BeNull();
    }

    /// <summary>
    /// Verifies that setting the <see cref="SubtitleSettings.SubtitlePath"/> property with an existing .srt file path succeeds.
    /// </summary>
    [Fact]
    public void SubtitlePath_WithExistingSrtFile_AcceptsPath()
    {
        var settings = new SubtitleSettings { SubtitlePath = _tempSrt };

        settings.SubtitlePath.Should().Be(Path.GetFullPath(_tempSrt));
    }

    /// <summary>
    /// Verifies that setting the <see cref="SubtitleSettings.SubtitlePath"/> property with an existing .ass file path succeeds.
    /// </summary>
    [Fact]
    public void SubtitlePath_WithExistingAssFile_AcceptsPath()
    {
        var settings = new SubtitleSettings { SubtitlePath = _tempAss };

        settings.SubtitlePath.Should().Be(Path.GetFullPath(_tempAss));
    }

    /// <summary>
    /// Verifies that setting the <see cref="SubtitleSettings.SubtitlePath"/> property with a non-existent file path throws an exception.
    /// </summary>
    [Fact]
    public void SubtitlePath_WithNonexistentFile_ThrowsException()
    {
        var settings = new SubtitleSettings();

        var act = () => settings.SubtitlePath = "/nonexistent/subtitles.srt";

        act.Should().Throw<InvalidOperationConfigurationException>()
           .WithMessage("*does not exist*");
    }

    /// <summary>
    /// Verifies that setting the <see cref="SubtitleSettings.SubtitlePath"/> property with a file path having an unsupported extension throws an exception.
    /// </summary>
    [Fact]
    public void SubtitlePath_WithUnsupportedExtension_ThrowsException()
    {
        var tempTxt = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.txt");
        File.WriteAllText(tempTxt, "not a subtitle");

        try
        {
            var settings = new SubtitleSettings();
            var act = () => settings.SubtitlePath = tempTxt;

            act.Should().Throw<InvalidOperationConfigurationException>()
               .WithMessage("*Unsupported subtitle format*");
        }
        finally
        {
            if (File.Exists(tempTxt)) File.Delete(tempTxt);
        }
    }

    /// <summary>
    /// Verifies that setting the <see cref="SubtitleSettings.SubtitlePath"/> property with an empty string throws an exception.
    /// </summary>
    [Fact]
    public void SubtitlePath_WithEmptyString_ThrowsException()
    {
        var settings = new SubtitleSettings();

        var act = () => settings.SubtitlePath = string.Empty;

        act.Should().Throw<InvalidOperationConfigurationException>();
    }

    /// <summary>
    /// Verifies that setting the <see cref="SubtitleSettings.CharEncoding"/> property with an empty string throws an exception.
    /// </summary>
    [Fact]
    public void CharEncoding_WithEmptyValue_ThrowsException()
    {
        var settings = new SubtitleSettings();

        var act = () => settings.CharEncoding = string.Empty;

        act.Should().Throw<InvalidOperationConfigurationException>();
    }

    /// <summary>
    /// Verifies that setting the <see cref="SubtitleSettings.FontSize"/> property outside the valid range throws an exception during validation.
    /// </summary>
    [Fact]
    public void FontSize_OutsideValidRange_ThrowsOnValidate()
    {
        var settings = new SubtitleSettings
        {
            SubtitlePath = _tempSrt,
            FontSize = 5
        };

        var act = () => settings.Validate();
        act.Should().Throw<InvalidOperationConfigurationException>()
           .WithMessage("*FontSize*");
    }

    /// <summary>
    /// Verifies that a <see cref="SubtitleSettings"/> instance with valid settings does not throw an exception during validation.
    /// </summary>
    [Fact]
    public void Validate_WithValidSettings_DoesNotThrow()
    {
        var settings = new SubtitleSettings
        {
            SubtitlePath = _tempSrt,
            HardEmbed = true,
            FontSize = 24,
            Language = "en"
        };

        var act = () => settings.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that the <see cref="SubtitleSettings.Clone"/> method produces an independent copy of the original instance.
    /// </summary>
    [Fact]
    public void Clone_ProducesIndependentCopy()
    {
        var original = new SubtitleSettings
        {
            SubtitlePath = _tempSrt,
            HardEmbed = true,
            FontSize = 30,
            Language = "fr"
        };

        var clone = original.Clone();

        clone.SubtitlePath.Should().Be(original.SubtitlePath);
        clone.HardEmbed.Should().Be(original.HardEmbed);
        clone.FontSize.Should().Be(original.FontSize);
        clone.Language.Should().Be(original.Language);

        // Mutations on the clone should not affect the original
        clone.FontSize = 20;
        original.FontSize.Should().Be(30);
    }
}
