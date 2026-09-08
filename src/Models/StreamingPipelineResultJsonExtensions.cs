// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides JSON serialization extensions for <see cref="StreamingPipelineResult"/>.
/// </summary>
public static class StreamingPipelineResultJsonExtensions
{
    /// <summary>
    /// Serializes the specified <see cref="StreamingPipelineResult"/> to a JSON string.
    /// </summary>
    /// <param name="result">The streaming pipeline result to serialize.</param>
    /// <param name="indented">
    /// <see langword="true"/> to format the JSON with indentation; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>A JSON string representation of <paramref name="result"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is <see langword="null"/>.
    /// </exception>
    public static string ToJson(this StreamingPipelineResult result, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(result);

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = indented
        });
    }
}
