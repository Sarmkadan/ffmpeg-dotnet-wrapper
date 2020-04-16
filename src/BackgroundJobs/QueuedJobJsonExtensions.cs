using System.Text.Json;

namespace FFmpegDotnetWrapper.BackgroundJobs
{
    /// <summary>
    /// Provides JSON serialization and deserialization extension methods for <see cref="QueuedJob"/> instances.
    /// </summary>
    public static class QueuedJobJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes a <see cref="QueuedJob"/> instance into a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="QueuedJob"/> to serialize.</param>
        /// <param name="indented">Whether the resulting JSON string should be formatted with indentation.</param>
        /// <returns>A JSON string representation of the job.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this QueuedJob value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_options) { WriteIndented = true }
                : _options;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="QueuedJob"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="QueuedJob"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown when deserialization fails.</exception>
        public static QueuedJob? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<QueuedJob>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="QueuedJob"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">When this method returns, contains the deserialized <see cref="QueuedJob"/> if successful, or null if it fails.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
        public static bool TryFromJson(string json, out QueuedJob? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<QueuedJob>(json, _options);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}