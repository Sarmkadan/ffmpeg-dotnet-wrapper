namespace ffmpeg_dotnet_wrapper_tests
{
    using Xunit;
    using FFmpegDotnetWrapper.Models;
    using System;

    public class WatermarkSettingsExtensionsTests
    {
        [Fact]
        public void WithTopLeftPosition_WithValidSettings_ReturnsNewInstanceWithTopLeftPosition()
        {
            // Arrange
            var settings = new WatermarkSettings { XOffset = 20, YOffset = 30, Scale = 0.5 };

            // Act
            var result = settings.WithTopLeftPosition();

            // Assert
            Assert.NotSame(settings, result);
            Assert.Equal(WatermarkPosition.TopLeft, result.Position);
            Assert.Equal(20, result.XOffset);
            Assert.Equal(30, result.YOffset);
            Assert.Equal(0.5, result.Scale);
        }

        [Fact]
        public void WithTopLeftPosition_NullSettings_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => ((WatermarkSettings)null!).WithTopLeftPosition());

        [Fact]
        public void WithCenterPosition_WithValidSettings_ReturnsNewInstanceWithCenterPosition()
        {
            // Arrange
            var settings = new WatermarkSettings { XOffset = 20, YOffset = 30, Scale = 0.5 };

            // Act
            var result = settings.WithCenterPosition();

            // Assert
            Assert.NotSame(settings, result);
            Assert.Equal(WatermarkPosition.Center, result.Position);
            Assert.Equal(0, result.XOffset);
            Assert.Equal(0, result.YOffset);
        }

        [Fact]
        public void WithCenterPosition_NullSettings_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => ((WatermarkSettings)null!).WithCenterPosition());

        [Fact]
        public void WithScale_ValidScalePercentage_ReturnsNewInstanceWithCorrectScale()
        {
            // Arrange
            var settings = new WatermarkSettings();

            // Act
            var result = settings.WithScale(50.0);

            // Assert
            Assert.NotSame(settings, result);
            Assert.Equal(0.5, result.Scale);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(100.0)]
        public void WithScale_BoundaryValues_ReturnsCorrectScale(double scalePercentage)
        {
            // Arrange
            var settings = new WatermarkSettings();

            // Act
            var result = settings.WithScale(scalePercentage);

            // Assert
            Assert.Equal(scalePercentage / 100.0, result.Scale);
        }

        [Fact]
        public void WithScale_NullSettings_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => ((WatermarkSettings)null!).WithScale(50.0));

        [Theory]
        [InlineData(-1.0)]
        [InlineData(101.0)]
        public void WithScale_OutOfRangeScalePercentage_ThrowsArgumentOutOfRangeException(double scalePercentage) =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new WatermarkSettings().WithScale(scalePercentage));

        [Fact]
        public void WithAnimation_ValidDuration_ReturnsNewInstanceWithAnimationEnabled()
        {
            // Arrange
            var settings = new WatermarkSettings { Scale = 0.3 };
            var duration = TimeSpan.FromSeconds(2.5);

            // Act
            var result = settings.WithAnimation(duration);

            // Assert
            Assert.NotSame(settings, result);
            Assert.True(result.AnimateIn);
            Assert.Equal(duration, result.AnimateInDuration);
            Assert.Equal(0.3, result.Scale);
        }

        [Fact]
        public void WithAnimation_ZeroDuration_ReturnsNewInstanceWithZeroDuration()
        {
            // Arrange
            var settings = new WatermarkSettings();

            // Act
            var result = settings.WithAnimation(TimeSpan.Zero);

            // Assert
            Assert.NotSame(settings, result);
            Assert.True(result.AnimateIn);
            Assert.Equal(TimeSpan.Zero, result.AnimateInDuration);
        }

        [Fact]
        public void WithAnimation_NullSettings_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => ((WatermarkSettings)null!).WithAnimation(TimeSpan.FromSeconds(1)));

        [Fact]
        public void WithAnimation_NegativeDuration_ThrowsArgumentOutOfRangeException() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new WatermarkSettings().WithAnimation(TimeSpan.FromSeconds(-1)));

        [Fact]
        public void WithTimeConstraints_ValidStartTimeAndDuration_ReturnsNewInstanceWithTimeConstraints()
        {
            // Arrange
            var settings = new WatermarkSettings { XOffset = 15, Scale = 0.4 };
            var startTime = TimeSpan.FromSeconds(10);
            var duration = TimeSpan.FromSeconds(5);

            // Act
            var result = settings.WithTimeConstraints(startTime, duration);

            // Assert
            Assert.NotSame(settings, result);
            Assert.Equal(startTime, result.StartTime);
            Assert.Equal(duration, result.Duration);
            Assert.Equal(15, result.XOffset);
            Assert.Equal(0.4, result.Scale);
        }

        [Fact]
        public void WithTimeConstraints_ZeroStartTimeAndPositiveDuration_ReturnsNewInstance()
        {
            // Arrange
            var settings = new WatermarkSettings();

            // Act
            var result = settings.WithTimeConstraints(TimeSpan.Zero, TimeSpan.FromSeconds(3));

            // Assert
            Assert.NotSame(settings, result);
            Assert.Equal(TimeSpan.Zero, result.StartTime);
            Assert.Equal(TimeSpan.FromSeconds(3), result.Duration);
        }

        [Fact]
        public void WithTimeConstraints_NullSettings_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => ((WatermarkSettings)null!).WithTimeConstraints(TimeSpan.Zero, TimeSpan.FromSeconds(5)));

        [Fact]
        public void WithTimeConstraints_NegativeStartTime_ThrowsArgumentOutOfRangeException() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new WatermarkSettings().WithTimeConstraints(TimeSpan.FromSeconds(-5), TimeSpan.FromSeconds(5)));

        [Fact]
        public void WithTimeConstraints_ZeroDuration_ThrowsArgumentOutOfRangeException() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new WatermarkSettings().WithTimeConstraints(TimeSpan.Zero, TimeSpan.Zero));

        [Fact]
        public void WithTimeConstraints_NegativeDuration_ThrowsArgumentOutOfRangeException() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new WatermarkSettings().WithTimeConstraints(TimeSpan.Zero, TimeSpan.FromSeconds(-1)));

        [Fact]
        public void WithOpacity_ValidOpacity_ReturnsNewInstanceWithCorrectOpacity()
        {
            // Arrange
            var settings = new WatermarkSettings();

            // Act
            var result = settings.WithOpacity(0.75);

            // Assert
            Assert.NotSame(settings, result);
            Assert.Equal(0.75, result.Opacity);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        public void WithOpacity_BoundaryValues_ReturnsCorrectOpacity(double opacity)
        {
            // Arrange
            var settings = new WatermarkSettings();

            // Act
            var result = settings.WithOpacity(opacity);

            // Assert
            Assert.Equal(opacity, result.Opacity);
        }

        [Fact]
        public void WithOpacity_NullSettings_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => ((WatermarkSettings)null!).WithOpacity(0.5));

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        public void WithOpacity_OutOfRangeOpacity_ThrowsArgumentOutOfRangeException(double opacity) =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new WatermarkSettings().WithOpacity(opacity));
    }
}
