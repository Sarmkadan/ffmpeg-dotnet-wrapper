namespace FFmpegDotnetWrapper.Tests;

using Xunit;
using FluentAssertions;
using FFmpegDotnetWrapper.Exceptions;
using System;
using System.Collections.Generic;

public class OptionsValidationExceptionTests
{
    [Fact]
    public void Constructor_WithViolations_ExposesViolationsInOrder()
    {
        var violations = new List<string> { "first rule", "second rule" };

        var exception = new OptionsValidationException(violations);

        exception.Violations.Should().Equal("first rule", "second rule");
    }

    [Fact]
    public void Constructor_WithViolations_IsValidationException()
    {
        var exception = new OptionsValidationException(new[] { "rule" });

        exception.Should().BeAssignableTo<ValidationException>();
    }

    [Fact]
    public void Constructor_WithViolations_MessageListsEachViolation()
    {
        var exception = new OptionsValidationException(new[] { "first rule", "second rule" });

        exception.Message.Should().Contain("Invalid options (2 violation(s)):");
        exception.Message.Should().Contain("  - first rule");
        exception.Message.Should().Contain("  - second rule");
    }

    [Fact]
    public void Constructor_WithNullViolations_ThrowsArgumentNullException()
    {
        Action act = () => _ = new OptionsValidationException(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNoViolations_ThrowsArgumentException()
    {
        Action act = () => _ = new OptionsValidationException(Array.Empty<string>());

        act.Should().Throw<ArgumentException>();
    }
}
