// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
// Demonstrates advanced usage of the FFmpeg .NET Wrapper
// Shows configuration options, custom settings, error handling, and progress monitoring
// =============================================================================

using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Events;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Examples;

/// <summary>
/// Advanced usage example - configuration, custom options, and error handling.
/// This example demonstrates more sophisticated usage patterns of the library.
/// </summary>
public class AdvancedUsage
{
    public static async Task Main(string[] args)
    {
        // Step 1: Setup dependency injection with custom configuration
        var services = new ServiceCollection();

        // Add logging with debug level for detailed output
        services.AddLogging(builder =>
        {
            builder.AddConsole()
                   .SetMinimumLevel(LogLevel.Debug);
        });

        // Configure FFmpeg wrapper with custom options
        services.AddFFmpegWrapper(options =>
        {
            options.DefaultTimeout = TimeSpan.FromMinutes(30);  // 30 minute timeout
            options.EnableDetailedLogging = true;               // Enable FFmpeg debug output
            options.EnableOperationCaching = true;              // Cache operation results
            options.MaxCachedOperations = 100;                  // Limit cache size
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<AdvancedUsage>>();
        var ffmpegService = serviceProvider.GetRequiredService<IFFmpegService>();

        try
        {
            logger.LogInformation("Advanced FFmpeg Wrapper Example Starting");

            // Verify FFmpeg availability
            var isAvailable = await ffmpegService.IsFFmpegAvailableAsync();
            if (!isAvailable)
            {
                logger.LogError("FFmpeg is not available. Please install FFmpeg and add to PATH.");
                return;
            }

            var version = await ffmpegService.GetFFmpegVersionAsync();
            logger.LogInformation("FFmpeg Version: {Version}", version);

            // Example 1: Custom transcode with specific settings
            await RunCustomTranscodeExample(ffmpegService, logger);

            // Example 2: Error handling demonstration
            await RunErrorHandlingExample(ffmpegService, logger);

            // Example 3: Progress monitoring (if supported by operation)
            await RunProgressMonitoringExample(ffmpegService, logger);

            logger.LogInformation("Advanced example completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in advanced example");
        }
    }

    private static async Task RunCustomTranscodeExample(
        IFFmpegService ffmpegService,
        ILogger logger)
    {
        logger.LogInformation("\n=== Custom Transcode Example ===");

        var inputFile = "sample_input.mp4";
        var outputFile = "sample_output_custom.mp4";

        // Check if input file exists (for demo purposes)
        if (!File.Exists(inputFile))
        {
            logger.LogWarning("Input file {InputFile} not found. Creating dummy file for demo.", inputFile);
            // In a real scenario, you would have actual media files
            // For demo, we'll skip actual processing and just show configuration
            logger.LogInformation("Skipping actual transcode - no input file available");
            return;
        }

        // Create custom transcode settings
        var customSettings = new TranscodeSettings
        {
            // Video settings
            VideoCodec = VideoCodec.H264,
            VideoBitrate = 4500,           // 4.5 Mbps
            FrameRate = 30,
            Quality = QualityPreset.High,

            // Resolution constraints
            Width = 1920,
            Height = 1080,
            PreserveAspectRatio = true,

            // Audio settings
            AudioCodec = AudioCodec.AAC,
            AudioBitrate = 192,            // 192 kbps

            // Container and format
            Container = ContainerFormat.MP4,

            // Advanced options
            EnableTwoPass = true,
            CustomFFmpegArgs = "-movflags +faststart"  // Optimize for web streaming
        };

        logger.LogInformation("Starting custom transcode with settings:");
        logger.LogInformation("  Video: {Codec} @ {Bitrate}kbps, {Width}x{Height}@{FPS}fps",
            customSettings.VideoCodec, customSettings.VideoBitrate,
            customSettings.Width, customSettings.Height, customSettings.FrameRate);
        logger.LogInformation("  Audio: {Codec} @ {Bitrate}kbps",
            customSettings.AudioCodec, customSettings.AudioBitrate);
        logger.LogInformation("  Container: {Container}", customSettings.Container);
        logger.LogInformation("  Quality: {Quality}", customSettings.Quality);
        logger.LogInformation("  Custom Args: {Args}", customSettings.CustomFFmpegArgs);

        try
        {
            var result = await ffmpegService.TranscodeAsync(
                inputFile,
                outputFile,
                customSettings);

            if (result.IsSuccess)
            {
                logger.LogInformation("✓ Custom transcode completed successfully");
                logger.LogInformation("  Duration: {Duration}s", result.Duration.TotalSeconds);
                if (result.OutputMedia != null)
                {
                    logger.LogInformation("  Output: {Size} MB ({Codec}/{Codec})",
                        result.OutputMedia.FileSize / (1024.0 * 1024),
                        result.OutputMedia.VideoCodec,
                        result.OutputMedia.AudioCodec);
                }
            }
            else
            {
                logger.LogError("✗ Custom transcode failed: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during custom transcode operation");
        }
    }

    private static async Task RunErrorHandlingExample(
        IFFmpegService ffmpegService,
        ILogger logger)
    {
        logger.LogInformation("\n=== Error Handling Example ===");

        // Example: Try to process a non-existent file
        var nonExistentFile = "this_file_does_not_exist.mp4";
        var outputFile = "output.mp4";

        logger.LogInformation("Testing error handling with non-existent file: {File}", nonExistentFile);

        try
        {
            var settings = new TranscodeSettings
            {
                VideoCodec = VideoCodec.H264,
                Container = ContainerFormat.MP4
            };

            var result = await ffmpegService.TranscodeAsync(
                nonExistentFile,
                outputFile,
                settings);

            // This shouldn't be reached due to exception
            if (result.IsSuccess)
            {
                logger.LogInformation("Unexpected success");
            }
            else
            {
                logger.LogWarning("Operation failed as expected: {Error}", result.ErrorMessage);
            }
        }
        catch (FileNotFoundException ex)
        {
            logger.LogWarning(ex, "Expected file not found error: {Message}", ex.Message);
        }
        catch (FFmpegDotnetWrapper.Exceptions.InvalidMediaFileException ex)
        {
            logger.LogWarning(ex, "Invalid media file error: {Message}", ex.Message);
        }
        catch (FFmpegDotnetWrapper.Exceptions.FFmpegProcessException ex)
        {
            logger.LogError(ex, "FFmpeg process error: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
        }

        // Example: Invalid settings validation
        logger.LogInformation("\nTesting settings validation...");
        try
        {
            var invalidSettings = new TranscodeSettings
            {
                VideoBitrate = -1000,  // Invalid negative bitrate
                FrameRate = 0          // Invalid frame rate
            };

            // This will throw during validation
            invalidSettings.Validate(); // Manual validation call
        }
        catch (FFmpegDotnetWrapper.Exceptions.FFmpegValidationException ex)
        {
            logger.LogWarning(ex, "Settings validation failed as expected: {Message}", ex.Message);
        }
    }

    private static async Task RunProgressMonitoringExample(
        IFFmpegService ffmpegService,
        ILogger logger)
    {
        logger.LogInformation("\n=== Progress Monitoring Example ===");

        // Note: For actual progress monitoring, you would typically:
        // 1. Subscribe to FFmpeg events
        // 2. Parse FFmpeg output for progress information
        // 3. Use the IOperationRepository to track operation progress

        logger.LogInformation("Progress monitoring can be implemented by:");
        logger.LogInformation("  - Subscribing to FFmpegOperation events");
        logger.LogInformation("  - Parsing FFmpeg stderr output for progress info");
        logger.LogInformation("  - Using IOperationRepository to track long-running operations");
        logger.LogInformation("  - Implementing progress reporting via IProgress<T> or events");

        // Example of subscribing to operation events (conceptual)
        // In a real application, you would register event handlers
        // to receive progress updates during long operations
    }
}