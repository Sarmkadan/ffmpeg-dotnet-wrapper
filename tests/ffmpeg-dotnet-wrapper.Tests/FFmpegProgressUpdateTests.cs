// entire file content ...
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace ffmpeg_dotnet_wrapper_tests
{
    public class FFmpegProgressUpdateTests
    {
        [Fact]
        public void ToString_ReturnsExpectedString()
        {
            // Arrange
            var progressUpdate = new FFmpegProgressUpdate();

            // Act
            var result = progressUpdate.ToString();

            // Assert
            result.Should().NotBeNullOrEmpty();
        }
    }
}
