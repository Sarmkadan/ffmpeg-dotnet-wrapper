// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace FFmpegDotnetWrapper.Api.DTOs
{
    /// <summary>
    /// Base class for all API request DTOs.
    /// Provides common validation attributes and standardized request structure.
    /// </summary>
    public abstract class ApiRequest
    {
        /// <summary>
        /// Unique request identifier for tracking and correlation across distributed systems.
        /// </summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Timestamp when the request was received by the API.
        /// Useful for audit trails and performance monitoring.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional correlation ID for linking related operations in a workflow.
        /// Enables tracking of composite operations across multiple API calls.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Optional user or tenant identifier for multi-tenant scenarios.
        /// Enables resource isolation and audit logging per tenant.
        /// </summary>
        public string? TenantId { get; set; }
    }

    /// <summary>
    /// Request DTO for transcoding operations.
    /// Includes source file, output format, and codec parameters.
    /// </summary>
    public class TranscodeRequest : ApiRequest
    {
        private const string DefaultOutputFormat = "mp4";

        [Required(ErrorMessage = "Input path is required")]
        [StringLength(500)]
        public string InputPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Output path is required")]
        [StringLength(500)]
        public string OutputPath { get; set; } = string.Empty;

        [StringLength(20)]
        public string OutputFormat { get; set; } = DefaultOutputFormat;

        [StringLength(50)]
        public string? Codec { get; set; }

        [Range(1, 8000)]
        public int? Bitrate { get; set; }

        [Range(0, 51)]
        public int? Quality { get; set; }

        /// <summary>
        /// Returns a string representation of the TranscodeRequest.
        /// </summary>
        /// <returns>A concise, single-line, culture-invariant summary including InputPath, OutputPath and codec/format properties.</returns>
        public override string ToString()
        {
            return $"InputPath: {InputPath}, OutputPath: {OutputPath}, Format: {OutputFormat}, Codec: {Codec ?? "none"}";
        }
    }

    /// <summary>
    /// Request DTO for trimming operations.
    /// Supports both duration-based and time-range-based trimming.
    /// </summary>
    public class TrimRequest : ApiRequest
    {
        [Required(ErrorMessage = "Input path is required")]
        public string InputPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Output path is required")]
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>
        /// Start time in format "00:00:00" or seconds.
        /// Example: "00:00:10" trims from 10 seconds in.
        /// </summary>
        [StringLength(20)]
        public string? StartTime { get; set; }

        /// <summary>
        /// End time in format "00:00:00".
        /// Either EndTime or Duration must be specified.
        /// </summary>
        [StringLength(20)]
        public string? EndTime { get; set; }

        /// <summary>
        /// Duration to keep, in format "00:00:00".
        /// Either EndTime or Duration must be specified.
        /// </summary>
        [StringLength(20)]
        public string? Duration { get; set; }

        /// <summary>
        /// Returns a string representation of the TrimRequest.
        /// </summary>
        /// <returns>A concise, single-line, culture-invariant summary including InputPath, OutputPath and trimming properties.</returns>
        public override string ToString()
        {
            return $"InputPath: {InputPath}, OutputPath: {OutputPath}, Start: {StartTime ?? "none"}, End: {EndTime ?? "none"}, Duration: {Duration ?? "none"}";
        }
    }

    /// <summary>
    /// Request DTO for merging multiple video files.
    /// Concatenates files in the order provided while maintaining codec compatibility.
    /// </summary>
    public class MergeRequest : ApiRequest
    {
        [Required(ErrorMessage = "At least 2 input files are required")]
        [MinLength(2, ErrorMessage = "At least 2 input files are required")]
        public List<string> InputPaths { get; set; } = [];

        [Required(ErrorMessage = "Output path is required")]
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>
        /// When true, resizes all videos to match the first video's aspect ratio.
        /// Prevents letter-boxing or distortion when merging videos of different resolutions.
        /// </summary>
        public bool MaintainAspectRatio { get; set; } = true;

        /// <summary>
        /// Returns a string representation of the MergeRequest.
        /// </summary>
        /// <returns>A concise, single-line, culture-invariant summary including input count and output path.</returns>
        public override string ToString()
        {
            return $"InputCount: {InputPaths.Count}, OutputPath: {OutputPath}, MaintainAspectRatio: {MaintainAspectRatio}";
        }
    }

    /// <summary>
    /// Request DTO for watermarking operations.
    /// Overlays an image watermark on video with customizable positioning and transparency.
    /// </summary>
    public class WatermarkRequest : ApiRequest
    {
        private const int DefaultPositionX = 10;
        private const int DefaultPositionY = 10;
        private const double DefaultOpacity = 0.8;
        private const double DefaultScale = 0.15;

        [Required(ErrorMessage = "Input path is required")]
        public string InputPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Output path is required")]
        public string OutputPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Watermark path is required")]
        [StringLength(500)]
        public string WatermarkPath { get; set; } = string.Empty;

        /// <summary>
        /// Horizontal position in pixels from the left edge.
        /// Negative values position from the right edge.
        /// </summary>
        [Range(-4096, 4096)]
        public int PositionX { get; set; } = DefaultPositionX;

        /// <summary>
        /// Vertical position in pixels from the top edge.
        /// Negative values position from the bottom edge.
        /// </summary>
        [Range(-2160, 2160)]
        public int PositionY { get; set; } = DefaultPositionY;

        /// <summary>
        /// Opacity of the watermark from 0 (transparent) to 1 (opaque).
        /// Allows for subtle, semi-transparent watermarks.
        /// </summary>
        [Range(0.0, 1.0)]
        public double Opacity { get; set; } = DefaultOpacity;

        /// <summary>
        /// Scale factor for the watermark relative to video width.
        /// Example: 0.2 makes watermark 20% of video width.
        /// </summary>
        [Range(0.01, 1.0)]
        public double Scale { get; set; } = DefaultScale;

        /// <summary>
        /// Returns a string representation of the WatermarkRequest.
        /// </summary>
        /// <returns>A concise, single-line, culture-invariant summary including InputPath, OutputPath, WatermarkPath and watermark properties.</returns>
        public override string ToString()
        {
            return $"InputPath: {InputPath}, OutputPath: {OutputPath}, WatermarkPath: {WatermarkPath}, PositionX: {PositionX}, PositionY: {PositionY}, Opacity: {Opacity}, Scale: {Scale}";
        }
    }

    /// <summary>
    /// Request DTO for subtitle embedding operations.
    /// Supports both soft embedding (as a selectable stream) and hard embedding (burned into frames).
    /// </summary>
    public class SubtitleRequest : ApiRequest
    {
        private const string DefaultFontName = "Arial";
        private const int DefaultFontSize = 24;

        [Required(ErrorMessage = "Input path is required")]
        [StringLength(500)]
        public string InputPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Output path is required")]
        [StringLength(500)]
        public string OutputPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subtitle path is required")]
        [StringLength(500)]
        public string SubtitlePath { get; set; } = string.Empty;

        /// <summary>
        /// When <c>true</c>, subtitles are burned directly into the video frames.
        /// When <c>false</c> (default), subtitles are added as a selectable stream.
        /// </summary>
        public bool HardEmbed { get; set; } = false;

        /// <summary>
        /// ISO 639-1 language code stored in the subtitle stream metadata (e.g. <c>en</c>, <c>fr</c>).
        /// </summary>
        [StringLength(10)]
        public string? Language { get; set; }

        /// <summary>
        /// Font name used when hard-embedding subtitles. Defaults to <c>Arial</c>.
        /// </summary>
        [StringLength(100)]
        public string FontName { get; set; } = DefaultFontName;

        /// <summary>
        /// Font size in points when hard-embedding subtitles.
        /// </summary>
        [Range(6, 120)]
        public int FontSize { get; set; } = DefaultFontSize;
    }

    /// <summary>
    /// Request DTO for thumbnail extraction operations.
    /// Extracts one or more frames from a video as image files.
    /// </summary>
    public class ThumbnailRequest : ApiRequest
    {
        private const string DefaultFormat = "jpeg";
        private const int DefaultCount = 1;

        [Required(ErrorMessage = "Input path is required")]
        [StringLength(500)]
        public string InputPath { get; set; } = string.Empty;

        /// <summary>
        /// Output file path or pattern for the extracted thumbnail(s).
        /// Use <c>%03d</c> when extracting multiple thumbnails (e.g. <c>/out/thumb_%03d.jpg</c>).
        /// </summary>
        [Required(ErrorMessage = "Output pattern is required")]
        [StringLength(500)]
        public string OutputPattern { get; set; } = string.Empty;

        /// <summary>
        /// List of specific timestamps (in seconds) at which to capture thumbnails.
        /// When empty, thumbnails are evenly distributed across the video duration.
        /// </summary>
        public List<double> TimestampsSeconds { get; set; } = [];

        /// <summary>
        /// Number of evenly-spaced thumbnails to extract. Used when <see cref="TimestampsSeconds"/> is empty.
        /// </summary>
        [Range(1, 100)]
        public int Count { get; set; } = DefaultCount;

        /// <summary>
        /// Output width in pixels. Set to -1 to preserve aspect ratio relative to <see cref="Height"/>.
        /// </summary>
        [Range(-1, 7680)]
        public int? Width { get; set; }

        /// <summary>
        /// Output height in pixels. Set to -1 to preserve aspect ratio relative to <see cref="Width"/>.
        /// </summary>
        [Range(-1, 4320)]
        public int? Height { get; set; }

        /// <summary>
        /// Output image format. Defaults to <c>jpeg</c>.
        /// </summary>
        public string Format { get; set; } = DefaultFormat;
    }

    /// <summary>
    /// Request DTO for audio extraction operations.
    /// Extracts audio from video files and saves as standalone audio files.
    /// </summary>
    public class AudioExtractRequest : ApiRequest
    {
        private const string DefaultAudioCodec = "mp3";
        private const int DefaultAudioBitrate = 192;

        [Required(ErrorMessage = "Input path is required")]
        [StringLength(500)]
        public string InputPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Output path is required")]
        [StringLength(500)]
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>
        /// Target audio codec for extraction. Defaults to MP3.
        /// </summary>
        public string AudioCodec { get; set; } = DefaultAudioCodec;

        /// <summary>
        /// Target audio bitrate in kbps. Defaults to 192 kbps.
        /// </summary>
        [Range(32, 320)]
        public int AudioBitrate { get; set; } = DefaultAudioBitrate;
    }
}
