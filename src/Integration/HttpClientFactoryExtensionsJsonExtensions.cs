// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegDotnetWrapper.Integration
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="HttpClientConfig"/>.
    /// Enables serialization and deserialization of HTTP client configuration.
    /// </summary>
    public static class HttpClientFactoryExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Serializes an <see cref="HttpClientConfig"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The HttpClientConfig instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the HttpClientConfig instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this HttpClientConfig value, bool indented = false) =>
            ToJson(value, indented, _jsonSerializerOptions);

        /// <summary>
        /// Deserializes a JSON string to an <see cref="HttpClientConfig"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized HttpClientConfig instance, or null if deserialization fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static HttpClientConfig? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            return JsonSerializer.Deserialize<HttpClientConfig>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an <see cref="HttpClientConfig"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized HttpClientConfig instance if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
        public static bool TryFromJson(string json, out HttpClientConfig? value)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            try
            {
                value = JsonSerializer.Deserialize<HttpClientConfig>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Serializes an <see cref="HttpClientConfig"/> instance to a JSON string with custom options.
        /// </summary>
        /// <param name="value">The HttpClientConfig instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <param name="options">Custom JSON serializer options to use.</param>
        /// <returns>A JSON string representation of the HttpClientConfig instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null or when <paramref name="options"/> is null.</exception>
        private static string ToJson(HttpClientConfig value, bool indented, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(options);

            var localOptions = indented
                ? new JsonSerializerOptions(options) { WriteIndented = true }
                : options;

            return JsonSerializer.Serialize(value, localOptions);
        }
    }
}
