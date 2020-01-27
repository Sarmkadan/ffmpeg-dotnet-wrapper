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
/// Example 2: Batch Processing Multiple Files
/// Demonstrates concurrent transcoding of multiple files with progress tracking.
/// Usage: dotnet run --project examples/02-batch-processing.csproj input-dir output-dir
/// </summary>
public class BatchProcessingExample
{
    public static async Task Main(string[] args)
    {
        var inputDir = args.Length > 0 ? args[0] : "./input";
        var outputDir = args.Length > 1 ? args[1] : "./output";

        // Verify directories
        if (!Directory.Exists(inputDir))
        {
            Console.WriteLine($"Error: Input directory not found: {inputDir}");
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
            options.MaxConcurrentOperations = 4;  // Process 4 files simultaneously
            options.EnableDetailedLogging = false;
        });

        var serviceProvider = services.BuildServiceProvider();
        var batchService = serviceProvider.GetRequiredService<BatchOperationService>();
        var logger = serviceProvider.GetRequiredService<ILogger<BatchProcessingExample>>();

        try
        {
            // Find all MP4 files in input directory
            var files = Directory.GetFiles(inputDir, "*.mp4");
            if (files.Length == 0)
            {
                logger.LogWarning("No MP4 files found in {Directory}", inputDir);
                return;
            }

            logger.LogInformation("Found {Count} files to process", files.Length);

            // Create transcode settings
            var settings = new TranscodeSettings
            {
                VideoCodec = VideoCodec.H264,      // Fast, compatible
                AudioCodec = AudioCodec.AAC,
                Container = ContainerFormat.MP4,
                VideoBitrate = 2500,
                AudioBitrate = 128,
                Quality = QualityPreset.High,
                EnableAutoScale = true,
                MaxWidth = 1280,
                MaxHeight = 720
            };

            // Create progress reporter
            var progress = new Progress<OperationStatistics>(stat =>
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║     Batch Processing Progress          ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.WriteLine($"Completed:      {stat.CompletedOperations}/{stat.TotalOperations} files");
                Console.WriteLine($"Successful:     {stat.SuccessfulOperations} files");
                Console.WriteLine($"Failed:         {stat.FailedOperations} files");
                Console.WriteLine($"Success Rate:   {stat.SuccessRate:P2}");
                Console.WriteLine($"Progress:       {stat.Percentage:F1}%");
                Console.WriteLine($"Elapsed Time:   {stat.ElapsedTime.TotalSeconds:F0}s");

                if (stat.EstimatedTimeRemaining.HasValue)
                    Console.WriteLine($"Estimated ETA:  {stat.EstimatedTimeRemaining.Value.TotalSeconds:F0}s");

                // Progress bar
                var barLength = 30;
                var filled = (int)(stat.Percentage / 100 * barLength);
                var bar = new string('█', filled) + new string('░', barLength - filled);
                Console.WriteLine($"[{bar}]");
            });

            // Start batch processing
            logger.LogInformation("Starting batch processing with {Concurrency} parallel workers",
                options: 4);

            var startTime = DateTime.UtcNow;
            await batchService.ProcessFilesAsync(files, outputDir, settings, progress);
            var duration = DateTime.UtcNow - startTime;

            // Summary
            Console.Clear();
            logger.LogInformation("╔════════════════════════════════════════╗");
            logger.LogInformation("║     Batch Processing Complete          ║");
            logger.LogInformation("╚════════════════════════════════════════╝");
            logger.LogInformation("Total files processed:  {Count}", files.Length);
            logger.LogInformation("Total time:             {Duration}m {Seconds}s",
                (int)duration.TotalMinutes, (int)duration.Seconds);
            logger.LogInformation("Output directory:       {Path}", outputDir);

            // List output files
            var outputs = Directory.GetFiles(outputDir);
            logger.LogInformation("Output files: {Count}", outputs.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during batch processing");
        }
    }
}
