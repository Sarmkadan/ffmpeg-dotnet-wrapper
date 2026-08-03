using System;
using System.Collections.Generic;
using FluentAssertions;
using FFmpegDotnetWrapper.Models;
using Xunit;

namespace FFmpegDotnetWrapper.Models.Tests
{
    /// <summary>
    /// Unit tests for <see cref="ConversionResult"/>.
    /// </summary>
    public class ConversionResultTests
    {
        [Fact]
        public void NewConversionResult_HasNonEmptyIdAndCreatedAtIsSet()
        {
            // Act
            var result = new ConversionResult();

            // Assert
            result.Id.Should().NotBeNullOrEmpty();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.IsSuccess.Should().BeFalse(); // default
        }

        [Fact]
        public void MarkAsSuccess_SetsPropertiesCorrectly()
        {
            // Arrange
            var result = new ConversionResult
            {
                Duration = TimeSpan.FromSeconds(12.34)
            };
            var outputPath = "/tmp/output.mp4";

            // Act
            result.MarkAsSuccess(outputPath, exitCode: 0);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.OutputFilePath.Should().Be(outputPath);
            result.ExitCode.Should().Be(0);
            result.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.ErrorMessage.Should().BeNull();
            result.ErrorOutput.Should().BeNull();
        }

        [Fact]
        public void MarkAsFailed_SetsPropertiesCorrectly()
        {
            // Arrange
            var result = new ConversionResult
            {
                Duration = TimeSpan.FromSeconds(5)
            };
            var errorMessage = "Unsupported codec";
            var errorOutput = "Invalid codec combination";

            // Act
            result.MarkAsFailed(errorMessage, exitCode: 1, errorOutput: errorOutput);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be(errorMessage);
            result.ExitCode.Should().Be(1);
            result.ErrorOutput.Should().Be(errorOutput);
            result.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void GetSizeReductionPercentage_ReturnsNullWhenNotSuccessfulOrMissingMedia()
        {
            // Arrange
            var result = new ConversionResult();

            // Act / Assert
            result.GetSizeReductionPercentage(1_000_000).Should().BeNull();

            // Make it successful but still without OutputMedia
            result.MarkAsSuccess("/tmp/out.mp4");
            result.GetSizeReductionPercentage(1_000_000).Should().BeNull();
        }

        [Fact]
        public void SetMetric_And_GetMetric_WorkAsExpected()
        {
            // Arrange
            var result = new ConversionResult();

            // Act
            result.SetMetric("bitrate", 2500);
            result.SetMetric("profile", "high");

            // Assert
            result.GetMetric<int>("bitrate").Should().Be(2500);
            result.GetMetric<string>("profile").Should().Be("high");
            result.GetMetric<double>("nonexistent").Should().Be(default);
        }

        [Fact]
        public void GenerateSummary_ContainsExpectedSections()
        {
            // Arrange
            var result = new ConversionResult
            {
                Duration = TimeSpan.FromSeconds(3.5)
            };
            result.MarkAsSuccess("/tmp/out.mp4");
            result.SetMetric("speed", "2x");
            result.WarningMessage = "Low bitrate";

            // Act
            var summary = result.GenerateSummary();

            // Assert
            summary.Should().Contain($"Conversion ID: {result.Id}");
            summary.Should().Contain("Status: Success");
            summary.Should().Contain("Duration: 3.50 seconds");
            summary.Should().Contain("Exit Code: 0");
            summary.Should().Contain("Output: /tmp/out.mp4");
            summary.Should().Contain("Metrics:");
            summary.Should().Contain(" speed: 2x");
            summary.Should().Contain("Warning: Low bitrate");
        }
    }
}
