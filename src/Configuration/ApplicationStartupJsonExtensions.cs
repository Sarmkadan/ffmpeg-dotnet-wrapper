// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegDotnetWrapper.Configuration
{
    /// <summary>
    /// Provides System.Text.Json serialization and deserialization extensions for <see cref="ApplicationStartup"/>
    /// configuration records.
    /// </summary>
    public static class ApplicationStartupJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Serializes the <see cref="ApplicationStartup"/> configuration to a JSON string.
        /// </summary>
        /// <param name="value">The application startup configuration to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this ApplicationStartup value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an <see cref="ApplicationStartup"/> configuration instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Must not be <see langword="null"/>, empty, or whitespace-only.</param>
        /// <returns>The deserialized <see cref="ApplicationStartup"/> instance, or <see langword="null"/> if the JSON represents a null value.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
        /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized into an <see cref="ApplicationStartup"/> instance.</exception>
        public static ApplicationStartup? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            return JsonSerializer.Deserialize<ApplicationStartup>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an <see cref="ApplicationStartup"/> configuration instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Must not be <see langword="null"/>, empty, or whitespace-only.</param>
        /// <param name="value">Receives the deserialized instance if successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
        public static bool TryFromJson(string json, out ApplicationStartup? value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            try
            {
                value = JsonSerializer.Deserialize<ApplicationStartup>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }

    /// <summary>
    /// Configuration record for application startup settings.
    /// Contains all necessary parameters to initialize the FFmpeg wrapper application.
    /// </summary>
    public record ApplicationStartup
    {
        /// <summary>Path to FFmpeg executable.</summary>
        public string? FFmpegPath { get; init; }

        /// <summary>Path to FFprobe executable.</summary>
        public string? FFprobePath { get; init; }

        /// <summary>Enable hardware acceleration.</summary>
        public bool EnableHardwareAcceleration { get; init; } = false;

        /// <summary>CPU preset for encoding.</summary>
        public string? EncodingPreset { get; init; } = "medium";

        /// <summary>Enable verbose logging.</summary>
        public bool VerboseLogging { get; init; } = false;

        /// <summary>Enable concurrent operations.</summary>
        public bool AllowConcurrentOperations { get; init; } = true;

        /// <summary>Maximum concurrent operations (0 = unlimited).</summary>
        public int MaxConcurrentOperations { get; init; } = 0;

        /// <summary>Default timeout for operations in seconds.</summary>
        public int OperationTimeoutSeconds { get; init; } = 600;

        /// <summary>Maximum file size to process in bytes (0 = unlimited).</summary>
        public long MaxFileSizeBytes { get; init; } = 50L * 1024 * 1024 * 1024; // 50GB

        /// <summary>Keep temporary files for debugging.</summary>
        public bool KeepTemporaryFiles { get; init; } = false;

        /// <summary>Directory for temporary files.</summary>
        public string? TemporaryDirectory { get; init; }

        /// <summary>Supported output formats.</summary>
        public string[] SupportedFormats { get; init; } =
        ["mp4", "mkv", "webm", "avi", "mov", "flv", "3gp", "ts"];

        /// <summary>Number of retry attempts for failed operations.</summary>
        public int RetryAttempts { get; init; } = 1;

        /// <summary>Delay between retry attempts in milliseconds.</summary>
        public int RetryDelayMs { get; init; } = 1000;
    }
}
