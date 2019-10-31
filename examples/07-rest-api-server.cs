// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Examples;

/// <summary>
/// Example 7: REST API Server
/// Demonstrates running the wrapper as a REST API service.
/// Usage: dotnet run --project examples/07-rest-api-server.csproj
/// Then call: curl -X POST http://localhost:5000/api/transcode ...
/// </summary>
public class RestApiServerExample
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add logging
        builder.Logging.AddConsole()
            .SetMinimumLevel(LogLevel.Information);

        // Add FFmpeg wrapper services
        builder.Services.AddFFmpegWrapper(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(600);
            options.EnableDetailedLogging = false;
            options.MaxConcurrentOperations = 4;
        });

        // Add Kestrel server configuration
        builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(
            options => options.AllowSynchronousIO = true);

        var app = builder.Build();

        // Health check endpoint
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("Health")
            .WithOpenApi();

        // Get FFmpeg version
        app.MapGet("/api/info", async (IFFmpegService ffmpeg) =>
        {
            var available = await ffmpeg.IsFFmpegAvailableAsync();
            if (!available)
                return Results.ServiceUnavailable();

            var version = await ffmpeg.GetFFmpegVersionAsync();
            return Results.Ok(new { available = true, version });
        })
        .WithName("FFmpeg Info")
        .WithOpenApi();

        // Analyze media file
        app.MapPost("/api/analyze", async (AnalyzeRequest request, IFFmpegService ffmpeg) =>
        {
            if (!File.Exists(request.FilePath))
                return Results.BadRequest(new { error = "File not found" });

            try
            {
                var media = new MediaFile { Path = request.FilePath };
                var analyzed = await ffmpeg.AnalyzeMediaAsync(media);

                return Results.Ok(new
                {
                    file = request.FilePath,
                    duration = analyzed.Duration.TotalSeconds,
                    width = analyzed.Width,
                    height = analyzed.Height,
                    videoCodec = analyzed.VideoCodec,
                    audioCodec = analyzed.AudioCodec,
                    bitrate = analyzed.Bitrate,
                    frameRate = analyzed.FrameRate
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("Analyze Media")
        .WithOpenApi();

        // Transcode endpoint
        app.MapPost("/api/transcode", async (TranscodeRequest request, IFFmpegService ffmpeg) =>
        {
            if (!File.Exists(request.InputPath))
                return Results.BadRequest(new { error = "Input file not found" });

            try
            {
                var settings = new TranscodeSettings
                {
                    VideoCodec = Enum.Parse<VideoCodec>(request.VideoCodec),
                    AudioCodec = Enum.Parse<AudioCodec>(request.AudioCodec),
                    Container = Enum.Parse<ContainerFormat>(request.Container),
                    VideoBitrate = request.VideoBitrate ?? 2500,
                    AudioBitrate = request.AudioBitrate ?? 128,
                    FrameRate = request.FrameRate ?? 30,
                    Quality = Enum.Parse<QualityPreset>(request.Quality ?? "Medium"),
                    EnableAutoScale = request.EnableAutoScale ?? false,
                    MaxWidth = request.MaxWidth ?? 1920,
                    MaxHeight = request.MaxHeight ?? 1080,
                    PreserveAspectRatio = request.PreserveAspectRatio ?? true
                };

                var result = await ffmpeg.TranscodeAsync(
                    request.InputPath,
                    request.OutputPath,
                    settings);

                if (!result.Success)
                    return Results.BadRequest(new { error = result.ErrorMessage });

                return Results.Ok(new
                {
                    success = true,
                    outputPath = result.OutputPath,
                    elapsedSeconds = result.ElapsedTime.TotalSeconds,
                    fileSize = new FileInfo(result.OutputPath).Length
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("Transcode Video")
        .WithOpenApi();

        // Trim endpoint
        app.MapPost("/api/trim", async (TrimRequest request, IFFmpegService ffmpeg) =>
        {
            if (!File.Exists(request.InputPath))
                return Results.BadRequest(new { error = "Input file not found" });

            try
            {
                var settings = new TrimSettings
                {
                    StartTime = TimeSpan.FromSeconds(request.StartSeconds),
                    Duration = request.DurationSeconds.HasValue
                        ? TimeSpan.FromSeconds(request.DurationSeconds.Value)
                        : null,
                    PreserveAudio = request.PreserveAudio ?? true,
                    PreserveVideo = request.PreserveVideo ?? true,
                    Keyframe = request.Keyframe ?? false
                };

                var result = await ffmpeg.TrimAsync(
                    request.InputPath,
                    request.OutputPath,
                    settings);

                if (!result.Success)
                    return Results.BadRequest(new { error = result.ErrorMessage });

                return Results.Ok(new
                {
                    success = true,
                    outputPath = result.OutputPath,
                    elapsedSeconds = result.ElapsedTime.TotalSeconds
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("Trim Video")
        .WithOpenApi();

        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║   FFmpeg .NET REST API Server          ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine("Starting server on http://localhost:5000");
        Console.WriteLine();
        Console.WriteLine("Available endpoints:");
        Console.WriteLine("  GET  /health               - Health check");
        Console.WriteLine("  GET  /api/info             - FFmpeg info");
        Console.WriteLine("  POST /api/analyze          - Analyze media file");
        Console.WriteLine("  POST /api/transcode        - Transcode video");
        Console.WriteLine("  POST /api/trim             - Trim video");
        Console.WriteLine();

        await app.RunAsync();
    }
}

// Request/Response DTOs
public record AnalyzeRequest(string FilePath);

public record TranscodeRequest(
    string InputPath,
    string OutputPath,
    string VideoCodec = "H264",
    string AudioCodec = "AAC",
    string Container = "MP4",
    int? VideoBitrate = null,
    int? AudioBitrate = null,
    int? FrameRate = null,
    string? Quality = null,
    bool? EnableAutoScale = null,
    int? MaxWidth = null,
    int? MaxHeight = null,
    bool? PreserveAspectRatio = null);

public record TrimRequest(
    string InputPath,
    string OutputPath,
    double StartSeconds,
    double? DurationSeconds = null,
    bool? PreserveAudio = null,
    bool? PreserveVideo = null,
    bool? Keyframe = null);
