using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegDotnetWrapper.Models
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="StreamingProfile"/>.
    /// </summary>
    public static class StreamingProfileJsonExtensions
    {
        /// <summary>
        /// Converts a <see cref="StreamingProfile"/> to its JSON representation.
        /// </summary>
        /// <param name="profile">The profile to convert.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representing the profile.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="profile"/> is null.</exception>
        public static string ToJson(this StreamingProfile profile, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(profile);

            var options = new JsonSerializerOptions
            {
                WriteIndented = indented
            };

            return JsonSerializer.Serialize(profile, options);
        }
    }
}