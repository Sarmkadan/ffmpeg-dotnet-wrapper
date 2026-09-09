using System;
using System.Text.Json;

namespace FFmpegDotnetWrapper.Monitoring
{
    /// <summary>
    /// Extension methods for <see cref="OperationStats"/>.
    /// </summary>
    public static class OperationStatsJsonExtensions
    {
        /// <summary>
        /// Converts the <see cref="OperationStats"/> to a JSON string.
        /// </summary>
        /// <param name="stats">The operation statistics to convert.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representation of the statistics.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stats"/> is null.</exception>
        public static string ToJson(this OperationStats stats, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(stats);

            var options = new JsonSerializerOptions
            {
                WriteIndented = indented
            };

            return JsonSerializer.Serialize(stats, options);
        }
    }
}