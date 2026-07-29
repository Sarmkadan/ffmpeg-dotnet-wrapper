// SPDX-License-Identifier: MIT
// 2024 RedRocket

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using FFmpegDotnetWrapper.Utilities;

namespace FFmpegDotnetWrapper.Tests
{
    public class ProcessUtilitiesJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var processResult = new ProcessUtilities.ProcessResult();
            processResult.ExitCode = 0;
            processResult.StandardOutput = "output";
            processResult.StandardError = "error";

            // Act
            var json = ProcessUtilitiesJsonExtensions.ToJson(processResult);

            // Assert
            Assert.NotNull(json);
            Assert.NotEmpty(json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ProcessUtilitiesJsonExtensions.ToJson(null));
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsProcessResult()
        {
            // Arrange
            var json = "{\"ExitCode\":0,\"StandardOutput\":\"output\",\"StandardError\":\"error\"}";

            // Act
            var processResult = ProcessUtilitiesJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(processResult);
            Assert.Equal(0, processResult.ExitCode);
            Assert.Equal("output", processResult.StandardOutput);
            Assert.Equal("error", processResult.StandardError);
        }

        [Fact]
        public void FromJson_NullInput_ReturnsNull()
        {
            // Act
            var processResult = ProcessUtilitiesJsonExtensions.FromJson(null);

            // Assert
            Assert.Null(processResult);
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Act
            var processResult = ProcessUtilitiesJsonExtensions.FromJson("");

            // Assert
            Assert.Null(processResult);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"ExitCode\":0,\"StandardOutput\":\"output\",\"StandardError\":\"error\"}";

            // Act
            var success = ProcessUtilitiesJsonExtensions.TryFromJson(json, out var processResult);

            // Assert
            Assert.True(success);
            Assert.NotNull(processResult);
            Assert.Equal(0, processResult.ExitCode);
            Assert.Equal("output", processResult.StandardOutput);
            Assert.Equal("error", processResult.StandardError);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalse()
        {
            // Act
            var success = ProcessUtilitiesJsonExtensions.TryFromJson(null, out var processResult);

            // Assert
            Assert.False(success);
            Assert.Null(processResult);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Act
            var success = ProcessUtilitiesJsonExtensions.TryFromJson("", out var processResult);

            // Assert
            Assert.False(success);
            Assert.Null(processResult);
        }
    }
}
