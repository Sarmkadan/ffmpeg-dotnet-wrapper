// SPDX-License-Identifier: MIT
// Copyright © 2024

using System;
using FFmpegDotnetWrapper.Exceptions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class ConfigurationExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageAndLeavesOtherPropertiesNull()
    {
        // Arrange
        var message = "Configuration error occurred";

        // Act
        var ex = new ConfigurationException(message);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Null(ex.ConfigurationKey);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageAndKey_SetsMessageAndKey()
    {
        // Arrange
        var message = "Missing configuration value";
        var key = "FFmpeg:Path";

        // Act
        var ex = new ConfigurationException(message, key);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Equal(key, ex.ConfigurationKey);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsMessageAndInnerException()
    {
        // Arrange
        var message = "Invalid configuration combination";
        var inner = new InvalidOperationException("Invalid operation");

        // Act
        var ex = new ConfigurationException(message, inner);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Null(ex.ConfigurationKey);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageKeyAndInnerException_SetsAllProperties()
    {
        // Arrange
        var message = "Configuration violates constraints";
        var key = "Streaming:MaxBitrate";
        var inner = new ArgumentOutOfRangeException("bitrate", "Value too high");

        // Act
        var ex = new ConfigurationException(message, key, inner);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Equal(key, ex.ConfigurationKey);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_WithNullKey_LeavesConfigurationKeyNull()
    {
        // Arrange
        var message = "Null key test";

        // Act
        var ex = new ConfigurationException(message, (string?)null);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Null(ex.ConfigurationKey);
    }

    [Fact]
    public void Constructor_WithNullInnerException_LeavesInnerExceptionNull()
    {
        // Arrange
        var message = "Null inner exception test";

        // Act
        var ex = new ConfigurationException(message, (Exception?)null);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Constructor_WithNullKeyAndNullInnerException_SetsMessageOnly()
    {
        // Arrange
        var message = "Both null test";

        // Act
        var ex = new ConfigurationException(message, (string?)null, (Exception?)null);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Null(ex.ConfigurationKey);
        Assert.Null(ex.InnerException);
    }
}
