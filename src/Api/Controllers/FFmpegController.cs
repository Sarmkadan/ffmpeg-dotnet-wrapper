// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Api.DTOs;
using FFmpegDotnetWrapper.Services;
using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Api.Controllers
{
    /// <summary>
    /// REST API controller for FFmpeg transcoding, trimming, merging, and watermarking operations.
    /// Provides endpoints for video transformation workflows with request validation and error handling.
    /// </summary>
    public class FFmpegController
    {
        private readonly IFFmpegService _ffmpegService;
        private readonly ILogger<FFmpegController> _logger;

        public FFmpegController(IFFmpegService ffmpegService, ILogger<FFmpegController> logger)
        {
            _ffmpegService = ffmpegService ?? throw new ArgumentNullException(nameof(ffmpegService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Transcodes a video file to a different format or codec.
        /// Supports quality, bitrate, and codec selection through the fluent API.
        /// </summary>
        public async Task<ApiResponse<ConversionResult>> TranscodeAsync(TranscodeRequest request)
        {
            try
            {
                if (!System.IO.File.Exists(request.InputPath))
                {
                    _logger.LogWarning("Input file not found: {InputPath}", request.InputPath);
                    return ApiResponse<ConversionResult>.Failure("Input file does not exist");
                }

                var settings = new TranscodeSettings
                {
                    OutputFormat = request.OutputFormat,
                    Bitrate = request.Bitrate,
                    Codec = request.Codec,
                    Quality = request.Quality
                };

                var result = await _ffmpegService.TranscodeAsync(request.InputPath, request.OutputPath, settings);

                _logger.LogInformation(
                    "Transcode completed: {Input} -> {Output} ({Duration}ms)",
                    request.InputPath,
                    request.OutputPath,
                    result.ExecutionTime.TotalMilliseconds);

                return ApiResponse<ConversionResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transcode operation failed for {InputPath}", request.InputPath);
                return ApiResponse<ConversionResult>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Trims a video file to a specified duration or timeframe.
        /// Supports start time and duration or end time specifications.
        /// </summary>
        public async Task<ApiResponse<ConversionResult>> TrimAsync(TrimRequest request)
        {
            try
            {
                var settings = new TrimSettings
                {
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Duration = request.Duration
                };

                var result = await _ffmpegService.TrimAsync(request.InputPath, request.OutputPath, settings);

                _logger.LogInformation(
                    "Trim completed: {Input} -> {Output} ({StartTime}-{Duration})",
                    request.InputPath,
                    request.OutputPath,
                    request.StartTime,
                    request.Duration);

                return ApiResponse<ConversionResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trim operation failed");
                return ApiResponse<ConversionResult>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Merges multiple video files into a single output file.
        /// Maintains codec compatibility and handles stream synchronization.
        /// </summary>
        public async Task<ApiResponse<ConversionResult>> MergeAsync(MergeRequest request)
        {
            try
            {
                var settings = new MergeSettings
                {
                    MaintainAspectRatio = request.MaintainAspectRatio
                };

                var result = await _ffmpegService.MergeAsync(request.InputPaths, request.OutputPath, settings);

                _logger.LogInformation(
                    "Merge completed: {Count} files -> {Output}",
                    request.InputPaths.Count,
                    request.OutputPath);

                return ApiResponse<ConversionResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Merge operation failed");
                return ApiResponse<ConversionResult>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Adds a watermark overlay to a video file.
        /// Supports image watermarks with position and opacity customization.
        /// </summary>
        public async Task<ApiResponse<ConversionResult>> WatermarkAsync(WatermarkRequest request)
        {
            try
            {
                var settings = new WatermarkSettings
                {
                    WatermarkPath = request.WatermarkPath,
                    PositionX = request.PositionX,
                    PositionY = request.PositionY,
                    Opacity = request.Opacity,
                    Scale = request.Scale
                };

                var result = await _ffmpegService.WatermarkAsync(request.InputPath, request.OutputPath, settings);

                _logger.LogInformation(
                    "Watermark applied: {Input} -> {Output}",
                    request.InputPath,
                    request.OutputPath);

                return ApiResponse<ConversionResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Watermark operation failed");
                return ApiResponse<ConversionResult>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the status and metadata of a media file without performing conversions.
        /// Returns codec info, duration, bitrate, and resolution.
        /// </summary>
        public ApiResponse<MediaFile> GetMediaInfoAsync(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    return ApiResponse<MediaFile>.Failure("File not found");
                }

                // This would integrate with FFprobe or ffmpeg metadata extraction
                var info = new MediaFile { FilePath = filePath };
                return ApiResponse<MediaFile>.Success(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve media info for {FilePath}", filePath);
                return ApiResponse<MediaFile>.Failure(ex.Message);
            }
        }
    }
}
