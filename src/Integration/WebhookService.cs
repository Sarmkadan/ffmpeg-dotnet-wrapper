// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Events;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Integration
{
    /// <summary>
    /// Webhook endpoint configuration for delivering events to external systems.
    /// Supports retries, filtering by event type, and authentication.
    /// </summary>
    public class WebhookEndpoint
    {
        /// <summary>Unique identifier for this webhook.</summary>
        public string WebhookId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>URL where events should be delivered.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>Types of events to deliver (empty = all events).</summary>
        public List<string> EventTypes { get; set; } = new();

        /// <summary>Authentication token for webhook requests.</summary>
        public string? AuthToken { get; set; }

        /// <summary>Whether this webhook is active.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Number of delivery retry attempts on failure.</summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>When the webhook was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Custom headers to include in webhook requests.</summary>
        public Dictionary<string, string> Headers { get; set; } = new();
    }

    /// <summary>
    /// Service for delivering events to external systems via HTTP webhooks.
    /// Handles retries, backoff, and failure tracking.
    /// Integrates with event system to automatically deliver events.
    /// </summary>
    public interface IWebhookService
    {
        Task RegisterWebhookAsync(WebhookEndpoint endpoint);
        Task UnregisterWebhookAsync(string webhookId);
        Task<WebhookEndpoint?> GetWebhookAsync(string webhookId);
        Task<IEnumerable<WebhookEndpoint>> GetActiveWebhooksAsync();
    }

    public class WebhookService : IWebhookService, IEventHandler<OperationCompletedEvent>, IEventHandler<OperationFailedEvent>, IEventHandler<OperationStartedEvent>
    {
        private readonly ILogger<WebhookService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, WebhookEndpoint> _webhooks = new();
        private readonly object _lockObject = new();

        public WebhookService(ILogger<WebhookService> logger, HttpClient? httpClient = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// Registers a new webhook endpoint for event delivery.
        /// The webhook will receive events matching its configured event types.
        /// </summary>
        public Task RegisterWebhookAsync(WebhookEndpoint endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            if (string.IsNullOrEmpty(endpoint.Url))
                throw new ArgumentException("Webhook URL cannot be empty", nameof(endpoint));

            lock (_lockObject)
            {
                _webhooks[endpoint.WebhookId] = endpoint;
            }

            _logger.LogInformation(
                "Webhook registered: {WebhookId} -> {Url}",
                endpoint.WebhookId,
                endpoint.Url);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Unregisters and removes a webhook endpoint.
        /// The webhook will no longer receive event notifications.
        /// </summary>
        public Task UnregisterWebhookAsync(string webhookId)
        {
            if (string.IsNullOrEmpty(webhookId))
                return Task.CompletedTask;

            lock (_lockObject)
            {
                if (_webhooks.Remove(webhookId))
                {
                    _logger.LogInformation("Webhook unregistered: {WebhookId}", webhookId);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves webhook configuration by ID.
        /// Returns null if webhook doesn't exist.
        /// </summary>
        public Task<WebhookEndpoint?> GetWebhookAsync(string webhookId)
        {
            lock (_lockObject)
            {
                _webhooks.TryGetValue(webhookId, out var endpoint);
                return Task.FromResult(endpoint);
            }
        }

        /// <summary>
        /// Gets all active webhooks that are registered.
        /// </summary>
        public Task<IEnumerable<WebhookEndpoint>> GetActiveWebhooksAsync()
        {
            lock (_lockObject)
            {
                var active = new List<WebhookEndpoint>();
                foreach (var endpoint in _webhooks.Values)
                {
                    if (endpoint.IsActive)
                        active.Add(endpoint);
                }
                return Task.FromResult<IEnumerable<WebhookEndpoint>>(active);
            }
        }

        /// <summary>
        /// Event handler for operation completion events.
        /// Delivers the event to all registered webhooks.
        /// </summary>
        async Task IEventHandler<OperationCompletedEvent>.HandleAsync(OperationCompletedEvent @event)
        {
            await DeliverEventToWebhooksAsync(@event, nameof(OperationCompletedEvent)).ConfigureAwait(false);
        }

        /// <summary>
        /// Event handler for operation failure events.
        /// Ensures all webhooks are notified of operation failures.
        /// </summary>
        async Task IEventHandler<OperationFailedEvent>.HandleAsync(OperationFailedEvent @event)
        {
            await DeliverEventToWebhooksAsync(@event, nameof(OperationFailedEvent)).ConfigureAwait(false);
        }

        /// <summary>
        /// Event handler for operation started events.
        /// Notifies webhooks of new operations beginning.
        /// </summary>
        async Task IEventHandler<OperationStartedEvent>.HandleAsync(OperationStartedEvent @event)
        {
            await DeliverEventToWebhooksAsync(@event, nameof(OperationStartedEvent)).ConfigureAwait(false);
        }

        /// <summary>
        /// Delivers an event to all registered webhooks that are interested in it.
        /// Implements exponential backoff retry logic on failure.
        /// </summary>
        private async Task DeliverEventToWebhooksAsync<T>(T @event, string eventTypeName) where T : FFmpegEvent
        {
            List<WebhookEndpoint> webhooksToNotify;

            lock (_lockObject)
            {
                webhooksToNotify = _webhooks.Values
                    .Where(w => w.IsActive && (w.EventTypes.Count == 0 || w.EventTypes.Contains(eventTypeName)))
                    .ToList();
            }

            if (webhooksToNotify.Count == 0)
                return;

            var payload = JsonSerializer.Serialize(@event);
            var tasks = webhooksToNotify.Select(wh => DeliverEventWithRetryAsync(wh, payload, eventTypeName));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Delivers event payload to a webhook with retry logic.
        /// Implements exponential backoff: 1s, 2s, 4s, etc.
        /// </summary>
        private async Task DeliverEventWithRetryAsync(WebhookEndpoint webhook, string payload, string eventType)
        {
            for (int attempt = 0; attempt <= webhook.MaxRetries; attempt++)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url);
                    request.Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

                    // Add authentication if configured
                    if (!string.IsNullOrEmpty(webhook.AuthToken))
                    {
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", webhook.AuthToken);
                    }

                    // Add custom headers
                    foreach (var header in webhook.Headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    // Add event type header
                    request.Headers.Add("X-Event-Type", eventType);
                    request.Headers.Add("X-Event-Id", Guid.NewGuid().ToString());

                    using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogDebug(
                            "Webhook delivered successfully: {WebhookId} ({StatusCode})",
                            webhook.WebhookId,
                            response.StatusCode);
                        return;
                    }

                    _logger.LogWarning(
                        "Webhook delivery failed: {WebhookId} ({StatusCode})",
                        webhook.WebhookId,
                        response.StatusCode);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Webhook delivery exception (Attempt {Attempt}): {WebhookId}",
                        attempt + 1,
                        webhook.WebhookId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error delivering webhook: {WebhookId}", webhook.WebhookId);
                    return; // Don't retry on unexpected errors
                }

                // Wait before retrying (exponential backoff)
                if (attempt < webhook.MaxRetries)
                {
                    var delayMs = (int)Math.Pow(2, attempt) * 1000; // 1s, 2s, 4s, 8s
                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
            }

            _logger.LogError(
                "Webhook delivery failed after {MaxRetries} retries: {WebhookId}",
                webhook.MaxRetries,
                webhook.WebhookId);
        }
    }
}
