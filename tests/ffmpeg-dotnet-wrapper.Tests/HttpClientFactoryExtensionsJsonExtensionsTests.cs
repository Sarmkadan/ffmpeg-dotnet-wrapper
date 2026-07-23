// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FFmpegDotnetWrapper.Integration;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Unit tests for <see cref="HttpClientFactoryExtensionsJsonExtensions"/> JSON serialization extensions.
/// Tests serialization and deserialization of HttpClientConfig instances.
/// </summary>
public class HttpClientFactoryExtensionsJsonExtensionsTests
{
    /// <summary>
    /// Tests that ToJson serializes a valid HttpClientConfig with default options.
    /// Verifies that the JSON output is valid and contains expected properties.
    /// </summary>
    [Fact]
    public void ToJson_WithDefaultOptions_SerializesCorrectly()
    {
        // Arrange
        var config = new HttpClientConfig
        {
            WebhookTimeoutSeconds = 15,
            ProbeTimeoutSeconds = 30,
            MediaTransferTimeoutMinutes = 15,
            EnableRetries = false,
            MaxRetryAttempts = 2,
            InitialBackoffMs = 200
        };

        // Act
        var json = config.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("webhookTimeoutSeconds");
        json.Should().Contain("probeTimeoutSeconds");
        json.Should().Contain("mediaTransferTimeoutMinutes");
        json.Should().Contain("enableRetries");
        json.Should().Contain("maxRetryAttempts");
        json.Should().Contain("initialBackoffMs");
        json.Should().Contain("15");
        json.Should().Contain("30");
    }

    /// <summary>
    /// Tests that ToJson serializes with indented formatting when requested.
    /// Verifies that the indented output is more readable than compact format.
    /// </summary>
    [Fact]
    public void ToJson_WithIndentedTrue_FormatsWithNewlines()
    {
        // Arrange
        var config = new HttpClientConfig
        {
            WebhookTimeoutSeconds = 10
        };

        // Act
        var indentedJson = config.ToJson(indented: true);
        var compactJson = config.ToJson(indented: false);

        // Assert
        indentedJson.Should().NotBeNullOrWhiteSpace();
        compactJson.Should().NotBeNullOrWhiteSpace();

        // Indented should have newlines and be longer than compact
        indentedJson.Should().Contain("\n");
        indentedJson.Length.Should().BeGreaterThan(compactJson.Length);
    }

    /// <summary>
    /// Tests that FromJson deserializes valid JSON back to HttpClientConfig.
    /// Verifies round-trip serialization preserves all properties.
    /// </summary>
    [Fact]
    public void FromJson_WithValidJson_ReturnsDeserializedConfig()
    {
        // Arrange
        var originalConfig = new HttpClientConfig
        {
            WebhookTimeoutSeconds = 25,
            ProbeTimeoutSeconds = 45,
            MediaTransferTimeoutMinutes = 20,
            EnableRetries = true,
            MaxRetryAttempts = 5,
            InitialBackoffMs = 500
        };

        var json = originalConfig.ToJson();

        // Act
        var deserializedConfig = HttpClientFactoryExtensionsJsonExtensions.FromJson(json);

        // Assert
        deserializedConfig.Should().NotBeNull();
        deserializedConfig.WebhookTimeoutSeconds.Should().Be(originalConfig.WebhookTimeoutSeconds);
        deserializedConfig.ProbeTimeoutSeconds.Should().Be(originalConfig.ProbeTimeoutSeconds);
        deserializedConfig.MediaTransferTimeoutMinutes.Should().Be(originalConfig.MediaTransferTimeoutMinutes);
        deserializedConfig.EnableRetries.Should().Be(originalConfig.EnableRetries);
        deserializedConfig.MaxRetryAttempts.Should().Be(originalConfig.MaxRetryAttempts);
        deserializedConfig.InitialBackoffMs.Should().Be(originalConfig.InitialBackoffMs);
    }

    /// <summary>
    /// Tests that FromJson handles camelCase property names correctly.
    /// Verifies the JSON serializer respects the camelCase naming policy.
    /// </summary>
    [Fact]
    public void FromJson_WithCamelCaseProperties_DeserializesCorrectly()
    {
        // Arrange
        var json = "{\"webhookTimeoutSeconds\":10,\"probeTimeoutSeconds\":20,\"mediaTransferTimeoutMinutes\":5,\"enableRetries\":true,\"maxRetryAttempts\":3,\"initialBackoffMs\":100}";

        // Act
        var config = HttpClientFactoryExtensionsJsonExtensions.FromJson(json);

        // Assert
        config.Should().NotBeNull();
        config.WebhookTimeoutSeconds.Should().Be(10);
        config.ProbeTimeoutSeconds.Should().Be(20);
        config.MediaTransferTimeoutMinutes.Should().Be(5);
        config.EnableRetries.Should().BeTrue();
        config.MaxRetryAttempts.Should().Be(3);
        config.InitialBackoffMs.Should().Be(100);
    }

    /// <summary>
    /// Tests that TryFromJson returns true and deserializes valid JSON.
    /// Verifies the success path for TryFromJson.
    /// </summary>
    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializes()
    {
        // Arrange
        var json = "{\"webhookTimeoutSeconds\":12,\"probeTimeoutSeconds\":24,\"mediaTransferTimeoutMinutes\":8,\"enableRetries\":false,\"maxRetryAttempts\":1,\"initialBackoffMs\":50}";

        // Act
        var result = HttpClientFactoryExtensionsJsonExtensions.TryFromJson(json, out var deserializedConfig);

        // Assert
        result.Should().BeTrue();
        deserializedConfig.Should().NotBeNull();
        deserializedConfig!.WebhookTimeoutSeconds.Should().Be(12);
        deserializedConfig.ProbeTimeoutSeconds.Should().Be(24);
        deserializedConfig.MediaTransferTimeoutMinutes.Should().Be(8);
    }

