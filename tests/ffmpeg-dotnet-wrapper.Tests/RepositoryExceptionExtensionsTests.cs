using System;
using FFmpegDotnetWrapper.Exceptions;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests.Exceptions;

public class RepositoryExceptionExtensionsTests
{
    [Fact]
    public void IsRepositoryNotFound_WithNotFoundMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new RepositoryException("Repository not found");

        // Act
        var result = exception.IsRepositoryNotFound();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRepositoryNotFound_WithDoesNotExistMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new RepositoryException("Repository does not exist");

        // Act
        var result = exception.IsRepositoryNotFound();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRepositoryNotFound_WithCaseInsensitiveMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new RepositoryException("REPOSITORY NOT FOUND");

        // Act
        var result = exception.IsRepositoryNotFound();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRepositoryNotFound_WithDifferentMessage_ReturnsFalse()
    {
        // Arrange
        var exception = new RepositoryException("Repository already exists");

        // Act
        var result = exception.IsRepositoryNotFound();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRepositoryNotFound_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        RepositoryException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.IsRepositoryNotFound());
    }

    [Fact]
    public void IsRepositoryAlreadyExists_WithAlreadyExistsMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new RepositoryException("Repository already exists");

        // Act
        var result = exception.IsRepositoryAlreadyExists();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRepositoryAlreadyExists_WithAlreadyPresentMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new RepositoryException("Repository already present");

        // Act
        var result = exception.IsRepositoryAlreadyExists();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRepositoryAlreadyExists_WithCaseInsensitiveMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new RepositoryException("REPOSITORY ALREADY EXISTS");

        // Act
        var result = exception.IsRepositoryAlreadyExists();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRepositoryAlreadyExists_WithDifferentMessage_ReturnsFalse()
    {
        // Arrange
        var exception = new RepositoryException("Repository not found");

        // Act
        var result = exception.IsRepositoryAlreadyExists();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRepositoryAlreadyExists_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        RepositoryException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.IsRepositoryAlreadyExists());
    }

    [Fact]
    public void IsAccessDenied_WithAccessDeniedMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new RepositoryException("Access denied to repository");

        // Act
        var result = exception.IsAccessDenied();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAccessDenied_WithPermissionDeniedMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new RepositoryException("Permission denied for repository operation");

        // Act
        var result = exception.IsAccessDenied();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAccessDenied_WithInsufficientPermissionsMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new RepositoryException("Insufficient permissions to access repository");

        // Act
        var result = exception.IsAccessDenied();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAccessDenied_WithCaseInsensitiveMessage_ReturnsTrue()
    {
        // Arrange
        var exception = new RepositoryException("ACCESS DENIED TO REPOSITORY");

        // Act
        var result = exception.IsAccessDenied();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAccessDenied_WithDifferentMessage_ReturnsFalse()
    {
        // Arrange
        var exception = new RepositoryException("Repository not found");

        // Act
        var result = exception.IsAccessDenied();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAccessDenied_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        RepositoryException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.IsAccessDenied());
    }

    [Fact]
    public void WithContext_WithValidAdditionalContext_ReturnsNewExceptionWithCombinedMessage()
    {
        // Arrange
        var originalException = new RepositoryException("Original repository error", "test-repo");
        var additionalContext = "Additional context about the error";

        // Act
        var result = originalException.WithContext(additionalContext);

        // Assert
        result.Should().NotBeSameAs(originalException);
        result.Message.Should().Be("Original repository error | Context: Additional context about the error");
        result.RepositoryName.Should().Be("test-repo");
        result.InnerException.Should().BeSameAs(originalException);
    }

    [Fact]
    public void WithContext_WithEmptyAdditionalContext_ReturnsNewExceptionWithEmptyContext()
    {
        // Arrange
        var originalException = new RepositoryException("Original repository error", "test-repo");
        var additionalContext = string.Empty;

        // Act
        var result = originalException.WithContext(additionalContext);

        // Assert
        result.Should().NotBeSameAs(originalException);
        result.Message.Should().Be("Original repository error | Context: ");
        result.RepositoryName.Should().Be("test-repo");
        result.InnerException.Should().BeSameAs(originalException);
    }

    [Fact]
    public void WithContext_WithWhitespaceAdditionalContext_ReturnsNewExceptionWithWhitespaceContext()
    {
        // Arrange
        var originalException = new RepositoryException("Original repository error", "test-repo");
        var additionalContext = "   ";

        // Act
        var result = originalException.WithContext(additionalContext);

        // Assert
        result.Should().NotBeSameAs(originalException);
        result.Message.Should().Be("Original repository error | Context:    ");
        result.RepositoryName.Should().Be("test-repo");
        result.InnerException.Should().BeSameAs(originalException);
    }

    [Fact]
    public void WithContext_WithNullAdditionalContext_ThrowsArgumentNullException()
    {
        // Arrange
        var originalException = new RepositoryException("Original repository error", "test-repo");
        string additionalContext = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => originalException.WithContext(additionalContext));
    }

    [Fact]
    public void WithContext_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        RepositoryException originalException = null!;
        var additionalContext = "Additional context";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => originalException.WithContext(additionalContext));
    }

    [Fact]
    public void WithContext_WithInnerException_PreservesInnerException()
    {
        // Arrange
        var innerException = new Exception("Inner error");
        var originalException = new RepositoryException("Original repository error", "test-repo", innerException);
        var additionalContext = "Additional context";

        // Act
        var result = originalException.WithContext(additionalContext);

        // Assert
        result.InnerException.Should().BeSameAs(originalException);
    }

    [Fact]
    public void WithContext_WithComplexMessage_CombinesMessagesCorrectly()
    {
        // Arrange
        var originalException = new RepositoryException("Failed to access repository: my-repo", "my-repo");
        var additionalContext = "Operation: DeleteRepository, Attempt: 2";

        // Act
        var result = originalException.WithContext(additionalContext);

        // Assert
        result.Message.Should().Be("Failed to access repository: my-repo | Context: Operation: DeleteRepository, Attempt: 2");
        result.RepositoryName.Should().Be("my-repo");
    }
}