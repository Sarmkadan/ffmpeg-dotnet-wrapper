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
/// Example 3: Video Trimming
/// Demonstrates how to extract segments from a video file.
/// Usage: dotnet run --project examples/03-video-trimming.csproj input-file start-seconds duration-seconds
/// Example: dotnet run examples/03-video-trimming input.mp4 10 60
/// </summary>
public class VideoTrimmingExample
{
    public static async Task Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: 03-video-trimming <input-file> <start-seconds> <duration-seconds>");
            Console.WriteLine("Example: 03-video-trimming video.mp4 10 60");
            Console.WriteLine("This extracts a 60-second segment starting at 10 seconds");
            return;
        }

        var inputFile = args[0];
        var startSeconds = double.Parse(args[1]);
        var durationSeconds = double.Parse(args[2]);
        var outputFile = Path.GetFileNameWithoutExtension(inputFile) + "_trimmed.mp4";

        // Verify input file
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: File not found: {inputFile}");
            return;
        }

        // Setup dependency injection
        var services = new ServiceCollection();

        services.AddLogging(builder =>
            builder.AddConsole()
                   .SetMinimumLevel(LogLevel.Information));

        services.AddFFmpegWrapper(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(300);
            options.EnableDetailedLogging = false;
        });

        var serviceProvider = services.BuildServiceProvider();
        var ffmpeg = serviceProvider.GetRequiredService<IFFmpegService>();
        var logger = serviceProvider.GetRequiredService<ILogger<VideoTrimmingExample>>();

        try
        {
            // First, analyze input file to get duration
            logger.LogInformation("Analyzing input file: {File}", inputFile);
            var media = new MediaFile { Path = inputFile };
            var analyzed = await ffmpeg.AnalyzeMediaAsync(media).ConfigureAwait(false);

            logger.LogInformation("File duration: {Duration}", analyzed.Duration);
            logger.LogInformation("Resolution: {Width}x{Height}", analyzed.Width, analyzed.Height);
            logger.LogInformation("Video codec: {Codec}", analyzed.VideoCodec);

            // Validate trim parameters
            var startTime = TimeSpan.FromSeconds(startSeconds);
            var duration = TimeSpan.FromSeconds(durationSeconds);

            if (startTime >= analyzed.Duration)
            {
                logger.LogError("Start time ({Start}s) exceeds file duration ({Duration}s)",
                    startSeconds, analyzed.Duration.TotalSeconds);
                return;
            }

            if (startTime.Add(duration) > analyzed.Duration)
            {
                logger.LogWarning(
                    "Trim end time exceeds file duration, will trim to end of file");
                duration = analyzed.Duration - startTime;
            }

            // Create trim settings
            var settings = new TrimSettings
            {
                StartTime = startTime,
                Duration = duration,
                PreserveAudio = true,
                PreserveVideo = true,
                Keyframe = true  // Start at nearest keyframe for reliability
            };

            logger.LogInformation("Trimming segment from {Start}s to {End}s (duration: {Duration}s)",
                startSeconds, startSeconds + duration.TotalSeconds, duration.TotalSeconds);

            // Perform trim
            var result = await ffmpeg.TrimAsync(inputFile, outputFile, settings).ConfigureAwait(false);

            if (result.Success)
            {
                var outputInfo = new FileInfo(outputFile);
                logger.LogInformation("✓ Trimming completed successfully");
                logger.LogInformation("Output: {Path} ({Size} MB)",
                    outputFile, outputInfo.Length / 1_000_000.0);
                logger.LogInformation("Duration: {Duration}s", result.ElapsedTime.TotalSeconds);
            }
            else
            {
                logger.LogError("✗ Trimming failed: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during video trimming");
        }
    }
}
