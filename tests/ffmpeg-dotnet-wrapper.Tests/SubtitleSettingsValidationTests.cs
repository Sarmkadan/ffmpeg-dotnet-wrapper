using FFmpegDotnetWrapper.Models;
using Xunit;

namespace ffmpeg_dotnet_wrapper.Tests.Models;

public class SubtitleSettingsValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_WhenSubtitleSettingsIsValid()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 10,
            SubtitleStreamIndex = 0,
            FontName = "Arial",
            Language = "en"
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ReturnsListWithFontSizeError_WhenFontSizeIsTooSmall()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 5,
            SubtitleStreamIndex = 0,
            FontName = "Arial",
            Language = "en"
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Single(errors);
        Assert.Equal("FontSize must be between 6 and 120, but was 5.", errors[0]);
    }

    [Fact]
    public void Validate_ReturnsListWithSubtitleStreamIndexError_WhenSubtitleStreamIndexIsNegative()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 10,
            SubtitleStreamIndex = -1,
            FontName = "Arial",
            Language = "en"
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Single(errors);
        Assert.Equal("SubtitleStreamIndex must be non-negative, but was -1.", errors[0]);
    }

    [Fact]
    public void Validate_ReturnsListWithFontNameError_WhenFontNameIsEmpty()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 10,
            SubtitleStreamIndex = 0,
            FontName = "",
            Language = "en"
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Single(errors);
        Assert.Equal("FontName cannot be whitespace.", errors[0]);
    }

    [Fact]
    public void Validate_ReturnsListWithLanguageError_WhenLanguageIsEmpty()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 10,
            SubtitleStreamIndex = 0,
            FontName = "Arial",
            Language = ""
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Single(errors);
        Assert.Equal("Language cannot be whitespace.", errors[0]);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenSubtitleSettingsIsValid()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 10,
            SubtitleStreamIndex = 0,
            FontName = "Arial",
            Language = "en"
        };

        // Act
        var isValid = SubtitleSettingsValidation.IsValid(subtitleSettings);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenSubtitleSettingsIsInvalid()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 5,
            SubtitleStreamIndex = -1,
            FontName = "",
            Language = ""
        };

        // Act
        var isValid = SubtitleSettingsValidation.IsValid(subtitleSettings);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WhenSubtitleSettingsIsInvalid()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 5,
            SubtitleStreamIndex = -1,
            FontName = "",
            Language = ""
        };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => SubtitleSettingsValidation.EnsureValid(subtitleSettings));
    }
}
