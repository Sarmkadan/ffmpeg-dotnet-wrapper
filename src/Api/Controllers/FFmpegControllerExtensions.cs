// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Api.DTOs;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Api.Controllers
{
    /// <summary>
    /// Extension methods for <see cref="FFmpegController"/> that provide common video processing operations
    /// and convenience methods for chaining multiple operations together.
    /// </summary>
    public static class FFmpegControllerExtensions
    {
        /// <summary>
        /// Extracts media file information including codec, resolution, duration, and bitrate.
        /// </summary>
        /// <param name="controller">The FFmpeg controller instance</param>
        /// <param name="filePath">Path to the media file</param>
        /// <returns>Media file information with metadata</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
        public static ApiResponse<MediaFile> ExtractMediaInfo(this FFmpegController controller, string filePath)
        {
            ArgumentNullException.ThrowIfNull(controller);

            ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

            if (!File.Exists(filePath))
                return ApiResponse<MediaFile>.Failure("File does not exist");

            try
            {
                return controller.GetMediaInfoAsync(filePath);
            }
            catch (Exception ex)
            {
                return ApiResponse<MediaFile>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Transcodes a video file with automatic format detection based on output extension.
        /// </summary>
        /// <param name="controller">The FFmpeg controller instance</param>
        /// <param name="inputPath">Path to the input video file</param>
        /// <param name="outputPath">Path to save the transcoded output</param>
        /// <param name="bitrate">Target bitrate in kbps (e.g., 2000 for 2Mbps)</param>
        /// <param name="quality">Quality level (0-100, where higher is better quality)</param>
        /// <returns>Conversion result with output file information</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="inputPath"/> or <paramref name="outputPath"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
        public static async Task<ApiResponse<ConversionResult>> TranscodeAsync(
            this FFmpegController controller,
            string inputPath,
            string outputPath,
            int bitrate = 2000,
            int quality = 85)
        {
            ArgumentNullException.ThrowIfNull(controller);

            ArgumentException.ThrowIfNullOrWhiteSpace(inputPath, nameof(inputPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(outputPath));

            if (!File.Exists(inputPath))
                return ApiResponse<ConversionResult>.Failure("Input file does not exist");

            try
            {
                var request = new TranscodeRequest
                {
                    InputPath = inputPath,
                    OutputPath = outputPath,
                    Bitrate = bitrate,
                    Quality = quality,
                    OutputFormat = Path.GetExtension(outputPath)?.TrimStart('.') ?? "mp4"
                };

                return await controller.TranscodeAsync(request);
            }
            catch (Exception ex)
            {
                return ApiResponse<ConversionResult>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Trims a video file to a specific duration starting from the beginning.
        /// </summary>
        /// <param name="controller">The FFmpeg controller instance</param>
        /// <param name="inputPath">Path to the input video file</param>
        /// <param name="outputPath">Path to save the trimmed output</param>
        /// <param name="duration">Duration to keep in seconds</param>
        /// <returns>Conversion result with output file information</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="inputPath"/> or <paramref name="outputPath"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
        public static async Task<ApiResponse<ConversionResult>> TrimFromStartAsync(
            this FFmpegController controller,
            string inputPath,
            string outputPath,
            double duration)
        {
            ArgumentNullException.ThrowIfNull(controller);

            ArgumentException.ThrowIfNullOrWhiteSpace(inputPath, nameof(inputPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(outputPath));

            if (!File.Exists(inputPath))
                return ApiResponse<ConversionResult>.Failure("Input file does not exist");

            if (duration <= 0)
                return ApiResponse<ConversionResult>.Failure("Duration must be positive");

            try
            {
                var request = new TrimRequest
                {
                    InputPath = inputPath,
                    OutputPath = outputPath,
                    Duration = TimeSpan.FromSeconds(duration)
                };

                return await controller.TrimAsync(request);
            }
            catch (Exception ex)
            {
                return ApiResponse<ConversionResult>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Trims a video file between start and end times.
        /// </summary>
        /// <param name="controller">The FFmpeg controller instance</param>
        /// <param name="inputPath">Path to the input video file</param>
        /// <param name="outputPath">Path to save the trimmed output</param>
        /// <param name="startTime">Start time in seconds</param>
        /// <param name="endTime">End time in seconds</param>
        /// <returns>Conversion result with output file information</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="inputPath"/> or <paramref name="outputPath"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
        public static async Task<ApiResponse<ConversionResult>> TrimAsync(
            this FFmpegController controller,
            string inputPath,
            string outputPath,
            double startTime,
            double endTime)
        {
            ArgumentNullException.ThrowIfNull(controller);

            ArgumentException.ThrowIfNullOrWhiteSpace(inputPath, nameof(inputPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(outputPath));

            if (!File.Exists(inputPath))
                return ApiResponse<ConversionResult>.Failure("Input file does not exist");

            if (startTime < 0 || endTime <= startTime)
                return ApiResponse<ConversionResult>.Failure("Invalid time range");

            try
            {
                var request = new TrimRequest
                {
                    InputPath = inputPath,
                    OutputPath = outputPath,
                    StartTime = TimeSpan.FromSeconds(startTime),
                    EndTime = TimeSpan.FromSeconds(endTime)
                };

                return await controller.TrimAsync(request);
            }
            catch (Exception ex)
            {
                return ApiResponse<ConversionResult>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Merges multiple video files into a single output file.
        /// </summary>
        /// <param name="controller">The FFmpeg controller instance</param>
        /// <param name="inputPaths">List of input file paths to merge</param>
        /// <param name="outputPath">Path to save the merged output</param>
        /// <param name="maintainAspectRatio">Whether to maintain aspect ratio</param>
        /// <returns>Conversion result with output file information</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="outputPath"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
        public static async Task<ApiResponse<ConversionResult>> MergeAsync(
            this FFmpegController controller,
            IEnumerable<string> inputPaths,
            string outputPath,
            bool maintainAspectRatio = true)
        {
            ArgumentNullException.ThrowIfNull(controller);

            ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(outputPath));

            if (inputPaths == null || !inputPaths.Any())
                return ApiResponse<ConversionResult>.Failure("No input files provided");

            if (inputPaths.Any(p => !File.Exists(p)))
                return ApiResponse<ConversionResult>.Failure("One or more input files do not exist");

            try
            {
                var request = new MergeRequest
                {
                    InputPaths = new List<string>(inputPaths),
                    OutputPath = outputPath,
                    MaintainAspectRatio = maintainAspectRatio
                };

                return await controller.MergeAsync(request);
            }
            catch (Exception ex)
            {
                return ApiResponse<ConversionResult>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Adds a watermark to a video file with default positioning (bottom-right).
        /// </summary>
        /// <param name="controller">The FFmpeg controller instance</param>
        /// <param name="inputPath">Path to the input video file</param>
        /// <param name="outputPath">Path to save the watermarked output</param>
        /// <param name="watermarkPath">Path to the watermark image file</param>
        /// <param name="opacity">Watermark opacity (0-1)</param>
        /// <param name="scale">Watermark scale factor (0.1-1.0)</param>
        /// <returns>Conversion result with output file information</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="inputPath"/>, <paramref name="outputPath"/>, or <paramref name="watermarkPath"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
        public static async Task<ApiResponse<ConversionResult>> AddWatermarkAsync(
            this FFmpegController controller,
            string inputPath,
            string outputPath,
            string watermarkPath,
            double opacity = 0.5,
            double scale = 0.2)
        {
            ArgumentNullException.ThrowIfNull(controller);

            ArgumentException.ThrowIfNullOrWhiteSpace(inputPath, nameof(inputPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(outputPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(watermarkPath, nameof(watermarkPath));

            if (!File.Exists(inputPath))
                return ApiResponse<ConversionResult>.Failure("Input file does not exist");

            if (!File.Exists(watermarkPath))
                return ApiResponse<ConversionResult>.Failure("Watermark file does not exist");

            if (opacity < 0 || opacity > 1)
                return ApiResponse<ConversionResult>.Failure("Opacity must be between 0 and 1");

            if (scale <= 0 || scale > 1)
                return ApiResponse<ConversionResult>.Failure("Scale must be between 0 and 1");

            try
            {
                var request = new WatermarkRequest
                {
                    InputPath = inputPath,
                    OutputPath = outputPath,
                    WatermarkPath = watermarkPath,
                    Opacity = opacity,
                    Scale = scale,
                    PositionX = "W-w*0.1",
                    PositionY = "H-h*0.1"
                };

                return await controller.WatermarkAsync(request);
            }
            catch (Exception ex)
            {
                return ApiResponse<ConversionResult>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Extracts thumbnails from a video file at evenly spaced intervals.
        /// </summary>
        /// <param name="controller">The FFmpeg controller instance</param>
        /// <param name="inputPath">Path to the input video file</param>
        /// <param name="outputPattern">Output file pattern (e.g., "thumbnails/thumb_{0}.jpg")</param>
        /// <param name="count">Number of thumbnails to extract</param>
        /// <param name="width">Thumbnail width in pixels</param>
        /// <param name="height">Thumbnail height in pixels</param>
        /// <param name="format">Thumbnail format (jpg or png)</param>
        /// <returns>Thumbnail extraction result with paths to generated thumbnails</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="inputPath"/> or <paramref name="outputPattern"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
        public static async Task<ApiResponse<ThumbnailResult>> ExtractThumbnailsAsync(
            this FFmpegController controller,
            string inputPath,
            string outputPattern,
            int count = 5,
            int width = 320,
            int height = 240,
            string format = "jpg")
        {
            ArgumentNullException.ThrowIfNull(controller);

            ArgumentException.ThrowIfNullOrWhiteSpace(inputPath, nameof(inputPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(outputPattern, nameof(outputPattern));

            if (!File.Exists(inputPath))
                return ApiResponse<ThumbnailResult>.Failure("Input file does not exist");

            if (count <= 0)
                return ApiResponse<ThumbnailResult>.Failure("Count must be positive");

            if (width <= 0 || height <= 0)
                return ApiResponse<ThumbnailResult>.Failure("Width and height must be positive");

            try
            {
                var timestamps = new List<double>();
                var mediaInfo = await controller.GetMediaInfoAsync(inputPath);

                if (mediaInfo.Success && mediaInfo.Data != null && mediaInfo.Data.Duration.HasValue)
                {
                    var duration = mediaInfo.Data.Duration.Value.TotalSeconds;
                    var interval = duration / (count + 1);

                    for (int i = 1; i <= count; i++)
                    {
                        timestamps.Add(i * interval);
                    }
                }
                else
                {
                    // Default: extract at 10%, 30%, 50%, 70%, 90% of video
                    for (int i = 1; i <= count; i++)
                    {
                        timestamps.Add(i * 10.0);
                    }
                }

                var request = new ThumbnailRequest
                {
                    InputPath = inputPath,
                    OutputPattern = outputPattern,
                    Count = count,
                    Width = width,
                    Height = height,
                    Format = format,
                    TimestampsSeconds = timestamps
                };

                return await controller.ExtractThumbnailsAsync(request);
            }
            catch (Exception ex)
            {
                return ApiResponse<ThumbnailResult>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Embeds subtitles into a video file (hard embed - burned into frames).
        /// </summary>
        /// <param name="controller">The FFmpeg controller instance</param>
        /// <param name="inputPath">Path to the input video file</param>
        /// <param name="outputPath">Path to save the output with embedded subtitles</param>
        /// <param name="subtitlePath">Path to the subtitle file (.srt, .ass, etc.)</param>
        /// <param name="language">Subtitle language code</param>
        /// <param name="fontName">Font name for subtitle rendering</param>
        /// <param name="fontSize">Font size in pixels</param>
        /// <returns>Conversion result with output file information</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="inputPath"/>, <paramref name="outputPath"/>, <paramref name="subtitlePath"/>, or <paramref name="language"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
        public static async Task<ApiResponse<ConversionResult>> EmbedSubtitlesAsync(
            this FFmpegController controller,
            string inputPath,
            string outputPath,
            string subtitlePath,
            string language = "eng",
            string fontName = "Arial",
            int fontSize = 24)
        {
            ArgumentNullException.ThrowIfNull(controller);

            ArgumentException.ThrowIfNullOrWhiteSpace(inputPath, nameof(inputPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(outputPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(subtitlePath, nameof(subtitlePath));
            ArgumentException.ThrowIfNullOrWhiteSpace(language, nameof(language));
            ArgumentException.ThrowIfNullOrWhiteSpace(fontName, nameof(fontName));

            if (!File.Exists(inputPath))
                return ApiResponse<ConversionResult>.Failure("Input file does not exist");

            if (!File.Exists(subtitlePath))
                return ApiResponse<ConversionResult>.Failure("Subtitle file does not exist");

            try
            {
                var request = new SubtitleRequest
                {
                    InputPath = inputPath,
                    OutputPath = outputPath,
                    SubtitlePath = subtitlePath,
                    HardEmbed = true,
                    Language = language,
                    FontName = fontName,
                    FontSize = fontSize
                };

                return await controller.EmbedSubtitlesAsync(request);
            }
            catch (Exception ex)
            {
                return ApiResponse<ConversionResult>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Chains multiple operations together: trim, then add watermark, then transcode.
        /// </summary>
        /// <param name="controller">The FFmpeg controller instance</param>
        /// <param name="inputPath">Path to the input video file</param>
        /// <param name="finalOutputPath">Path for the final output</param>
        /// <param name="trimDuration">Duration to keep in seconds</param>
        /// <param name="watermarkPath">Path to the watermark image</param>
        /// <param name="targetBitrate">Target bitrate for final transcode</param>
        /// <returns>Final conversion result with all intermediate files cleaned up</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="inputPath"/>, <paramref name="finalOutputPath"/>, or <paramref name="watermarkPath"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
        public static async Task<ApiResponse<ConversionResult>> TrimWatermarkTranscodeAsync(
            this FFmpegController controller,
            string inputPath,
            string finalOutputPath,
            double trimDuration,
            string watermarkPath,
            int targetBitrate = 2000)
        {
            ArgumentNullException.ThrowIfNull(controller);

            ArgumentException.ThrowIfNullOrWhiteSpace(inputPath, nameof(inputPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(finalOutputPath, nameof(finalOutputPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(watermarkPath, nameof(watermarkPath));

            if (!File.Exists(inputPath))
                return ApiResponse<ConversionResult>.Failure("Input file does not exist");

            if (trimDuration <= 0)
                return ApiResponse<ConversionResult>.Failure("Duration must be positive");

            if (!File.Exists(watermarkPath))
                return ApiResponse<ConversionResult>.Failure("Watermark file does not exist");

            try
            {
                // Step 1: Trim
                var tempTrimPath = Path.Combine(
                    Path.GetDirectoryName(finalOutputPath) ?? string.Empty,
                    $"temp_trim_{Guid.NewGuid()}{Path.GetExtension(finalOutputPath)}");

                var trimResult = await controller.TrimFromStartAsync(inputPath, tempTrimPath, trimDuration);

                if (!trimResult.Success || trimResult.Data == null)
                {
                    return ApiResponse<ConversionResult>.Failure(
                        $"Trim failed: {trimResult.Message}",
                        trimResult.StatusCode);
                }

                // Step 2: Add watermark
                var tempWatermarkPath = Path.Combine(
                    Path.GetDirectoryName(finalOutputPath) ?? string.Empty,
                    $"temp_watermark_{Guid.NewGuid()}{Path.GetExtension(finalOutputPath)}");

                var watermarkResult = await controller.AddWatermarkAsync(
                    tempTrimPath,
                    tempWatermarkPath,
                    watermarkPath);

                // Clean up temp trim file
                try { if (File.Exists(tempTrimPath)) File.Delete(tempTrimPath); } catch { }

                if (!watermarkResult.Success || watermarkResult.Data == null)
                {
                    return ApiResponse<ConversionResult>.Failure(
                        $"Watermark failed: {watermarkResult.Message}",
                        watermarkResult.StatusCode);
                }

                // Step 3: Transcode to final format
                var transcodeResult = await controller.TranscodeAsync(
                    tempWatermarkPath,
                    finalOutputPath,
                    targetBitrate);

                // Clean up temp watermark file
                try { if (File.Exists(tempWatermarkPath)) File.Delete(tempWatermarkPath); } catch { }

                return transcodeResult;
            }
            catch (Exception ex)
            {
                return ApiResponse<ConversionResult>.Failure(ex.Message);
            }
        }
    }
}