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
/// Example 5: Video Watermarking
/// Demonstrates how to add a watermark/logo overlay to a video.
/// Usage: dotnet run --project examples/05-watermarking.csproj video-file watermark-file [position]
/// Example: dotnet run examples/05-watermarking video.mp4 logo.png TopRight
/// </summary>
public class WatermarkingExample
{
    public static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: 05-watermarking <video-file> <watermark-file> [position]");
            Console.WriteLine("Positions: TopLeft, TopRight, TopCenter, BottomLeft, BottomRight, BottomCenter, Center");
            Console.WriteLine("Example: 05-watermarking video.mp4 logo.png TopRight");
            return;
        }

        var videoFile = args[0];
        var watermarkFile = args[1];
        var positionStr = args.Length > 2 ? args[2] : "TopRight";
        var outputFile = Path.GetFileNameWithoutExtension(videoFile) + "_watermarked.mp4";

        // Verify input files
        if (!File.Exists(videoFile))
        {
            Console.WriteLine($"Error: Video file not found: {videoFile}");
            return;
        }

        if (!File.Exists(watermarkFile))
        {
            Console.WriteLine($"Error: Watermark file not found: {watermarkFile}");
            return;
        }

        // Parse position
        if (!Enum.TryParse<WatermarkPosition>(positionStr, out var position))
        {
            Console.WriteLine($"Error: Invalid position '{positionStr}'");
            Console.WriteLine("Valid positions: TopLeft, TopRight, TopCenter, BottomLeft, BottomRight, BottomCenter, Center");
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
        var logger = serviceProvider.GetRequiredService<ILogger<WatermarkingExample>>();

        try
        {
            // Analyze video
            logger.LogInformation("Analyzing video: {File}", videoFile);
            var media = new MediaFile { Path = videoFile };
            var videoProps = await ffmpeg.AnalyzeMediaAsync(media).ConfigureAwait(false);

            logger.LogInformation("Video resolution: {Width}x{Height}", videoProps.Width, videoProps.Height);
            logger.LogInformation("Duration: {Duration}s", videoProps.Duration.TotalSeconds);

            // Analyze watermark
            var watermarkMedia = new MediaFile { Path = watermarkFile };
            var watermarkProps = await ffmpeg.AnalyzeMediaAsync(watermarkMedia).ConfigureAwait(false);
            logger.LogInformation("Watermark size: {Width}x{Height}", watermarkProps.Width, watermarkProps.Height);

            // Create watermark settings
            var settings = new WatermarkSettings
            {
                Position = position,
                XOffset = 15,                   // 15 pixels from edge
                YOffset = 15,
                Scale = 0.15,                   // 15% of video width
                Opacity = 0.8,                  // 80% opacity
                PreserveAspectRatio = true
            };

            logger.LogInformation("Adding watermark at {Position} position",
                settings.Position);
            logger.LogInformation("  Scale: {Scale}% of video width", settings.Scale * 100);
            logger.LogInformation("  Opacity: {Opacity}%", settings.Opacity * 100);
            logger.LogInformation("  Offset: X={X}px, Y={Y}px", settings.XOffset, settings.YOffset);

            // Create progress reporter
            var progress = new Progress<OperationStatistics>(stat =>
            {
                Console.Write($"\rProgress: {stat.Percentage:F1}% | " +
                    $"Elapsed: {stat.ElapsedTime.TotalSeconds:F0}s");
            });

            // Perform watermarking
            var result = await ffmpeg.WatermarkAsync(
                videoFile,
                watermarkFile,
                outputFile,
                settings,
                progress);

            Console.WriteLine();  // New line after progress

            if (result.Success)
            {
                var outputInfo = new FileInfo(outputFile);
                logger.LogInformation("✓ Watermarking completed successfully");
                logger.LogInformation("Output: {Path}", outputFile);
                logger.LogInformation("Size: {Size} MB", outputInfo.Length / 1_000_000.0);
                logger.LogInformation("Duration: {Duration}s", result.ElapsedTime.TotalSeconds);
            }
            else
            {
                logger.LogError("✗ Watermarking failed: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during watermarking");
        }
    }
}
