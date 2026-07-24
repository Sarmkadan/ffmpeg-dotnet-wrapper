namespace ffmpeg_dotnet_wrapper_tests
{
    using Xunit;
    using FFmpegDotnetWrapper.Models;
    using System;
    using System.Collections.Generic;

    public class ThumbnailResultTests
    {
        [Fact]
        public void Constructor_InitializesDefaults()
        {
            // Act
            var result = new ThumbnailResult();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Empty(result.Thumbnails);
            Assert.Equal(TimeSpan.Zero, result.Duration);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(0, result.Count);
            Assert.Null(result.FirstThumbnail);
        }

        [Fact]
        public void SetProperties_UpdatesValues()
        {
            // Arrange
            var result = new ThumbnailResult();
            var thumbnails = new List<string> { "path/to/thumb1.jpg", "path/to/thumb2.jpg" };
            var duration = TimeSpan.FromSeconds(5);

            // Act
            result.IsSuccess = true;
            result.Thumbnails = thumbnails;
            result.Duration = duration;
            result.ErrorMessage = "Error message";

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(thumbnails, result.Thumbnails);
            Assert.Equal(duration, result.Duration);
            Assert.Equal("Error message", result.ErrorMessage);
        }

        [Fact]
        public void Count_ReturnsCorrectValue()
        {
            // Arrange
            var result = new ThumbnailResult();

            // Assert initial (0)
            Assert.Equal(0, result.Count);

            // Act
            result.Thumbnails.Add("thumb1.jpg");
            
            // Assert (1)
            Assert.Equal(1, result.Count);

            // Act
            result.Thumbnails.Add("thumb2.jpg");
            
            // Assert (2)
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FirstThumbnail_ReturnsCorrectValue()
        {
            // Arrange
            var result = new ThumbnailResult();

            // Assert initial (null)
            Assert.Null(result.FirstThumbnail);

            // Act
            result.Thumbnails.Add("thumb1.jpg");
            result.Thumbnails.Add("thumb2.jpg");

            // Assert (first)
            Assert.Equal("thumb1.jpg", result.FirstThumbnail);
        }
    }
}
