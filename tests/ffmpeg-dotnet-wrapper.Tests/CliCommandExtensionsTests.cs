using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FFmpegDotnetWrapper.Cli;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class CliCommandExtensionsTests
{
    private static CliCommand CreateCommandWithOptions(IDictionary<string, string> options)
    {
        var command = new CliCommand();
        // Assuming Options is a public mutable dictionary; if not, use reflection or appropriate API.
        foreach (var kvp in options)
        {
            command.Options[kvp.Key] = kvp.Value;
        }

        return command;
    }

    #region HasOption

    [Fact]
    public void HasOption_ShouldReturnTrue_WhenOptionExists()
    {
        // Arrange
        var command = CreateCommandWithOptions(new Dictionary<string, string>
        {
            { "output", "file.mp4" }
        });

        // Act
        var result = command.HasOption("output");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasOption_ShouldReturnFalse_WhenOptionDoesNotExist()
    {
        // Arrange
        var command = CreateCommandWithOptions(new Dictionary<string, string>());

        // Act
        var result = command.HasOption("missing");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasOption_ShouldThrowArgumentNullException_WhenCommandIsNull()
    {
        // Act
        Action act = () => ((CliCommand)null!).HasOption("any");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void HasOption_ShouldThrowArgumentException_WhenOptionNameIsInvalid(string? optionName)
    {
        // Arrange
        var command = new CliCommand();

        // Act
        Action act = () => command.HasOption(optionName!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region GetOptionValue

    [Fact]
    public void GetOptionValue_ShouldReturnValue_WhenOptionExists()
    {
        // Arrange
        var command = CreateCommandWithOptions(new Dictionary<string, string>
        {
            { "codec", "h264" }
        });

        // Act
        var value = command.GetOptionValue("codec");

        // Assert
        value.Should().Be("h264");
    }

    [Fact]
    public void GetOptionValue_ShouldReturnNull_WhenOptionDoesNotExist()
    {
        // Arrange
        var command = CreateCommandWithOptions(new Dictionary<string, string>());

        // Act
        var value = command.GetOptionValue("nonexistent");

        // Assert
        value.Should().BeNull();
    }

    [Fact]
    public void GetOptionValue_ShouldThrowArgumentNullException_WhenCommandIsNull()
    {
        // Act
        Action act = () => ((CliCommand)null!).GetOptionValue("any");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetOptionValue_ShouldThrowArgumentException_WhenOptionNameIsInvalid(string? optionName)
    {
        // Arrange
        var command = new CliCommand();

        // Act
        Action act = () => command.GetOptionValue(optionName!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region TryGetOptionValue

    [Fact]
    public void TryGetOptionValue_ShouldReturnTrueAndValue_WhenOptionExists()
    {
        // Arrange
        var command = CreateCommandWithOptions(new Dictionary<string, string>
        {
            { "fps", "24" }
        });

        // Act
        var result = command.TryGetOptionValue("fps", out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be("24");
    }

    [Fact]
    public void TryGetOptionValue_ShouldReturnFalseAndNull_WhenOptionDoesNotExist()
    {
        // Arrange
        var command = CreateCommandWithOptions(new Dictionary<string, string>());

        // Act
        var result = command.TryGetOptionValue("missing", out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryGetOptionValue_ShouldThrowArgumentNullException_WhenCommandIsNull()
    {
        // Act
        Action act = () => ((CliCommand)null!).TryGetOptionValue("any", out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TryGetOptionValue_ShouldThrowArgumentException_WhenOptionNameIsInvalid(string? optionName)
    {
        // Arrange
        var command = new CliCommand();

        // Act
        Action act = () => command.TryGetOptionValue(optionName!, out _);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion
}
