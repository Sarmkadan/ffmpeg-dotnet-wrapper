// entire file content ...
using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace ffmpeg_dotnet_wrapper_tests
{
    public class TranscodeSettingsTests
    {
        [Fact]
        public void Constructor_DefaultSettings_ReturnsExpectedValues()
        {
            // Arrange
            // Act
            var settings = new TranscodeSettings();

            // Assert
            settings.Should().NotBeNull();
        }

        [Fact]
        public void Validate_UndefinedVideoCodec_ThrowsConfigurationException()
        {
            // Arrange
            var settings = new TranscodeSettings { VideoCodec = (VideoCodec)999 };

            // Act
            var act = () => settings.Validate();

            // Assert
            act.Should().Throw<InvalidOperationConfigurationException>();
        }

        [Fact]
        public void Validate_UndefinedQualityPreset_ThrowsConfigurationException()
        {
            // Arrange
            var settings = new TranscodeSettings { Quality = (QualityPreset)999 };

            // Act
            var act = () => settings.Validate();

            // Assert
            act.Should().Throw<InvalidOperationConfigurationException>();
        }
    }
}
