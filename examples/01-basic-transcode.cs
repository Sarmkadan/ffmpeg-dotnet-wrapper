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
/// Example 1: Basic Video Transcoding
/// Demonstrates how to transcode a video file from one format to another.
/// Usage: dotnet run --project examples/01-basic-transcode.csproj
/// </summary>
public class BasicTranscodeExample
{
    public static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: 01-basic-transcode <input-file> <output-file>");
            Console.WriteLine("Example: 01-basic-transcode video.mp4 output.webm");
            return;
        }

        var inputFile = args[0];
        var outputFile = args[1];

        // Verify input file exists
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: Input file not found: {inputFile}");
            return;
        }

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
        var logger = serviceProvider.GetRequiredService<ILogger<BasicTranscodeExample>>();

        try
        {
            // Check FFmpeg availability
            var available = await ffmpeg.IsFFmpegAvailableAsync();
            if (!available)
            {
                logger.LogError("FFmpeg is not installed or not available in PATH");
                return;
            }

            // Create transcode settings for WebM output
            var settings = new TranscodeSettings
            {
                VideoCodec = VideoCodec.VP9,
                AudioCodec = AudioCodec.Opus,
                Container = ContainerFormat.WebM,
                VideoBitrate = 1500,           // 1.5 Mbps
                AudioBitrate = 96,             // 96 kbps
                FrameRate = 30,
                Quality = QualityPreset.Medium,
                EnableAutoScale = true,
                MaxWidth = 1280,
                MaxHeight = 720,
                PreserveAspectRatio = true
            };

            logger.LogInformation("Starting transcode: {InputFile} -> {OutputFile}", inputFile, outputFile);
            logger.LogInformation("Settings: {VideoCodec} + {AudioCodec}, {Bitrate}k",
                settings.VideoCodec, settings.AudioCodec, settings.VideoBitrate);

            // Create progress reporter
            var progress = new Progress<OperationStatistics>(stat =>
            {
                Console.Write($"\rProgress: {stat.Percentage:F1}% | " +
                    $"Elapsed: {stat.ElapsedTime.TotalSeconds:F0}s | " +
                    $"ETA: {stat.EstimatedTimeRemaining?.TotalSeconds:F0}s");
            });

            // Perform transcode
            var result = await ffmpeg.TranscodeAsync(inputFile, outputFile, settings, progress);

            Console.WriteLine();  // New line after progress

            if (result.Success)
            {
                var fileInfo = new FileInfo(outputFile);
                logger.LogInformation("✓ Transcode completed successfully");
                logger.LogInformation("Duration: {Duration}s", result.ElapsedTime.TotalSeconds);
                logger.LogInformation("Output file: {Path} ({Size} MB)",
                    outputFile, fileInfo.Length / 1_000_000.0);
            }
            else
            {
                logger.LogError("✗ Transcode failed: {Error}", result.ErrorMessage);
                logger.LogError("Exit code: {ExitCode}", result.ExitCode);
                if (!string.IsNullOrEmpty(result.RawOutput))
                    logger.LogError("FFmpeg output: {Output}", result.RawOutput);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during transcode");
        }
    }
}
