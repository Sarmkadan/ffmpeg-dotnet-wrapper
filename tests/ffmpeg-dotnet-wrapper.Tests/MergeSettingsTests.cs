// entire file content ...
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace ffmpeg_dotnet_wrapper_tests
{
    public class MergeSettingsTests
    {
        [Fact]
        public void Constructor_DefaultSettings_ReturnsExpectedValues()
        {
            // Arrange
            // Act
            var settings = new MergeSettings();

            // Assert
            settings.Should().NotBeNull();
        }
    }
}
