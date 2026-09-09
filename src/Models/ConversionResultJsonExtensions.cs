using System.Text.Json;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides JSON serialization extension methods for <see cref="ConversionResult"/>.
/// </summary>
public static class ConversionResultJsonExtensions
{
    /// <summary>
    /// Serializes the specified <see cref="ConversionResult"/> to a JSON string.
    /// </summary>
    /// <param name="result">The conversion result to serialize.</param>
    /// <param name="indented">
    /// <see langword="true"/> to format the JSON with indentation; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>A JSON string representation of <paramref name="result"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is <see langword="null"/>.
    /// </exception>
    public static string ToJson(this ConversionResult result, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(result);

        var options = new JsonSerializerOptions
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(result, options);
    }
}
