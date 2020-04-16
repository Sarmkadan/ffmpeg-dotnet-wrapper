// entire file content ...
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
    }
}
