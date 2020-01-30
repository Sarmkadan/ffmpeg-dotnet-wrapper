// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using System.Globalization;
using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Repository;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Main implementation of FFmpeg service orchestrating all media operations.
/// </summary>
public class FFmpegService : IFFmpegService
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IOperationRepository _operationRepository;
    private readonly ILogger<FFmpegService> _logger;
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;
    private readonly TimeSpan _defaultTimeout;

    public FFmpegService(
        IMediaRepository mediaRepository,
        IOperationRepository operationRepository,
        ILogger<FFmpegService> logger)
    {
        _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
        _operationRepository = operationRepository ?? throw new ArgumentNullException(nameof(operationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _ffmpegPath = ResolveExecutablePath(FFmpegConstants.FFmpegExecutableName);
        _ffprobePath = ResolveExecutablePath(FFmpegConstants.FFprobeExecutableName);
        _defaultTimeout = TimeSpan.FromSeconds(FFmpegConstants.DefaultTimeoutSeconds);
    }

    public async Task<ConversionResult> TranscodeAsync(
        MediaFile inputMedia,
        string outputPath,
        TranscodeSettings settings,
        CancellationToken cancellationToken = default)
    {
        var operation = new FFmpegOperation
        {
            Name = $"Transcode {inputMedia.Name}",
            Type = FFmpegOperationType.Transcode,
            OutputFile = outputPath,
            Timeout = _defaultTimeout
        };

        operation.AddInputFile(inputMedia.FilePath);

        try
        {
            inputMedia.ValidateAsVideo();
            settings.Validate();

            // Build transcoding arguments
            BuildTranscodeArguments(operation, settings);

            _logger.LogInformation("Starting transcode operation for {File}", inputMedia.Name);
            var result = await ExecuteFFmpegAsync(operation, cancellationToken);

            if (result.IsSuccess)
            {
                var outputMedia = await AnalyzeMediaAsync(outputPath, cancellationToken);
                result.OutputMedia = outputMedia;
                result.SetMetric("InputSize", inputMedia.FileSize);
                result.SetMetric("OutputSize", outputMedia.FileSize);
                result.SetMetric("SizeReduction", result.GetSizeReductionPercentage(inputMedia.FileSize));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transcode operation failed for {File}", inputMedia.Name);
            throw;
        }
    }

    public async Task<ConversionResult> TrimAsync(
        MediaFile inputMedia,
        string outputPath,
        TrimSettings settings,
        CancellationToken cancellationToken = default)
    {
        var operation = new FFmpegOperation
        {
            Name = $"Trim {inputMedia.Name}",
            Type = FFmpegOperationType.Trim,
            OutputFile = outputPath,
            Timeout = _defaultTimeout
        };

        operation.AddInputFile(inputMedia.FilePath);

        try
        {
            settings.Validate(inputMedia);

            // Build trim arguments
            BuildTrimArguments(operation, settings);

            _logger.LogInformation("Starting trim operation for {File}", inputMedia.Name);
            var result = await ExecuteFFmpegAsync(operation, cancellationToken);

            if (result.IsSuccess)
            {
                var outputMedia = await AnalyzeMediaAsync(outputPath, cancellationToken);
                result.OutputMedia = outputMedia;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Trim operation failed for {File}", inputMedia.Name);
            throw;
        }
    }

    public async Task<ConversionResult> MergeAsync(
        IEnumerable<string> inputFiles,
        string outputPath,
        MergeSettings settings,
        CancellationToken cancellationToken = default)
    {
        var operation = new FFmpegOperation
        {
            Name = "Merge media files",
            Type = FFmpegOperationType.Merge,
            OutputFile = outputPath,
            Timeout = TimeSpan.FromSeconds(FFmpegConstants.DefaultTimeoutSeconds * 2)
        };

        foreach (var file in inputFiles)
            operation.AddInputFile(file);

        try
        {
            settings.InputFiles = operation.InputFiles;
            settings.Validate();

            _logger.LogInformation("Starting merge operation with {Count} files", operation.InputFiles.Count);
            var result = await ExecuteFFmpegAsync(operation, cancellationToken);

            if (result.IsSuccess)
            {
                var outputMedia = await AnalyzeMediaAsync(outputPath, cancellationToken);
                result.OutputMedia = outputMedia;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Merge operation failed");
            throw;
        }
    }

    public async Task<ConversionResult> AddWatermarkAsync(
        MediaFile inputMedia,
        string outputPath,
        WatermarkSettings settings,
        CancellationToken cancellationToken = default)
    {
        var operation = new FFmpegOperation
        {
            Name = $"Add watermark to {inputMedia.Name}",
            Type = FFmpegOperationType.Watermark,
            OutputFile = outputPath,
            Timeout = _defaultTimeout
        };

        operation.AddInputFile(inputMedia.FilePath);

        try
        {
            settings.Validate(inputMedia);

            // Build watermark arguments
            BuildWatermarkArguments(operation, settings, inputMedia);

            _logger.LogInformation("Starting watermark operation for {File}", inputMedia.Name);
            var result = await ExecuteFFmpegAsync(operation, cancellationToken);

            if (result.IsSuccess)
            {
                var outputMedia = await AnalyzeMediaAsync(outputPath, cancellationToken);
                result.OutputMedia = outputMedia;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Watermark operation failed for {File}", inputMedia.Name);
            throw;
        }
    }

    public async Task<MediaFile> AnalyzeMediaAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new InvalidMediaFileException($"File does not exist: {filePath}", filePath);

        var mediaFile = new MediaFile(filePath);

        try
        {
            _logger.LogInformation("Analyzing media file: {File}", filePath);

            var ffprobeArgs = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"";

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffprobePath,
                    Arguments = ffprobeArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                throw new FFmpegProcessException($"ffprobe failed with exit code {process.ExitCode}. Error: {error}", process.ExitCode);
            }

            var ffprobeResult = System.Text.Json.JsonDocument.Parse(output);
            var format = ffprobeResult.RootElement.GetProperty("format");

            mediaFile.FormatName = format.GetProperty("format_name").GetString();
            mediaFile.Duration = TimeSpan.FromSeconds(format.GetProperty("duration").GetDouble());
            mediaFile.FileSize = format.GetProperty("size").GetInt64();
            mediaFile.BitRate = format.GetProperty("bit_rate").GetInt64();

            if (ffprobeResult.RootElement.TryGetProperty("streams", out var streamsElement))
            {
                foreach (var streamElement in streamsElement.EnumerateArray())
                {
                    var codecType = streamElement.GetProperty("codec_type").GetString();
                    if (codecType == "video")
                    {
                        mediaFile.Width = streamElement.GetProperty("width").GetInt32();
                        mediaFile.Height = streamElement.GetProperty("height").GetInt32();
                        mediaFile.FrameRate = (int)Math.Round(streamElement.GetProperty("r_frame_rate").GetString().ParseDouble());
                        mediaFile.VideoCodec = streamElement.GetProperty("codec_name").GetString();
                    }
                    else if (codecType == "audio")
                    {
                        mediaFile.AudioCodec = streamElement.GetProperty("codec_name").GetString();
                        mediaFile.SampleRate = streamElement.GetProperty("sample_rate").GetInt32();
                        mediaFile.Channels = streamElement.GetProperty("channels").GetInt32();
                    }
                }
            }

            // Save the media file to the repository
            await _mediaRepository.AddAsync(mediaFile, cancellationToken);
            return mediaFile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Media analysis failed for {File}", filePath);
            throw;
        }
    }

    public async Task<ConversionResult> ExecuteCustomOperationAsync(
        FFmpegOperation operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            operation.Validate();
            _logger.LogInformation("Executing custom FFmpeg operation: {Name}", operation.Name);

            var result = await ExecuteFFmpegAsync(operation, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Custom operation failed: {Name}", operation.Name);
            throw;
        }
    }

    public async Task<string> GetFFmpegVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var versionOutput = await process.StandardOutput.ReadLineAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            return versionOutput ?? "Unknown";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get FFmpeg version");
            throw;
        }
    }

    public Task<bool> IsFFmpegAvailableAsync(CancellationToken cancellationToken = default)
    {
        bool available = File.Exists(_ffmpegPath) && File.Exists(_ffprobePath);
        _logger.LogInformation("FFmpeg availability check: {Available}", available);
        return Task.FromResult(available);
    }

    private async Task<ConversionResult> ExecuteFFmpegAsync(
        FFmpegOperation operation,
        CancellationToken cancellationToken)
    {
        var result = new ConversionResult
        {
            Duration = operation.Timeout ?? _defaultTimeout
        };

        var sw = Stopwatch.StartNew();

        try
        {
            await _operationRepository.AddAsync(operation, cancellationToken);

            var arguments = operation.BuildCommandLine();
            _logger.LogDebug("Executing FFmpeg command: {Command}", arguments);

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = operation.BuildCommandLine(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var timeout = operation.Timeout ?? _defaultTimeout;
            if (!await process.WaitForExitAsync(timeout, cancellationToken))
            {
                process.Kill();
                throw new FFmpegProcessException(
                    $"FFmpeg process timed out after {timeout.TotalSeconds} seconds",
                    timeout);
            }

            sw.Stop();
            result.Duration = sw.Elapsed;

            if (process.ExitCode == 0)
            {
                result.MarkAsSuccess(operation.OutputFile);
                _logger.LogInformation("FFmpeg operation completed successfully: {Name}", operation.Name);
            }
            else
            {
                var errorOutput = process.StandardError.ReadToEnd();
                result.MarkAsFailed($"FFmpeg exited with code {process.ExitCode}");
                result.FFmpegOutput = errorOutput;
                _logger.LogError("FFmpeg operation failed: {Error}", errorOutput);
            }

            operation.ExecutedAt = DateTime.UtcNow;
            await _operationRepository.UpdateAsync(operation, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            result.MarkAsFailed(ex.Message);
            _logger.LogError(ex, "FFmpeg execution error");
            throw;
        }
    }



    private void BuildTranscodeArguments(FFmpegOperation operation, TranscodeSettings settings)
    {
        var videoCodec = GetVideoCodecName(settings.VideoCodec);
        var audioCodec = GetAudioCodecName(settings.AudioCodec);

        operation.AddArgument($"-c:v {videoCodec}");
        operation.AddArgument($"-c:a {audioCodec}");
        operation.AddArgument($"-b:v {settings.VideoBitrate}k");
        operation.AddArgument($"-b:a {settings.AudioBitrate}k");
        operation.AddArgument($"-r {settings.FrameRate}");
        operation.AddArgument($"-preset {GetPresetName(settings.Quality)}");

        if (settings.Width.HasValue || settings.Height.HasValue)
        {
            var width = settings.Width ?? -1;
            var height = settings.Height ?? -1;
            operation.AddArgument($"-vf \"scale={width}:{height}\"");
        }

        if (!string.IsNullOrEmpty(settings.CustomFFmpegArgs))
            operation.AddArgument(settings.CustomFFmpegArgs);
    }

    private void BuildTrimArguments(FFmpegOperation operation, TrimSettings settings)
    {
        operation.AddArgument($"-ss {settings.StartTime.TotalSeconds}");

        if (settings.Duration.HasValue)
            operation.AddArgument($"-t {settings.Duration.Value.TotalSeconds}");
        else if (settings.EndTime.HasValue)
            operation.AddArgument($"-to {settings.EndTime.Value.TotalSeconds}");

        operation.AddArgument("-c copy"); // Copy without re-encoding
    }

    private void BuildWatermarkArguments(
        FFmpegOperation operation,
        WatermarkSettings settings,
        MediaFile videoFile)
    {
        var filter = $"overlay=";
        var (x, y) = settings.CalculatePosition(videoFile.Width ?? 1920, videoFile.Height ?? 1080);
        filter += $"{x}:{y}";

        operation.AddArgument($"-i \"{settings.WatermarkPath}\"");
        operation.AddArgument($"-filter_complex \"{filter}\"");
        operation.AddArgument($"-c:a copy");
    }

    private string GetVideoCodecName(VideoCodec codec) =>
        codec switch
        {
            VideoCodec.H264 => FFmpegConstants.VideoCodecNames.H264,
            VideoCodec.H265 => FFmpegConstants.VideoCodecNames.H265,
            VideoCodec.VP9 => FFmpegConstants.VideoCodecNames.VP9,
            VideoCodec.AV1 => FFmpegConstants.VideoCodecNames.AV1,
            _ => FFmpegConstants.VideoCodecNames.H264
        };

    private string GetAudioCodecName(AudioCodec codec) =>
        codec switch
        {
            AudioCodec.AAC => FFmpegConstants.AudioCodecNames.AAC,
            AudioCodec.MP3 => FFmpegConstants.AudioCodecNames.MP3,
            AudioCodec.OPUS => FFmpegConstants.AudioCodecNames.OPUS,
            AudioCodec.FLAC => FFmpegConstants.AudioCodecNames.FLAC,
            _ => FFmpegConstants.AudioCodecNames.AAC
        };

    private string GetPresetName(QualityPreset preset) =>
        preset switch
        {
            QualityPreset.Ultrafast => FFmpegConstants.PresetLevels.Ultrafast,
            QualityPreset.Slow => FFmpegConstants.PresetLevels.Slow,
            QualityPreset.Medium => FFmpegConstants.PresetLevels.Medium,
            _ => FFmpegConstants.PresetLevels.Medium
        };

    private string ResolveExecutablePath(string executableName)
    {
        // Check in PATH
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();

        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, executableName);
            if (File.Exists(fullPath))
                return fullPath;
        }

        // Return the executable name as fallback (assume it's in PATH)
        return executableName;
    }
}
