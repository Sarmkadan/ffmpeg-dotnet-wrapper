using System;
using System.Text.Json;

namespace FFmpegDotnetWrapper.Middleware
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="RateLimitStatus"/>.
    /// </summary>
    public static class RateLimitStatusJsonExtensions
    {
        /// <summary>
        /// Serializes the specified rate limit status to JSON.
        /// </summary>
        /// <param name="status">The rate limit status to serialize.</param>
        /// <param name="indented">
        /// <see langword="true"/> to format the JSON with indentation; otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>A JSON string representing the rate limit status.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="status"/> is <see langword="null"/>.
        /// </exception>
        public static string ToJson(this RateLimitStatus status, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(status);

            return JsonSerializer.Serialize(status, new JsonSerializerOptions
            {
                WriteIndented = indented
            });
        }
    }
}
