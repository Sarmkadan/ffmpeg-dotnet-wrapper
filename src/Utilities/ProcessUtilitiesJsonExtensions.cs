// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegDotnetWrapper.Utilities
{
    /// <summary>
    /// Provides System.Text.Json serialization and deserialization extensions for ProcessUtilities types.
    /// Enables serialization of <see cref="ProcessUtilities.ProcessResult"/> to JSON,
    /// and deserialization back to strongly-typed objects.
    /// </summary>
    public static class ProcessUtilitiesJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes a <see cref="ProcessUtilities.ProcessResult"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The ProcessResult instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the ProcessResult instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this ProcessUtilities.ProcessResult value, bool indented = false) =>
            JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            } : _jsonSerializerOptions);

        /// <summary>
        /// Deserializes a JSON string to a <see cref="ProcessUtilities.ProcessResult"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A ProcessResult instance, or <see langword="null"/> if deserialization fails.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
        public static ProcessUtilities.ProcessResult? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                return JsonSerializer.Deserialize<ProcessUtilities.ProcessResult>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="ProcessUtilities.ProcessResult"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized ProcessResult instance if successful.</param>
        /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
        public static bool TryFromJson(string json, out ProcessUtilities.ProcessResult? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<ProcessUtilities.ProcessResult>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}