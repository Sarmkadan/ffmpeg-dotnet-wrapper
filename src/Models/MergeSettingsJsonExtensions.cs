using System;
using System.Text.Json;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Models
{
    /// <summary>
    /// Provides extension methods for <see cref="MergeSettings"/>.
    /// </summary>
    public static class MergeSettingsJsonExtensions
    {
        /// <summary>
        /// Converts the <see cref="MergeSettings"/> instance to a JSON string.
        /// </summary>
        /// <param name="settings">The settings to convert.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representation of the settings.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
        public static string ToJson(this MergeSettings settings, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(settings);
            var options = new JsonSerializerOptions { WriteIndented = indented };
            return JsonSerializer.Serialize(settings, options);
        }
    }
}