// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper;

/// <summary>
/// Example console application demonstrating the FFmpeg wrapper usage.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup dependency injection and logging
        var services = new ServiceCollection();

        services.AddLogging(builder =>
            builder.AddConsole()
                   .SetMinimumLevel(LogLevel.Information));

        // Register FFmpeg wrapper services
        services.AddFFmpegWrapper(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(600);
            options.EnableDetailedLogging = true;
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("FFmpeg Wrapper Example Application Starting");

            // Verify FFmpeg is available
            var ffmpegService = serviceProvider.GetRequiredService<IFFmpegService>();
            var isAvailable = await ffmpegService.IsFFmpegAvailableAsync();

            if (!isAvailable)
            {
                logger.LogError("FFmpeg is not installed or not available in PATH");
                return;
            }

            var version = await ffmpegService.GetFFmpegVersionAsync();
            logger.LogInformation("FFmpeg Version: {Version}", version);

            // Example: Create a sample media file reference
            logger.LogInformation("=== FFmpeg Wrapper Ready ===");
            logger.LogInformation("The wrapper provides the following capabilities:");
            logger.LogInformation("  - Transcode: Convert between video/audio formats");
            logger.LogInformation("  - Trim: Cut videos to specific time ranges");
            logger.LogInformation("  - Merge: Concatenate multiple media files");
            logger.LogInformation("  - Watermark: Add overlays to videos");
            logger.LogInformation("  - Batch Processing: Process multiple files concurrently");
            logger.LogInformation("  - Media Analysis: Extract file metadata and properties");

            await RunExamplesAsync(serviceProvider, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in application");
        }
    }

    /// <summary>
    /// Demonstrates various usage examples of the FFmpeg wrapper.
    /// </summary>
    private static async Task RunExamplesAsync(IServiceProvider serviceProvider, ILogger<Program> logger)
    {
        var ffmpegService = serviceProvider.GetRequiredService<IFFmpegService>();
        var transcodeService = serviceProvider.GetRequiredService<TranscodeService>();

        logger.LogInformation("\n=== Example: Transcode Configuration ===");

        // Create transcode settings example
        var transcodeSettings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = 2500,
            AudioBitrate = 128,
            FrameRate = 30,
            Quality = QualityPreset.Medium,
            EnableAutoScale = true,
            MaxWidth = 1280,
            MaxHeight = 720,
            PreserveAspectRatio = true
        };

        logger.LogInformation("Transcode Settings:");
        logger.LogInformation("  Video Codec: {Codec}", transcodeSettings.VideoCodec);
        logger.LogInformation("  Audio Codec: {Codec}", transcodeSettings.AudioCodec);
        logger.LogInformation("  Container: {Container}", transcodeSettings.Container);
        logger.LogInformation("  Video Bitrate: {Bitrate}k", transcodeSettings.VideoBitrate);
        logger.LogInformation("  Audio Bitrate: {Bitrate}k", transcodeSettings.AudioBitrate);
        logger.LogInformation("  Frame Rate: {FPS} fps", transcodeSettings.FrameRate);
        logger.LogInformation("  Quality Preset: {Quality}", transcodeSettings.Quality);

        logger.LogInformation("\n=== Example: Trim Configuration ===");

        // Create trim settings example
        var trimSettings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(30),
            PreserveAudio = true,
            PreserveVideo = true,
            Keyframe = true
        };

        logger.LogInformation("Trim Settings:");
        logger.LogInformation("  Start Time: {Start}s", trimSettings.StartTime.TotalSeconds);
        logger.LogInformation("  Duration: {Duration}s", trimSettings.Duration?.TotalSeconds);
        logger.LogInformation("  Preserve Audio: {Preserve}", trimSettings.PreserveAudio);
        logger.LogInformation("  Preserve Video: {Preserve}", trimSettings.PreserveVideo);

        logger.LogInformation("\n=== Example: Merge Configuration ===");

        // Create merge settings example
        var mergeSettings = new MergeSettings
        {
            PreserveAudio = true,
            PreserveVideo = true,
            Crossfade = false
        };

        mergeSettings.AddInputFile("input1.mp4");
        mergeSettings.AddInputFile("input2.mp4");
        mergeSettings.AddInputFile("input3.mp4");

        logger.LogInformation("Merge Settings:");
        logger.LogInformation("  Input Files: {Count}", mergeSettings.GetInputFileCount());
        logger.LogInformation("  Preserve Audio: {Preserve}", mergeSettings.PreserveAudio);
        logger.LogInformation("  Preserve Video: {Preserve}", mergeSettings.PreserveVideo);
        logger.LogInformation("  Crossfade: {Enabled}", mergeSettings.Crossfade);

        logger.LogInformation("\n=== Example: Watermark Configuration ===");

        // Create watermark settings example
        var watermarkSettings = new WatermarkSettings
        {
            Position = WatermarkPosition.BottomRight,
            XOffset = 10,
            YOffset = 10,
            Scale = 0.1,
            Opacity = 0.8,
            PreserveAspectRatio = true
        };

        logger.LogInformation("Watermark Settings:");
        logger.LogInformation("  Position: {Position}", watermarkSettings.Position);
        logger.LogInformation("  Offset: X={X}, Y={Y}", watermarkSettings.XOffset, watermarkSettings.YOffset);
        logger.LogInformation("  Scale: {Scale}%", watermarkSettings.Scale * 100);
        logger.LogInformation("  Opacity: {Opacity}", watermarkSettings.Opacity);

        logger.LogInformation("\n=== Configuration Complete ===");
        logger.LogInformation("The FFmpeg wrapper is ready for use with the following configured scenarios:");
        logger.LogInformation("  1. Web optimization transcode");
        logger.LogInformation("  2. Video trimming/cutting");
        logger.LogInformation("  3. Multiple file merging");
        logger.LogInformation("  4. Watermark overlays");
        logger.LogInformation("  5. Batch concurrent processing");
        logger.LogInformation("  6. Media file analysis");

        logger.LogInformation("\nNote: Actual file operations require valid input files and FFmpeg installation.");
    }
}
