// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Serialization;

using FluentAssertions;

using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Unit tests for the <see cref="JsonOutputFormatterValidation"/> class.
/// </summary>
public class JsonOutputFormatterValidationTests
{
    /// <summary>
    /// Tests that Validate returns an empty error list for a non-null instance.
    /// </summary>
    [Fact]
    public void Validate_ShouldReturnNoErrors_WhenInstanceIsNotNull()
    {
        object value = new object();

        var errors = value.Validate();

        errors.Should().NotBeNull();
        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Validate throws ArgumentNullException for a null instance.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrowArgumentNullException_WhenValueIsNull()
    {
        object? value = null;

        Action act = () => value.Validate();

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that IsValid returns true for non-null instances, including boundary-like values.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(0)]
    [InlineData(false)]
    public void IsValid_ShouldReturnTrue_WhenInstanceIsNotNull(object value)
    {
        value.IsValid().Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsValid throws ArgumentNullException for a null instance.
    /// </summary>
    [Fact]
    public void IsValid_ShouldThrowArgumentNullException_WhenValueIsNull()
    {
        object? value = null;

        Action act = () => value.IsValid();

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that EnsureValid completes without throwing for a non-null instance.
    /// </summary>
    [Fact]
    public void EnsureValid_ShouldNotThrow_WhenInstanceIsNotNull()
    {
        object value = new object();

        Action act = () => value.EnsureValid();

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that EnsureValid throws ArgumentNullException for a null instance.
    /// </summary>
    [Fact]
    public void EnsureValid_ShouldThrowArgumentNullException_WhenValueIsNull()
    {
        object? value = null;

        Action act = () => value.EnsureValid();

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }
}
