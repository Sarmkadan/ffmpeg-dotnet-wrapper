using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Events;
using FFmpegDotnetWrapper.Integration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class WebhookEndpointTests
{
    private readonly Mock<ILogger<WebhookService>> _loggerMock = new();
    private readonly Mock<HttpClient> _httpClientMock = new();
    private readonly WebhookService _webhookService;

    public WebhookEndpointTests()
    {
        _webhookService = new WebhookService(_loggerMock.Object, _httpClientMock.Object);
    }

    [Fact]
    public void WebhookEndpoint_Properties_ShouldHaveCorrectDefaultValues()
    {
        // Arrange & Act
        var endpoint = new WebhookEndpoint();

        // Assert
        endpoint.WebhookId.Should().NotBeNullOrEmpty();
        Guid.TryParse(endpoint.WebhookId, out _).Should().BeTrue();
        endpoint.Url.Should().BeEmpty();
        endpoint.EventTypes.Should().BeEmpty();
        endpoint.AuthToken.Should().BeNull();
        endpoint.IsActive.Should().BeTrue();
        endpoint.MaxRetries.Should().Be(3);
        endpoint.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        endpoint.Headers.Should().BeEmpty();
    }

    [Fact]
    public void WebhookEndpoint_WithParameters_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var url = "https://example.com/webhook";
        var eventTypes = new List<string> { "OperationCompletedEvent", "OperationFailedEvent" };
        var authToken = "test-token-123";
        var headers = new Dictionary<string, string> { { "X-Custom", "value" } };
        var createdAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var endpoint = new WebhookEndpoint
        {
            WebhookId = "test-id-123",
            Url = url,
            EventTypes = eventTypes,
            AuthToken = authToken,
            IsActive = false,
            MaxRetries = 5,
            CreatedAt = createdAt,
            Headers = headers
        };

        // Assert
        endpoint.WebhookId.Should().Be("test-id-123");
        endpoint.Url.Should().Be(url);
        endpoint.EventTypes.Should().BeEquivalentTo(eventTypes);
        endpoint.AuthToken.Should().Be(authToken);
        endpoint.IsActive.Should().BeFalse();
        endpoint.MaxRetries.Should().Be(5);
        endpoint.CreatedAt.Should().Be(createdAt);
        endpoint.Headers.Should().BeEquivalentTo(headers);
    }

    [Fact]
    public async Task RegisterWebhookAsync_WithValidEndpoint_ShouldRegisterSuccessfully()
    {
        // Arrange
        var endpoint = new WebhookEndpoint
        {
            Url = "https://example.com/webhook",
            EventTypes = new List<string> { "OperationCompletedEvent" }
        };

        // Act
        await _webhookService.RegisterWebhookAsync(endpoint);

        // Assert
        var registered = await _webhookService.GetWebhookAsync(endpoint.WebhookId);
        registered.Should().NotBeNull();
        registered.Should().BeEquivalentTo(endpoint);
    }

    [Fact]
    public async Task RegisterWebhookAsync_WithNullEndpoint_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _webhookService.RegisterWebhookAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RegisterWebhookAsync_WithEmptyUrl_ShouldThrowArgumentException()
    {
        // Arrange
        var endpoint = new WebhookEndpoint { Url = string.Empty };

        // Act
        Func<Task> act = async () => await _webhookService.RegisterWebhookAsync(endpoint);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UnregisterWebhookAsync_WithValidId_ShouldRemoveWebhook()
    {
        // Arrange
        var endpoint = new WebhookEndpoint { Url = "https://example.com/webhook" };
        await _webhookService.RegisterWebhookAsync(endpoint);

        // Act
        await _webhookService.UnregisterWebhookAsync(endpoint.WebhookId);

        // Assert
        var unregistered = await _webhookService.GetWebhookAsync(endpoint.WebhookId);
        unregistered.Should().BeNull();
    }

    [Fact]
    public async Task UnregisterWebhookAsync_WithEmptyId_ShouldNotThrow()
    {
        // Act
        Func<Task> act = async () => await _webhookService.UnregisterWebhookAsync(string.Empty);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetWebhookAsync_WithExistingId_ShouldReturnWebhook()
    {
        // Arrange
        var endpoint = new WebhookEndpoint { Url = "https://example.com/webhook" };
        await _webhookService.RegisterWebhookAsync(endpoint);

        // Act
        var result = await _webhookService.GetWebhookAsync(endpoint.WebhookId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(endpoint);
    }

    [Fact]
    public async Task GetWebhookAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _webhookService.GetWebhookAsync("non-existing-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveWebhooksAsync_WithActiveAndInactiveWebhooks_ShouldReturnOnlyActive()
    {
        // Arrange
        var activeEndpoint1 = new WebhookEndpoint { Url = "https://example.com/webhook1", IsActive = true };
        var activeEndpoint2 = new WebhookEndpoint { Url = "https://example.com/webhook2", IsActive = true };
        var inactiveEndpoint = new WebhookEndpoint { Url = "https://example.com/webhook3", IsActive = false };

        await _webhookService.RegisterWebhookAsync(activeEndpoint1);
        await _webhookService.RegisterWebhookAsync(activeEndpoint2);
        await _webhookService.RegisterWebhookAsync(inactiveEndpoint);

        // Act
        var activeWebhooks = await _webhookService.GetActiveWebhooksAsync();

        // Assert
        activeWebhooks.Should().HaveCount(2);
        activeWebhooks.Should().ContainSingle(w => w.WebhookId == activeEndpoint1.WebhookId);
        activeWebhooks.Should().ContainSingle(w => w.WebhookId == activeEndpoint2.WebhookId);
        activeWebhooks.Should().NotContain(w => w.WebhookId == inactiveEndpoint.WebhookId);
    }

    [Fact]
    public async Task GetActiveWebhooksAsync_WithNoWebhooks_ShouldReturnEmptyCollection()
    {
        // Act
        var activeWebhooks = await _webhookService.GetActiveWebhooksAsync();

        // Assert
        activeWebhooks.Should().BeEmpty();
    }

    [Fact]
    public async Task WebhookEndpoint_EventTypesFiltering_ShouldWorkCorrectly()
    {
        // Arrange
        var allEventsEndpoint = new WebhookEndpoint
        {
            Url = "https://example.com/all",
            EventTypes = new List<string>()
        };

        var specificEventsEndpoint = new WebhookEndpoint
        {
            Url = "https://example.com/specific",
            EventTypes = new List<string> { nameof(OperationCompletedEvent) }
        };

        await _webhookService.RegisterWebhookAsync(allEventsEndpoint);
        await _webhookService.RegisterWebhookAsync(specificEventsEndpoint);

        // Act
        var allEventsWebhook = await _webhookService.GetWebhookAsync(allEventsEndpoint.WebhookId);
        var specificEventsWebhook = await _webhookService.GetWebhookAsync(specificEventsEndpoint.WebhookId);

        // Assert
        allEventsWebhook.Should().NotBeNull();
        specificEventsWebhook.Should().NotBeNull();
        allEventsWebhook!.EventTypes.Should().BeEmpty();
        specificEventsWebhook!.EventTypes.Should().ContainSingle(nameof(OperationCompletedEvent));
    }

    [Fact]
    public async Task WebhookService_ConcurrentOperations_ShouldNotThrow()
    {
        // Arrange
        var tasks = new List<Task>();
        var endpoint = new WebhookEndpoint { Url = "https://example.com/concurrent" };

        // Act - Perform concurrent registrations and retrievals
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await _webhookService.RegisterWebhookAsync(endpoint);
                var _ = await _webhookService.GetWebhookAsync(endpoint.WebhookId);
            }));
        }

        // Assert
        await Task.WhenAll(tasks);
        var result = await _webhookService.GetWebhookAsync(endpoint.WebhookId);
        result.Should().NotBeNull();
    }
}