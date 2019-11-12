// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Examples;

/// <summary>
/// Example 8: Advanced Transcoding Scenarios
/// Demonstrates various transcoding scenarios with different quality/performance tradeoffs.
/// Usage: dotnet run --project examples/08-advanced-transcoding.csproj input-file output-dir [preset]
/// Presets: web, streaming, mobile, archive
/// </summary>
public class AdvancedTranscodingExample
{
    public static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: 08-advanced-transcoding <input-file> <output-dir> [preset]");
            Console.WriteLine("Presets:");
            Console.WriteLine("  web       - Optimized for web (VP9, 1280x720)");
            Console.WriteLine("  streaming - Optimized for streaming (H264, 1920x1080)");
            Console.WriteLine("  mobile    - Optimized for mobile (H264, 854x480)");
            Console.WriteLine("  archive   - High quality archival (H265, lossless audio)");
            return;
        }

        var inputFile = args[0];
        var outputDir = args[1];
        var preset = args.Length > 2 ? args[2] : "web";

        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: File not found: {inputFile}");
            return;
        }

        Directory.CreateDirectory(outputDir);

        // Setup dependency injection
        var services = new ServiceCollection();

        services.AddLogging(builder =>
            builder.AddConsole()
                   .SetMinimumLevel(LogLevel.Information));

        services.AddFFmpegWrapper(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(600);
            options.EnableDetailedLogging = false;
        });

        var serviceProvider = services.BuildServiceProvider();
        var ffmpeg = serviceProvider.GetRequiredService<IFFmpegService>();
        var logger = serviceProvider.GetRequiredService<ILogger<AdvancedTranscodingExample>>();

        try
        {
            // Analyze input
            logger.LogInformation("Analyzing input file: {File}", inputFile);
            var media = new MediaFile { Path = inputFile };
            var analyzed = await ffmpeg.AnalyzeMediaAsync(media).ConfigureAwait(false);

            logger.LogInformation("Duration: {Duration}", analyzed.Duration);
            logger.LogInformation("Resolution: {Width}x{Height}", analyzed.Width, analyzed.Height);
            logger.LogInformation("Bitrate: {Bitrate} kbps", analyzed.Bitrate / 1000);

            // Select preset
            var settings = preset switch
            {
                "web" => CreateWebPreset(),
                "streaming" => CreateStreamingPreset(),
                "mobile" => CreateMobilePreset(),
                "archive" => CreateArchivePreset(),
                _ => CreateWebPreset()
            };

            var outputFile = Path.Combine(outputDir,
                $"{Path.GetFileNameWithoutExtension(inputFile)}__{preset}.{GetExtension(settings.Container)}");

            logger.LogInformation("Transcoding with preset: {Preset}", preset);
            logger.LogInformation("Settings:");
            logger.LogInformation("  Video codec:  {Codec}", settings.VideoCodec);
            logger.LogInformation("  Audio codec:  {Codec}", settings.AudioCodec);
            logger.LogInformation("  Bitrate:      {Bitrate}k", settings.VideoBitrate);
            logger.LogInformation("  Quality:      {Quality}", settings.Quality);
            logger.LogInformation("  Resolution:   max {Width}x{Height}", settings.MaxWidth, settings.MaxHeight);

            // Progress reporting
            var startTime = DateTime.UtcNow;
            var progress = new Progress<OperationStatistics>(stat =>
            {
                var speed = analyzed.Duration.TotalSeconds > 0
                    ? (analyzed.Duration.TotalSeconds / stat.ElapsedTime.TotalSeconds)
                    : 0;

                Console.Write($"\rProgress: {stat.Percentage:F1}% | Speed: {speed:F2}x | " +
                    $"Time: {stat.ElapsedTime.TotalSeconds:F0}s");
            });

            // Perform transcode
            var result = await ffmpeg.TranscodeAsync(inputFile, outputFile, settings, progress).ConfigureAwait(false);

            Console.WriteLine();

            if (result.Success)
            {
                var elapsed = DateTime.UtcNow - startTime;
                var outputSize = new FileInfo(outputFile).Length;
                var inputSize = new FileInfo(inputFile).Length;
                var compression = (1.0 - (double)outputSize / inputSize) * 100;

                logger.LogInformation("✓ Transcode completed successfully");
                logger.LogInformation("Output: {Path}", outputFile);
                logger.LogInformation("Input size:   {Input} MB", inputSize / 1_000_000.0);
                logger.LogInformation("Output size:  {Output} MB", outputSize / 1_000_000.0);
                logger.LogInformation("Compression:  {Compression:F1}%", compression);
                logger.LogInformation("Encoding time: {Duration}m {Seconds}s",
                    (int)elapsed.TotalMinutes, (int)elapsed.Seconds);

                // Calculate efficiency
                var speed = analyzed.Duration.TotalSeconds / result.ElapsedTime.TotalSeconds;
                logger.LogInformation("Encoding speed: {Speed:F2}x", speed);
            }
            else
            {
                logger.LogError("✗ Transcode failed: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during transcoding");
        }
    }

    private static TranscodeSettings CreateWebPreset()
    {
        return new TranscodeSettings
        {
            VideoCodec = VideoCodec.VP9,
            AudioCodec = AudioCodec.Opus,
            Container = ContainerFormat.WebM,
            VideoBitrate = 1500,
            AudioBitrate = 96,
            FrameRate = 30,
            Quality = QualityPreset.High,
            EnableAutoScale = true,
            MaxWidth = 1280,
            MaxHeight = 720,
            PreserveAspectRatio = true
        };
    }

    private static TranscodeSettings CreateStreamingPreset()
    {
        return new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = 5000,
            AudioBitrate = 192,
            FrameRate = 30,
            Quality = QualityPreset.High,
            EnableAutoScale = true,
            MaxWidth = 1920,
            MaxHeight = 1080,
            PreserveAspectRatio = true
        };
    }

    private static TranscodeSettings CreateMobilePreset()
    {
        return new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = 800,
            AudioBitrate = 96,
            FrameRate = 24,
            Quality = QualityPreset.Medium,
            EnableAutoScale = true,
            MaxWidth = 854,
            MaxHeight = 480,
            PreserveAspectRatio = true
        };
    }

    private static TranscodeSettings CreateArchivePreset()
    {
        return new TranscodeSettings
        {
            VideoCodec = VideoCodec.H265,
            AudioCodec = AudioCodec.FLAC,
            Container = ContainerFormat.MKV,
            VideoBitrate = 8000,
            AudioBitrate = 320,
            FrameRate = 30,
            Quality = QualityPreset.Lossless,
            EnableAutoScale = false,
            PreserveAspectRatio = true
        };
    }

    private static string GetExtension(ContainerFormat format)
    {
        return format switch
        {
            ContainerFormat.MP4 => "mp4",
            ContainerFormat.WebM => "webm",
            ContainerFormat.MKV => "mkv",
            ContainerFormat.Ogg => "ogv",
            ContainerFormat.AVI => "avi",
            _ => "mp4"
        };
    }
}
