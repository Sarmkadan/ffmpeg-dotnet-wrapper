using System;
using System.Text.Json;

namespace FFmpegDotnetWrapper.Integration
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="WebhookEndpoint"/> instances.
    /// </summary>
    public static class WebhookEndpointJsonExtensions
    {
        /// <summary>
        /// Serializes the specified <see cref="WebhookEndpoint"/> to a JSON string.
        /// </summary>
        /// <param name="endpoint">The webhook endpoint to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representation of <paramref name="endpoint"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="endpoint"/> is <see langword="null"/>.
        /// </exception>
        public static string ToJson(this WebhookEndpoint endpoint, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(endpoint);

            var options = new JsonSerializerOptions { WriteIndented = indented };
            return JsonSerializer.Serialize(endpoint, options);
        }
    }
}
