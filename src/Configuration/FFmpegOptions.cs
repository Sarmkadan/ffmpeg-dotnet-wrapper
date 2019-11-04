// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace FFmpegDotnetWrapper.Configuration
{
    /// <summary>
    /// Configuration options for FFmpeg execution and behavior.
    /// Bindable from configuration files (appsettings.json) using options pattern.
    /// </summary>
    public class FFmpegOptions
    {
        public const string Section = "FFmpeg";

        /// <summary>Path to FFmpeg executable (auto-detected if empty).</summary>
        public string? FFmpegPath { get; set; }

        /// <summary>Path to FFprobe executable for media analysis (auto-detected if empty).</summary>
        public string? FFprobePath { get; set; }

        /// <summary>Default timeout for FFmpeg operations in seconds.</summary>
        public int OperationTimeoutSeconds { get; set; } = 600; // 10 minutes

        /// <summary>Maximum file size to process in bytes (0 = unlimited).</summary>
        public long MaxFileSizeBytes { get; set; } = 50L * 1024 * 1024 * 1024; // 50GB

        /// <summary>Enable hardware acceleration (NVIDIA CUDA, Intel QSV, etc).</summary>
        public bool EnableHardwareAcceleration { get; set; } = false;

        /// <summary>CPU preset for encoding (ultrafast, superfast, veryfast, faster, fast, medium, slow, slower, veryslow).</summary>
        public string? EncodingPreset { get; set; } = "medium";

        /// <summary>Keep temporary files for debugging (normally deleted).</summary>
        public bool KeepTemporaryFiles { get; set; } = false;

        /// <summary>Directory for temporary files (uses system temp if empty).</summary>
        public string? TemporaryDirectory { get; set; }

        /// <summary>Enable verbose logging of FFmpeg output.</summary>
        public bool VerboseLogging { get; set; } = false;

        /// <summary>Default quality level for encoding (0-51 for H.264).</summary>
        public int? DefaultQuality { get; set; } = 23;

        /// <summary>Default audio bitrate in kbps.</summary>
        public int DefaultAudioBitrate { get; set; } = 128;

        /// <summary>Default video bitrate in kbps (0 = auto).</summary>
        public int DefaultVideoBitrate { get; set; } = 0;

        /// <summary>Enable concurrent processing of multiple files.</summary>
        public bool AllowConcurrentOperations { get; set; } = true;

        /// <summary>Maximum concurrent operations (0 = unlimited).</summary>
        public int MaxConcurrentOperations { get; set; } = 0;

        /// <summary>Supported output formats.</summary>
        public List<string> SupportedFormats { get; set; } = new()
        {
            "mp4", "mkv", "webm", "avi", "mov", "flv", "3gp", "ts"
        };

        /// <summary>Validation: ensure path exists.</summary>
        public bool ValidatePaths { get; set; } = true;

        /// <summary>Validation: check output directory is writable.</summary>
        public bool ValidateOutputPath { get; set; } = true;

        /// <summary>Number of retry attempts for failed operations.</summary>
        public int RetryAttempts { get; set; } = 1;

        /// <summary>Delay between retry attempts in milliseconds.</summary>
        public int RetryDelayMs { get; set; } = 1000;
    }

    /// <summary>
    /// Configuration options for caching behavior.
    /// </summary>
    public class CachingOptions
    {
        public const string Section = "Caching";

        /// <summary>Enable in-memory caching of media metadata.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Maximum number of entries in the cache.</summary>
        public int MaxCacheSize { get; set; } = 1000;

        /// <summary>Default expiration time for cache entries in minutes.</summary>
        public int DefaultExpirationMinutes { get; set; } = 60;

        /// <summary>Cache data types to enable (Metadata, ProbeResults, ConversionResults).</summary>
        public List<string> EnabledCacheTypes { get; set; } = new()
        {
            "Metadata", "ProbeResults"
        };
    }

    /// <summary>
    /// Configuration options for rate limiting.
    /// </summary>
    public class RateLimitingOptions
    {
        public const string Section = "RateLimiting";

        /// <summary>Enable rate limiting.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Maximum transcode operations per hour per user.</summary>
        public int TranscodeOperationsPerHour { get; set; } = 5;

        /// <summary>Maximum watermark operations per hour per user.</summary>
        public int WatermarkOperationsPerHour { get; set; } = 20;

        /// <summary>Maximum merge operations per hour per user.</summary>
        public int MergeOperationsPerHour { get; set; } = 10;

        /// <summary>Global rate limit window in seconds.</summary>
        public int WindowSeconds { get; set; } = 3600;

        /// <summary>Enable per-user rate limiting in addition to global limits.</summary>
        public bool PerUserLimiting { get; set; } = true;
    }

    /// <summary>
    /// Configuration options for webhook integration.
    /// </summary>
    public class WebhookOptions
    {
        public const string Section = "Webhooks";

        /// <summary>Enable webhook delivery of events.</summary>
        public bool Enabled { get; set; } = false;

        /// <summary>Maximum number of retries for webhook delivery.</summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>Request timeout for webhook calls in seconds.</summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>HTTP client name for webhook delivery (from HttpClientFactory).</summary>
        public string HttpClientName { get; set; } = "webhook";

        /// <summary>Pre-configured webhook endpoints.</summary>
        public List<PreConfiguredWebhook> PreConfiguredWebhooks { get; set; } = new();
    }

    /// <summary>
    /// Pre-configured webhook endpoint from configuration.
    /// </summary>
    public class PreConfiguredWebhook
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? AuthToken { get; set; }
        public List<string> EventTypes { get; set; } = new();
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// Configuration options for logging.
    /// </summary>
    public class LoggingOptions
    {
        public const string Section = "Logging";

        /// <summary>Log request bodies.</summary>
        public bool LogRequestBody { get; set; } = true;

        /// <summary>Log response bodies.</summary>
        public bool LogResponseBody { get; set; } = false;

        /// <summary>Log performance metrics.</summary>
        public bool LogPerformanceMetrics { get; set; } = true;

        /// <summary>Log FFmpeg command lines.</summary>
        public bool LogFFmpegCommands { get; set; } = false;

        /// <summary>Maximum length for values in logs (prevents logging huge blobs).</summary>
        public int MaxValueLength { get; set; } = 500;

        /// <summary>Include stack traces in error logs.</summary>
        public bool IncludeStackTraces { get; set; } = true;
    }

    /// <summary>
    /// Configuration options for CLI behavior.
    /// </summary>
    public class CliOptions
    {
        public const string Section = "Cli";

        /// <summary>Enable colored output in console.</summary>
        public bool EnableColoredOutput { get; set; } = true;

        /// <summary>Show progress bars for long operations.</summary>
        public bool ShowProgressBars { get; set; } = true;

        /// <summary>Verbosity level (0=Quiet, 1=Normal, 2=Verbose, 3=Debug).</summary>
        public int VerbosityLevel { get; set; } = 1;

        /// <summary>Default output format (JSON, CSV, Text).</summary>
        public string DefaultOutputFormat { get; set; } = "Text";
    }
}
