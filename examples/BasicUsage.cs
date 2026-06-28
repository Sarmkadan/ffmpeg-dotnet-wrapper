// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
// Demonstrates minimal setup and basic usage of the FFmpeg .NET Wrapper
// Shows the simplest way to transcode a video file
// =============================================================================

using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Examples;

/// <summary>
/// Basic usage example - minimal setup and first call to FFmpeg wrapper.
/// This example shows the simplest possible way to use the library.
/// </summary>
public class BasicUsage
{
    public static async Task Main(string[] args)
    {
        // Step 1: Setup dependency injection container
        // This is the minimal required setup to use the FFmpeg wrapper
        var services = new ServiceCollection();

        // Add logging (console output)
        services.AddLogging(builder => builder.AddConsole());

        // Register FFmpeg wrapper with default configuration
        // The wrapper will automatically detect FFmpeg in your PATH
        services.AddFFmpegWrapper();

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        // Step 2: Get the FFmpeg service
        // This is the main entry point for all FFmpeg operations
        var ffmpegService = serviceProvider.GetRequiredService<IFFmpegService>();

        // Verify FFmpeg is available
        var isAvailable = await ffmpegService.IsFFmpegAvailableAsync();
        if (!isAvailable)
        {
            Console.WriteLine("ERROR: FFmpeg is not installed or not in PATH");
            Console.WriteLine("Please install FFmpeg and ensure it's available in your system PATH");
            return;
        }

        Console.WriteLine("✓ FFmpeg is available");

        // Step 3: Perform a simple transcode operation
        // This converts input.mp4 to output.webm using VP9 codec
        var inputFile = "input.mp4";
        var outputFile = "output.webm";

        Console.WriteLine($"Transcoding {inputFile} -> {outputFile}");

        try
        {
            // Create transcode settings
            var transcodeSettings = new TranscodeSettings
            {
                VideoCodec = VideoCodec.VP9,      // Use VP9 codec for modern web format
                AudioCodec = AudioCodec.OPUS,     // Use Opus audio codec
                Container = ContainerFormat.WebM,  // Output container format
                VideoBitrate = 2500,             // 2.5 Mbps video bitrate
                AudioBitrate = 128,              // 128 kbps audio bitrate
                Quality = QualityPreset.Medium     // Balance between speed and quality
            };

            // Execute the transcode operation
            var result = await ffmpegService.TranscodeAsync(
                inputFile,
                outputFile,
                transcodeSettings
            );

            // Check if operation succeeded
            if (result.IsSuccess)
            {
                Console.WriteLine("✓ Transcode completed successfully!");
                Console.WriteLine($"Output file: {outputFile}");
                Console.WriteLine($"Duration: {result.Duration.TotalSeconds:F1}s");

                // If the output file was analyzed, show some info
                if (result.OutputMedia != null)
                {
                    Console.WriteLine($"Output size: {result.OutputMedia.FileSize / (1024 * 1024):F2} MB");
                    Console.WriteLine($"Video codec: {result.OutputMedia.VideoCodec}");
                    Console.WriteLine($"Audio codec: {result.OutputMedia.AudioCodec}");
                }
            }
            else
            {
                Console.WriteLine("✗ Transcode failed");
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error during transcode: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}