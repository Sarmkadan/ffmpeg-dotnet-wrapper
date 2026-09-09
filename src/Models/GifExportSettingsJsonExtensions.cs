using System;
using System.Text.Json;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides JSON serialization extensions for <see cref="GifExportSettings"/>.
/// </summary>
public static class GifExportSettingsJsonExtensions
{
    /// <summary>
    /// Serializes the specified GIF export settings to a JSON string.
    /// </summary>
    /// <param name="settings">The GIF export settings to serialize.</param>
    /// <param name="indented">
    /// <see langword="true"/> to format the JSON with indentation; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>A JSON string representation of <paramref name="settings"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="settings"/> is <see langword="null"/>.
    /// </exception>
    public static string ToJson(this GifExportSettings settings, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = indented
        });
    }
}
