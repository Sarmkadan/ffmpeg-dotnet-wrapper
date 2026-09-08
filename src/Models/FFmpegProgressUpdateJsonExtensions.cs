namespace FFmpegDotnetWrapper.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides JSON serialization extension methods for <see cref="FFmpegProgressUpdate"/>.
/// </summary>
public static class FFmpegProgressUpdateJsonExtensions
{
    /// <summary>
    /// Serializes the <see cref="FFmpegProgressUpdate"/> instance to a JSON string.
    /// </summary>
    /// <param name="update">The progress update to serialize.</param>
    /// <param name="indented">If set to <c>true</c>, the JSON is pretty-printed with indentation. Default is <c>false</c>.</param>
    /// <returns>A JSON string representing the <see cref="FFmpegProgressUpdate"/>.</returns>
    public static string ToJson(this FFmpegProgressUpdate update, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(update);

        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(update, options);
    }
}