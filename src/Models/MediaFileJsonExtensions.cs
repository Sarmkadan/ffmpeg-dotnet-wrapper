using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegDotnetWrapper.Models
{
    /// <summary>
    /// Provides JSON serialization extension methods for <see cref="MediaFile"/>.
    /// </summary>
    public static class MediaFileJsonExtensions
    {
        /// <summary>
        /// Serializes the <see cref="MediaFile"/> instance to a JSON string.
        /// </summary>
        /// <param name="mediaFile">The <see cref="MediaFile"/> to serialize.</param>
        /// <param name="indented">Whether to write indented JSON.</param>
        /// <returns>A JSON string representing the <see cref="MediaFile"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaFile"/> is <see langword="null"/>.</exception>
        public static string ToJson(this MediaFile mediaFile, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(mediaFile);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = indented
            };

            return JsonSerializer.Serialize(mediaFile, options);
        }
    }
}