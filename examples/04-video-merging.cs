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
/// Example 4: Video Merging
/// Demonstrates how to concatenate multiple video files into one.
/// Usage: dotnet run --project examples/04-video-merging.csproj output-file file1 file2 file3...
/// Example: dotnet run examples/04-video-merging merged.mp4 intro.mp4 main.mp4 outro.mp4
/// </summary>
public class VideoMergingExample
{
    public static async Task Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: 04-video-merging <output-file> <input-file1> <input-file2> [input-file3]...");
            Console.WriteLine("Example: 04-video-merging merged.mp4 video1.mp4 video2.mp4 video3.mp4");
            return;
        }

        var outputFile = args[0];
        var inputFiles = args.Skip(1).ToArray();

        // Verify all input files exist
        foreach (var file in inputFiles)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"Error: Input file not found: {file}");
                return;
            }
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
        var logger = serviceProvider.GetRequiredService<ILogger<VideoMergingExample>>();

        try
        {
            // Analyze all input files
            logger.LogInformation("Analyzing {Count} input files...", inputFiles.Length);
            var totalDuration = TimeSpan.Zero;

            foreach (var file in inputFiles)
            {
                var media = new MediaFile { Path = file };
                var analyzed = await ffmpeg.AnalyzeMediaAsync(media).ConfigureAwait(false);
                totalDuration = totalDuration.Add(analyzed.Duration);

                logger.LogInformation("  {File}: {Duration}s, {Width}x{Height}, {Codec}",
                    Path.GetFileName(file),
                    analyzed.Duration.TotalSeconds,
                    analyzed.Width,
                    analyzed.Height,
                    analyzed.VideoCodec);
            }

            logger.LogInformation("Total duration: {Duration}s", totalDuration.TotalSeconds);

            // Create merge settings
            var settings = new MergeSettings
            {
                PreserveAudio = true,
                PreserveVideo = true,
                Crossfade = false
            };

            // Add input files to settings
            foreach (var file in inputFiles)
            {
                settings.AddInputFile(file);
            }

            logger.LogInformation("Merging {Count} videos into {Output}",
                settings.GetInputFileCount(), outputFile);

            // Create progress reporter
            var progress = new Progress<OperationStatistics>(stat =>
            {
                Console.Write($"\rProgress: {stat.Percentage:F1}% | " +
                    $"Elapsed: {stat.ElapsedTime.TotalSeconds:F0}s");
            });

            // Perform merge
            var result = await ffmpeg.MergeAsync(inputFiles, outputFile, settings, progress).ConfigureAwait(false);

            Console.WriteLine();  // New line after progress

            if (result.Success)
            {
                var outputInfo = new FileInfo(outputFile);
                logger.LogInformation("✓ Merge completed successfully");
                logger.LogInformation("Output: {Path}", outputFile);
                logger.LogInformation("Output size: {Size} MB", outputInfo.Length / 1_000_000.0);
                logger.LogInformation("Duration: {Duration}s", result.ElapsedTime.TotalSeconds);
            }
            else
            {
                logger.LogError("✗ Merge failed: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during video merging");
        }
    }
}
