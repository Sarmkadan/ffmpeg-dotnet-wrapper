// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegDotnetWrapper.Middleware
{
    /// <summary>
    /// Provides System.Text.Json serialization and deserialization extensions for RequestLoggingOptions.
    /// Enables JSON configuration files and API communication for logging configuration.
    /// </summary>
    public static class RequestLoggingOptionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the RequestLoggingOptions instance to a JSON string.
        /// </summary>
        /// <param name="value">The RequestLoggingOptions instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the RequestLoggingOptions.</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        public static string ToJson(this RequestLoggingOptions value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions)
                {
                    WriteIndented = true
                }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a RequestLoggingOptions instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A RequestLoggingOptions instance populated from the JSON, or null if parsing fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when json is null.</exception>
        /// <exception cref="ArgumentException">Thrown when json is empty or whitespace.</exception>
        public static RequestLoggingOptions? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrEmpty(json.Trim());

            return JsonSerializer.Deserialize<RequestLoggingOptions>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a RequestLoggingOptions instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized RequestLoggingOptions if successful, otherwise null.</param>
        /// <returns>True if deserialization succeeded; false if an exception occurred.</returns>
        /// <exception cref="ArgumentNullException">Thrown when json is null.</exception>
        public static bool TryFromJson(string json, out RequestLoggingOptions? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<RequestLoggingOptions>(json, _jsonOptions);
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