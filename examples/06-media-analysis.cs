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
/// Example 6: Media File Analysis
/// Demonstrates how to extract metadata from video files.
/// Usage: dotnet run --project examples/06-media-analysis.csproj [file1] [file2] ...
/// Example: dotnet run examples/06-media-analysis video1.mp4 video2.mkv video3.webm
/// </summary>
public class MediaAnalysisExample
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: 06-media-analysis <file1> [file2] [file3]...");
            Console.WriteLine("Example: 06-media-analysis video.mp4");
            return;
        }

        var files = args;

        // Verify files exist
        foreach (var file in files)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"Error: File not found: {file}");
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
            options.DefaultTimeout = TimeSpan.FromSeconds(30);
            options.EnableDetailedLogging = false;
        });

        var serviceProvider = services.BuildServiceProvider();
        var ffmpeg = serviceProvider.GetRequiredService<IFFmpegService>();

        try
        {
            // Check FFmpeg availability
            var available = await ffmpeg.IsFFmpegAvailableAsync().ConfigureAwait(false);
            if (!available)
            {
                Console.WriteLine("Error: FFmpeg is not installed");
                return;
            }

            // Get FFmpeg version
            var version = await ffmpeg.GetFFmpegVersionAsync().ConfigureAwait(false);
            Console.WriteLine($"FFmpeg: {version}\n");

            // Analyze each file
            foreach (var file in files)
            {
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine($"║  {Path.GetFileName(file),36}  ║");
                Console.WriteLine("╚════════════════════════════════════════╝");

                var media = new MediaFile { Path = file };
                var analyzed = await ffmpeg.AnalyzeMediaAsync(media).ConfigureAwait(false);

                // Display file information
                Console.WriteLine($"File:               {file}");
                var fileInfo = new FileInfo(file);
                Console.WriteLine($"Size:               {fileInfo.Length / 1_000_000.0:F2} MB");

                // Display timing
                Console.WriteLine($"\nTiming:");
                Console.WriteLine($"  Duration:         {analyzed.Duration}");
                Console.WriteLine($"  Duration (sec):   {analyzed.Duration.TotalSeconds:F2}s");

                // Display video properties
                if (analyzed.Width > 0 && analyzed.Height > 0)
                {
                    Console.WriteLine($"\nVideo:");
                    Console.WriteLine($"  Codec:            {analyzed.VideoCodec}");
                    Console.WriteLine($"  Resolution:       {analyzed.Width}x{analyzed.Height}");
                    Console.WriteLine($"  Aspect Ratio:     {(double)analyzed.Width / analyzed.Height:F2}:1");
                    Console.WriteLine($"  Frame Rate:       {analyzed.FrameRate} fps");
                }

                // Display audio properties
                if (!string.IsNullOrEmpty(analyzed.AudioCodec))
                {
                    Console.WriteLine($"\nAudio:");
                    Console.WriteLine($"  Codec:            {analyzed.AudioCodec}");
                    Console.WriteLine($"  Sample Rate:      {analyzed.SampleRate} Hz");
                    Console.WriteLine($"  Channels:         {analyzed.AudioChannels}");
                }

                // Display bitrate
                if (analyzed.Bitrate > 0)
                {
                    Console.WriteLine($"\nBitrate:");
                    Console.WriteLine($"  Total:            {analyzed.Bitrate / 1000} kbps");
                    Console.WriteLine($"  Total:            {analyzed.Bitrate / 1_000_000.0:F2} Mbps");
                }

                // Calculate file efficiency
                var megabytes = fileInfo.Length / 1_000_000.0;
                var seconds = analyzed.Duration.TotalSeconds;
                var bitrateActual = (megabytes * 8) / seconds;
                Console.WriteLine($"\nStatistics:");
                Console.WriteLine($"  File bitrate:     {bitrateActual:F2} Mbps");
                Console.WriteLine($"  Compression:      {(bitrateActual / (analyzed.Bitrate / 1_000_000.0)):P1}");

                Console.WriteLine();
            }

            // Summary
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║         Analysis Complete              ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine($"Files analyzed: {files.Length}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
