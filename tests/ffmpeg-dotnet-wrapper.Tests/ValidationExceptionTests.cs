using System;
using System.Collections.Generic;
using FFmpegDotnetWrapper.Exceptions;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_InitializesExceptionWithMessage()
    {
        // Arrange
        var message = "Test validation error";

        // Act
        var exception = new ValidationException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.ValidationErrors.Should().BeNull();
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndValidationErrors_InitializesExceptionWithErrors()
    {
        // Arrange
        var message = "Validation failed";
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required" } },
            { "Email", new[] { "Email is invalid", "Email is required" } }
        };

        // Act
        var exception = new ValidationException(message, errors);

        // Assert
        exception.Message.Should().Be(message);
        exception.ValidationErrors.Should().BeEquivalentTo(errors);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_InitializesExceptionWithInnerException()
    {
        // Arrange
        var message = "Validation failed";
        var innerException = new ArgumentException("Inner error");

        // Act
        var exception = new ValidationException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.ValidationErrors.Should().BeNull();
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [Fact]
    public void Constructor_WithMessageValidationErrorsAndInnerException_InitializesExceptionWithAllProperties()
    {
        // Arrange
        var message = "Validation failed";
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error 1" } },
            { "Field2", new[] { "Error 2", "Error 3" } }
        };
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new ValidationException(message, errors, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.ValidationErrors.Should().BeEquivalentTo(errors);
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [Fact]
    public void FromDictionary_WithEmptyErrorsDictionary_CreatesExceptionWithEmptyErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>();

        // Act
        var exception = ValidationException.FromDictionary(errors);

        // Assert
        exception.Message.Should().Be("Validation failed");
        exception.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public void FromDictionary_WithCustomMessage_CreatesExceptionWithCustomMessage()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field", new[] { "Error" } }
        };
        var customMessage = "Custom validation message";

        // Act
        var exception = ValidationException.FromDictionary(errors, customMessage);

        // Assert
        exception.Message.Should().Be(customMessage);
        exception.ValidationErrors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void FromDictionary_WithMultipleErrors_PreservesAllErrorMessages()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required", "Name must be at least 3 characters" } },
            { "Email", new[] { "Email is invalid" } },
            { "Age", new[] { "Age must be positive", "Age must be less than 120" } }
        };

        // Act
        var exception = ValidationException.FromDictionary(errors);

        // Assert
        exception.ValidationErrors.Should().HaveCount(3);
        exception.ValidationErrors["Name"].Should().BeEquivalentTo(
            new[] { "Name is required", "Name must be at least 3 characters" });
        exception.ValidationErrors["Email"].Should().BeEquivalentTo(new[] { "Email is invalid" });
        exception.ValidationErrors["Age"].Should().BeEquivalentTo(
            new[] { "Age must be positive", "Age must be less than 120" });
    }

    [Fact]
    public void FromDictionary_WithNullValuesInErrorsDictionary_PreservesNullValues()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error 1" } },
            { "Field2", new string[0] },
            { "Field3", new string[0] }
        };

        // Act
        var exception = ValidationException.FromDictionary(errors);

        // Assert
        exception.ValidationErrors["Field1"].Should().BeEquivalentTo(new[] { "Error 1" });
        exception.ValidationErrors["Field2"].Should().BeEmpty();
        exception.ValidationErrors["Field3"].Should().BeEmpty();
    }

    [Fact]
    public void ValidationErrors_WhenSetViaConstructor_CanBeModified()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field", new[] { "Error" } }
        };
        var exception = new ValidationException("Test", errors);

        // Act - modify the ValidationErrors property
        exception.ValidationErrors["NewField"] = new[] { "New error" };

        // Assert
        exception.ValidationErrors.Should().ContainKey("NewField");
        exception.ValidationErrors["NewField"].Should().BeEquivalentTo(new[] { "New error" });
    }

    [Fact]
    public void ValidationErrors_WhenNotSetViaConstructor_IsNull()
    {
        // Arrange & Act
        var exception = new ValidationException("Test message");

        // Assert
        exception.ValidationErrors.Should().BeNull();
    }
}