// SPDX-License-Identifier: MIT
// © 2024 RedRocket

using System;
using FFmpegDotnetWrapper.Configuration;
using Xunit;

namespace FFmpegDotnetWrapper.Tests
{
    public class FFmpegOptionsTests
    {
        [Fact]
        public void DefaultValues_ShouldMatchExpected()
        {
            // Arrange
            var options = new FFmpegOptions();

            // Assert
            Assert.Null(options.FFmpegPath);
            Assert.Null(options.FFprobePath);
            Assert.Equal(600, options.OperationTimeoutSeconds);
            Assert.Equal(50L * 1024 * 1024 * 1024, options.MaxFileSizeBytes);
            Assert.False(options.EnableHardwareAcceleration);
            Assert.Equal("medium", options.EncodingPreset);
            Assert.False(options.KeepTemporaryFiles);
            Assert.Null(options.TemporaryDirectory);
            Assert.False(options.VerboseLogging);
            Assert.Equal(23, options.DefaultQuality);
        }

        [Fact]
        public void CanSetAndGetStringProperties()
        {
            // Arrange
            var options = new FFmpegOptions
            {
                FFmpegPath = @"C:\ffmpeg\ffmpeg.exe",
                FFprobePath = @"C:\ffmpeg\ffprobe.exe",
                EncodingPreset = "fast",
                TemporaryDirectory = "/tmp/ffmpeg"
            };

            // Assert
            Assert.Equal(@"C:\ffmpeg\ffmpeg.exe", options.FFmpegPath);
            Assert.Equal(@"C:\ffmpeg\ffprobe.exe", options.FFprobePath);
            Assert.Equal("fast", options.EncodingPreset);
            Assert.Equal("/tmp/ffmpeg", options.TemporaryDirectory);
        }

        [Fact]
        public void CanSetAndGetNumericAndNullableProperties()
        {
            // Arrange
            var options = new FFmpegOptions
            {
                OperationTimeoutSeconds = 120,
                MaxFileSizeBytes = 10L * 1024 * 1024 * 1024, // 10 GB
                DefaultQuality = null
            };

            // Assert
            Assert.Equal(120, options.OperationTimeoutSeconds);
            Assert.Equal(10L * 1024 * 1024 * 1024, options.MaxFileSizeBytes);
            Assert.Null(options.DefaultQuality);
        }

        [Fact]
        public void BooleanProperties_ShouldDefaultAndBeSettable()
        {
            // Arrange
            var options = new FFmpegOptions
            {
                EnableHardwareAcceleration = true,
                KeepTemporaryFiles = true,
                VerboseLogging = true
            };

            // Assert
            Assert.True(options.EnableHardwareAcceleration);
            Assert.True(options.KeepTemporaryFiles);
            Assert.True(options.VerboseLogging);
        }

        [Fact]
        public void SettingNullStringProperties_ShouldNotThrow()
        {
            // Arrange & Act
            var options = new FFmpegOptions
            {
                FFmpegPath = null,
                FFprobePath = null,
                EncodingPreset = null,
                TemporaryDirectory = null
            };

            // Assert
            Assert.Null(options.FFmpegPath);
            Assert.Null(options.FFprobePath);
            Assert.Null(options.EncodingPreset);
            Assert.Null(options.TemporaryDirectory);
        }

        [Fact]
        public void SettingBoundaryNumericValues_ShouldPersist()
        {
            // Arrange
            var options = new FFmpegOptions
            {
                OperationTimeoutSeconds = 0,
                MaxFileSizeBytes = 0,
                DefaultQuality = 0
            };

            // Assert
            Assert.Equal(0, options.OperationTimeoutSeconds);
            Assert.Equal(0, options.MaxFileSizeBytes);
            Assert.Equal(0, options.DefaultQuality);
        }
    }
}
