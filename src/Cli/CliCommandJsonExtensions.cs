using System;
using System.Text.Json;

namespace FFmpegDotnetWrapper.Cli
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="CliCommand"/> instances.
    /// </summary>
    public static class CliCommandJsonExtensions
    {
        /// <summary>
        /// Serializes the specified <see cref="CliCommand"/> to a JSON string.
        /// </summary>
        /// <param name="command">The command to serialize.</param>
        /// <param name="indented">
        /// <see langword="true"/> to format the JSON with indentation; otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>A JSON string representation of <paramref name="command"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="command"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when a property type is not supported by the serializer.
        /// </exception>
        public static string ToJson(this CliCommand command, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(command);

            return JsonSerializer.Serialize(command, new JsonSerializerOptions
            {
                WriteIndented = indented
            });
        }
    }
}
