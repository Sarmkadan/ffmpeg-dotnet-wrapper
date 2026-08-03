using Moq;
using Xunit;
using FluentAssertions;
using FFmpegDotnetWrapper.Services;

namespace FFmpegDotnetWrapper.Tests;

public class FFmpegServiceValidationTests
{
    [Fact]
    public void Validate_NullService_ThrowsArgumentNullException()
    {
        // Arrange
        IFFmpegService? service = null;

        // Act
        Action act = () => FFmpegServiceValidation.Validate(service!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_ValidService_ReturnsEmptyList()
    {
        // Arrange
        var mockService = new Mock<IFFmpegService>();

        // Act
        var result = FFmpegServiceValidation.Validate(mockService.Object);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_NullService_ReturnsFalse()
    {
        // Arrange
        IFFmpegService? service = null;

        // Act
        var result = FFmpegServiceValidation.IsValid(service);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ValidService_ReturnsTrue()
    {
        // Arrange
        var mockService = new Mock<IFFmpegService>();

        // Act
        var result = FFmpegServiceValidation.IsValid(mockService.Object);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EnsureValid_NullService_ThrowsArgumentNullException()
    {
        // Arrange
        IFFmpegService? service = null;

        // Act
        Action act = () => FFmpegServiceValidation.EnsureValid(service!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_ValidService_DoesNotThrow()
    {
        // Arrange
        var mockService = new Mock<IFFmpegService>();

        // Act
        Action act = () => FFmpegServiceValidation.EnsureValid(mockService.Object);

        // Assert
        act.Should().NotThrow();
    }
}
