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

    [Fact]
    public void Validate_ReturnsListWithFontSizeTooLargeError_WhenFontSizeExceedsMaximum()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 121,
            SubtitleStreamIndex = 0,
            FontName = "Arial",
            Language = "en"
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Single(errors);
        Assert.Equal("FontSize must be between 6 and 120, but was 121.", errors[0]);
    }

    [Fact]
    public void Validate_ReturnsListWithFontSizeAtMinimum_WhenFontSizeIsExactlyMinimum()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 6,
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
    public void Validate_ReturnsListWithFontSizeAtMaximum_WhenFontSizeIsExactlyMaximum()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 120,
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
    public void Validate_ReturnsListWithSubtitleStreamIndexAtBoundary_WhenSubtitleStreamIndexIsZero()
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
    public void Validate_ReturnsListWithFontNameNull_WhenFontNameIsNull()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 10,
            SubtitleStreamIndex = 0,
            FontName = null,
            Language = "en"
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ReturnsListWithLanguageNull_WhenLanguageIsNull()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 10,
            SubtitleStreamIndex = 0,
            FontName = "Arial",
            Language = null
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ReturnsListWithLanguageTooLongError_WhenLanguageExceedsMaximumLength()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 10,
            SubtitleStreamIndex = 0,
            FontName = "Arial",
            Language = "ThisLanguageCodeIsWayTooLong"
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Single(errors);
        Assert.Equal("Language code is too long.", errors[0]);
    }

    [Fact]
    public void Validate_ReturnsListWithWhitespaceFontNameError_WhenFontNameIsWhitespace()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 10,
            SubtitleStreamIndex = 0,
            FontName = "   ",
            Language = "en"
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Single(errors);
        Assert.Equal("FontName cannot be whitespace.", errors[0]);
    }

    [Fact]
    public void Validate_ReturnsListWithWhitespaceLanguageError_WhenLanguageIsWhitespace()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 10,
            SubtitleStreamIndex = 0,
            FontName = "Arial",
            Language = "   "
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Single(errors);
        Assert.Equal("Language cannot be whitespace.", errors[0]);
    }

    [Fact]
    public void Validate_WithNullSubtitleSettings_ThrowsArgumentNullException()
    {
        // Arrange
        SubtitleSettings subtitleSettings = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => SubtitleSettingsValidation.Validate(subtitleSettings));
    }

    [Fact]
    public void Validate_ReturnsEmptyList_WhenOnlyRequiredFieldsAreValid()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 12,
            SubtitleStreamIndex = 5,
            FontName = null,
            Language = null
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenAllFieldsAreValid()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 18,
            SubtitleStreamIndex = 2,
            FontName = "Courier New",
            Language = "fr"
        };

        // Act
        var isValid = SubtitleSettingsValidation.IsValid(subtitleSettings);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenFontSizeIsOutOfRange()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 0,
            SubtitleStreamIndex = 0,
            FontName = "Arial",
            Language = "en"
        };

        // Act
        var isValid = SubtitleSettingsValidation.IsValid(subtitleSettings);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenSubtitleSettingsIsValid()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 24,
            SubtitleStreamIndex = 1,
            FontName = "Times New Roman",
            Language = "de"
        };

        // Act
        var exceptionResult = Record.Exception(() => SubtitleSettingsValidation.EnsureValid(subtitleSettings));

        // Assert
        Assert.Null(exceptionResult);
    }

    [Fact]
    public void Validate_ReturnsReadOnlyList_WithCorrectType()
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
        Assert.IsAssignableFrom<IReadOnlyList<string>>(errors);
    }

    [Fact]
    public void Validate_ReturnsListWithMultipleErrors_WhenMultipleFieldsAreInvalid()
    {
        // Arrange
        var subtitleSettings = new SubtitleSettings
        {
            FontSize = 5,
            SubtitleStreamIndex = -2,
            FontName = "   ",
            Language = "ThisLanguageIsTooLong"
        };

        // Act
        var errors = SubtitleSettingsValidation.Validate(subtitleSettings);

        // Assert
        Assert.Equal(4, errors.Count);
    }
}
