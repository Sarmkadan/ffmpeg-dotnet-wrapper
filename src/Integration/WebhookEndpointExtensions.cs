using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegDotnetWrapper.Integration
{
    /// <summary>
    /// Provides extension methods for <see cref="WebhookEndpoint"/> to enhance configuration validation, header management, and event handling.
    /// </summary>
    public static class WebhookEndpointExtensions
    {
        /// <summary>
        /// Validates the configuration of the webhook endpoint to ensure required properties are set.
        /// </summary>
        /// <param name="endpoint">The webhook endpoint to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endpoint"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if required properties are invalid.</exception>
        public static void ValidateConfiguration(this WebhookEndpoint endpoint)
        {
            ArgumentNullException.ThrowIfNull(endpoint);

            if (string.IsNullOrEmpty(endpoint.Url))
                throw new ArgumentException("Url must not be null or empty.", nameof(endpoint.Url));

            if (endpoint.EventTypes is null || endpoint.EventTypes.Count == 0)
                throw new ArgumentException("EventTypes must not be null or empty.", nameof(endpoint.EventTypes));

            if (string.IsNullOrEmpty(endpoint.WebhookId))
                throw new ArgumentException("WebhookId must not be null or empty.", nameof(endpoint.WebhookId));
        }

        /// <summary>
        /// Merges new headers into the existing headers of the webhook endpoint.
        /// </summary>
        /// <param name="endpoint">The webhook endpoint to update.</param>
        /// <param name="newHeaders">The headers to merge into the endpoint.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endpoint"/> or <paramref name="newHeaders"/> is null.</exception>
        public static void MergeHeaders(this WebhookEndpoint endpoint, IDictionary<string, string?> newHeaders)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            ArgumentNullException.ThrowIfNull(newHeaders);

            if (newHeaders.Any(kvp => string.IsNullOrEmpty(kvp.Key)))
                throw new ArgumentException("Header keys must not be null or empty.", nameof(newHeaders));

            endpoint.Headers ??= new Dictionary<string, string>();
            foreach (var (key, value) in newHeaders)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    endpoint.Headers[key] = value;
                }
            }
        }

        /// <summary>
        /// Determines whether the webhook endpoint is expired based on its creation time and a specified expiration period.
        /// </summary>
        /// <param name="endpoint">The webhook endpoint to check.</param>
        /// <param name="expirationPeriod">The time span after which the webhook is considered expired.</param>
        /// <returns><c>true</c> if the webhook is expired; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endpoint"/> is null.</exception>
        public static bool IsExpired(this WebhookEndpoint endpoint, TimeSpan expirationPeriod)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            return endpoint.CreatedAt < DateTime.UtcNow.Subtract(expirationPeriod);
        }

        /// <summary>
        /// Determines whether the webhook endpoint can handle the specified event type.
        /// </summary>
        /// <param name="endpoint">The webhook endpoint to check.</param>
        /// <param name="eventType">The event type to evaluate.</param>
        /// <returns><c>true</c> if the event type is supported; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endpoint"/> or <paramref name="eventType"/> is null.</exception>
        public static bool CanHandleEvent(this WebhookEndpoint endpoint, string eventType)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            return endpoint.EventTypes.Contains(eventType);
        }
    }
}
