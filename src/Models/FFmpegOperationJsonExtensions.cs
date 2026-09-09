using System;
using System.Text.Json;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides JSON serialization extension methods for <see cref="FFmpegOperation"/>.
/// </summary>
public static class FFmpegOperationJsonExtensions
{
    /// <summary>
    /// Serializes the specified <see cref="FFmpegOperation"/> to a JSON string.
    /// </summary>
    /// <param name="operation">The operation to serialize.</param>
    /// <param name="indented">
    /// <see langword="true"/> to format the JSON with indentation; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>A JSON string representation of <paramref name="operation"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="operation"/> is <see langword="null"/>.
    /// </exception>
    public static string ToJson(this FFmpegOperation operation, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var options = new JsonSerializerOptions { WriteIndented = indented };
        return JsonSerializer.Serialize(operation, options);
    }
}
