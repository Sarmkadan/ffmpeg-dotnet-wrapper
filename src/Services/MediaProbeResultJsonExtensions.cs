using System;
using System.Text.Json;

namespace FFmpegDotnetWrapper.Services
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="MediaProbeResult"/>.
    /// </summary>
    public static class MediaProbeResultJsonExtensions
    {
        /// <summary>
        /// Converts the <see cref="MediaProbeResult"/> to a JSON string.
        /// </summary>
        /// <param name="result">The media probe result to convert.</param>
        /// <param name="indented">If set to <c>true</c> the JSON is indented; otherwise, it's compact.</param>
        /// <returns>A JSON string representing the media probe result.</returns>
        public static string ToJson(this MediaProbeResult result, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(result);
            var options = new JsonSerializerOptions { WriteIndented = indented };
            return JsonSerializer.Serialize(result, options);
        }
    }
}