    /// <summary>
    /// Tests that TryFromJson returns false for invalid JSON.
    /// Verifies the error path for TryFromJson.
    /// </summary>
    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid json";

        // Act
        var result = HttpClientFactoryExtensionsJsonExtensions.TryFromJson(invalidJson, out var deserializedConfig);

        // Assert
        result.Should().BeFalse();
        deserializedConfig.Should().BeNull();
    }

    /// <summary>
    /// Tests that FromJson throws ArgumentNullException for null input.
    /// Verifies the null check in FromJson.
    /// </summary>
    [Fact]
    public void FromJson_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string nullJson = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => HttpClientFactoryExtensionsJsonExtensions.FromJson(nullJson));
    }

    /// <summary>
    /// Tests that FromJson throws ArgumentException for empty/whitespace input.
    /// Verifies the whitespace check in FromJson.
    /// </summary>
    [Fact]
    public void FromJson_WithEmptyOrWhitespaceInput_ThrowsArgumentException()
    {
        // Arrange
        var emptyJson = string.Empty;
        var whitespaceJson = "   \n\t  ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => HttpClientFactoryExtensionsJsonExtensions.FromJson(emptyJson));
        Assert.Throws<ArgumentException>(() => HttpClientFactoryExtensionsJsonExtensions.FromJson(whitespaceJson));
    }

    /// <summary>
    /// Tests that TryFromJson throws ArgumentNullException for null input.
    /// Verifies the null check in TryFromJson.
    /// </summary>
    [Fact]
    public void TryFromJson_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string nullJson = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            HttpClientFactoryExtensionsJsonExtensions.TryFromJson(nullJson, out _));
    }

    /// <summary>
    /// Tests that TryFromJson throws ArgumentException for empty/whitespace input.
    /// Verifies the whitespace check in TryFromJson.
    /// </summary>
    [Fact]
    public void TryFromJson_WithEmptyOrWhitespaceInput_ThrowsArgumentException()
    {
        // Arrange
        var emptyJson = string.Empty;
        var whitespaceJson = "   \n\t  ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            HttpClientFactoryExtensionsJsonExtensions.TryFromJson(emptyJson, out _));
        Assert.Throws<ArgumentException>(() =>
            HttpClientFactoryExtensionsJsonExtensions.TryFromJson(whitespaceJson, out _));
    }

    /// <summary>
    /// Tests that ToJson throws ArgumentNullException for null HttpClientConfig.
    /// Verifies the null check in ToJson.
    /// </summary>
    [Fact]
    public void ToJson_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        HttpClientConfig nullConfig = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullConfig.ToJson());
    }

    /// <summary>
    /// Tests deserialization of default HttpClientConfig values.
    /// Verifies that default property values are correctly serialized and deserialized.
    /// </summary>
    [Fact]
    public void FromJson_WithDefaultValues_DeserializesCorrectly()
    {
        // Arrange
        var json = "{\"webhookTimeoutSeconds\":30,\"probeTimeoutSeconds\":60,\"mediaTransferTimeoutMinutes\":30,\"enableRetries\":true,\"maxRetryAttempts\":3,\"initialBackoffMs\":100}";

        // Act
        var config = HttpClientFactoryExtensionsJsonExtensions.FromJson(json);

        // Assert
        config.Should().NotBeNull();
        config.WebhookTimeoutSeconds.Should().Be(30);
        config.ProbeTimeoutSeconds.Should().Be(60);
        config.MediaTransferTimeoutMinutes.Should().Be(30);
        config.EnableRetries.Should().BeTrue();
        config.MaxRetryAttempts.Should().Be(3);
        config.InitialBackoffMs.Should().Be(100);
    }

    /// <summary>
    /// Tests that null values are not included in serialized JSON (default behavior).
    /// Verifies that null properties are omitted from output.
    /// </summary>
    [Fact]
    public void ToJson_WithNullProperties_OmitsNullValues()
    {
        // Arrange - Create config with some null-like values that should be omitted
        var config = new HttpClientConfig
        {
            WebhookTimeoutSeconds = 10
            // Other properties use defaults, not explicitly set to null
        };

        // Act
        var json = config.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        // Should not contain null-related properties since they're omitted by default
        json.Should().NotContain("null");
    }

    /// <summary>
    /// Tests round-trip serialization preserves all property values exactly.
    /// Verifies that serialization and deserialization is lossless.
    /// </summary>
    [Fact]
    public void RoundTripSerialization_PreservesAllPropertyValues()
    {
        // Arrange - Use boundary values
        var originalConfig = new HttpClientConfig
        {
            WebhookTimeoutSeconds = 1,
            ProbeTimeoutSeconds = 1,
            MediaTransferTimeoutMinutes = 1,
            EnableRetries = false,
            MaxRetryAttempts = 1,
            InitialBackoffMs = 1
        };

        // Act
        var json = originalConfig.ToJson();
        var deserializedConfig = HttpClientFactoryExtensionsJsonExtensions.FromJson(json);

        // Assert
        deserializedConfig.Should().NotBeNull();
        deserializedConfig.Should().BeEquivalentTo(originalConfig);
    }
}