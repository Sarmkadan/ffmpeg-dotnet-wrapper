namespace FFmpegDotnetWrapper.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides JSON serialization extension methods for <see cref="TrimSettings"/>.
/// </summary>
public static class TrimSettingsJsonExtensions
{
    /// <summary>
    /// Serializes the <see cref="TrimSettings"/> instance to a JSON string.
    /// </summary>
    /// <param name="settings">The trim settings to serialize.</param>
    /// <param name="indented">If set to <c>true</c>, the JSON is pretty-printed with indentation. Default is <c>false</c>.</param>
    /// <returns>A JSON string representing the <see cref="TrimSettings"/>.</returns>
    public static string ToJson(this TrimSettings settings, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(settings, options);
    }
}