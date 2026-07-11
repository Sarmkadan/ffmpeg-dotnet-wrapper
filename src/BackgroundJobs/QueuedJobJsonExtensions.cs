using System.Text.Json;

namespace FFmpegDotnetWrapper.BackgroundJobs
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for <see cref="QueuedJob"/> objects.
    /// </summary>
    public static class QueuedJobJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes a <see cref="QueuedJob"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The job to serialize. Cannot be null.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
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
        /// Deserializes a JSON string to a <see cref="QueuedJob"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
        /// <returns>The deserialized <see cref="QueuedJob"/> instance, or null if deserialization fails.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
        public static QueuedJob? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<QueuedJob>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="QueuedJob"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
        /// <param name="value">Receives the deserialized job if successful; otherwise, null.</param>
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