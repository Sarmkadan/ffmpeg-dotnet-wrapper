// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================
// Demonstrates how to integrate FFmpeg .NET Wrapper into ASP.NET Core applications
// Shows dependency injection setup, service registration, and usage in controllers
// ===================================================================

using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Integration;

/// <summary>
/// Integration example - ASP.NET Core dependency injection setup and usage.
/// This example shows how to integrate the FFmpeg wrapper into a web application.
/// </summary>
public class IntegrationExample
{
    public static void Main(string[] args)
    {
        // Create and configure the web host
        var builder = WebApplication.CreateBuilder(args);

        // Configure services
        ConfigureServices(builder.Services);

        // Build the application
        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Map minimal API endpoints for video processing
        MapEndpoints(app);

        app.Run("http://localhost:5000");
    }

    /// <summary>
    /// Configures services including FFmpeg wrapper in the DI container.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // Add controllers (if using MVC)
        services.AddControllers();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // Configure FFmpeg wrapper with application-specific settings
        services.AddFFmpegWrapper(options =>
        {
            // Set appropriate timeout for web operations (10 minutes)
            options.DefaultTimeout = TimeSpan.FromMinutes(10);

            // Enable detailed logging for debugging
            options.EnableDetailedLogging = false;

            // Configure operation caching for better performance
            options.EnableOperationCaching = true;
            options.MaxCachedOperations = 1000;

            // Optionally specify custom FFmpeg paths
            // options.FFmpegPath = @"/usr/bin/ffmpeg";
            // options.FFprobePath = @"/usr/bin/ffprobe";
        });

        // Register additional services
        services.AddSingleton<VideoProcessingService>();
        services.AddSingleton<MediaAnalysisService>();

        // Add background services if needed
        // services.AddHostedService<FFmpegBackgroundService>();
    }

    /// <summary>
    /// Maps HTTP endpoints for video processing operations.
    /// </summary>
    private static void MapEndpoints(WebApplication app)
    {
        // Health check endpoint
        app.MapGet("/health", async context =>
        {
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegService>();
            var isAvailable = await ffmpegService.IsFFmpegAvailableAsync();

            context.Response.ContentType = "application/json";

            if (isAvailable)
            {
                await context.Response.WriteAsync("{\"status\":\"healthy\",\"ffmpeg\":\"available\"}");
            }
            else
            {
                await context.Response.WriteAsync("{\"status\":\"unhealthy\",\"ffmpeg\":\"not_available\"}");
            }
        });

        // Simple transcode endpoint
        app.MapPost("/api/transcode", async (
            HttpContext context,
            [FromBody] TranscodeRequest request) =>
        {
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegService>();
            var logger = context.RequestServices.GetRequiredService<ILogger<IntegrationExample>>();

            logger.LogInformation("Processing transcode request for {InputFile}", request.InputFile);

            try
            {
                var settings = new TranscodeSettings
                {
                    VideoCodec = request.VideoCodec,
                    AudioCodec = request.AudioCodec,
                    Container = request.Container,
                    VideoBitrate = request.VideoBitrate,
                    AudioBitrate = request.AudioBitrate,
                    Quality = request.Quality
                };

                var result = await ffmpegService.TranscodeAsync(
                    request.InputFile,
                    request.OutputFile,
                    settings);

                if (result.IsSuccess)
                {
                    logger.LogInformation("Transcode successful: {OutputFile}", request.OutputFile);
                    return Results.Ok(new
                    {
                        success = true,
                        outputFile = request.OutputFile,
                        duration = result.Duration.TotalSeconds,
                        size = result.OutputMedia?.FileSize,
                        exitCode = result.ExitCode
                    });
                }

                logger.LogError("Transcode failed with exit code {ExitCode}: {Error}", result.ExitCode, result.ErrorMessage);
                return Results.BadRequest(new
                {
                    success = false,
                    exitCode = result.ExitCode,
                    error = result.ErrorMessage,
                    errorOutput = result.ErrorOutput
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing transcode request");
                return Results.BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        });

        // Video trimming endpoint
        app.MapPost("/api/trim", async (
            HttpContext context,
            [FromBody] TrimRequest request) =>
        {
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegService>();
            var logger = context.RequestServices.GetRequiredService<ILogger<IntegrationExample>>();

            logger.LogInformation("Processing trim request: {InputFile} from {Start}s",
                request.InputFile, request.StartSeconds);

            try
            {
                var settings = new TrimSettings
                {
                    StartTime = TimeSpan.FromSeconds(request.StartSeconds),
                    Duration = TimeSpan.FromSeconds(request.DurationSeconds),
                    PreserveAudio = request.PreserveAudio,
                    PreserveVideo = request.PreserveVideo,
                    Keyframe = request.Keyframe
                };

                var result = await ffmpegService.TrimAsync(
                    request.InputFile,
                    request.OutputFile,
                    settings);

                if (result.IsSuccess)
                {
                    return Results.Ok(new
                    {
                        success = true,
                        outputFile = request.OutputFile,
                        duration = result.Duration.TotalSeconds
                    });
                }
                else
                {
                    return Results.BadRequest(new
                    {
                        success = false,
                        error = result.ErrorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        });

        // Media analysis endpoint
        app.MapGet("/api/media/{*filePath}", async (
            HttpContext context,
            string filePath) =>
        {
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegService>();
            var logger = context.RequestServices.GetRequiredService<ILogger<IntegrationExample>>();

            logger.LogInformation("Analyzing media file: {FilePath}", filePath);

            try
            {
                var mediaFile = await ffmpegService.AnalyzeMediaAsync(filePath);

                return Results.Ok(new
                {
                    success = true,
                    fileName = Path.GetFileName(filePath),
                    size = mediaFile.FileSize,
                    duration = mediaFile.Duration.TotalSeconds,
                    resolution = $"{mediaFile.Width}x{mediaFile.Height}",
                    videoCodec = mediaFile.VideoCodec,
                    audioCodec = mediaFile.AudioCodec,
                    frameRate = mediaFile.FrameRate,
                    bitRate = mediaFile.BitRate
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Media analysis failed");
                return Results.BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        });
    }
}

/// <summary>
/// Example request DTO for transcode operations.
/// </summary>
public class TranscodeRequest
{
    public string InputFile { get; set; } = string.Empty;
    public string OutputFile { get; set; } = string.Empty;
    public VideoCodec VideoCodec { get; set; } = VideoCodec.H264;
    public AudioCodec AudioCodec { get; set; } = AudioCodec.AAC;
    public ContainerFormat Container { get; set; } = ContainerFormat.MP4;
    public int VideoBitrate { get; set; } = 2500;
    public int AudioBitrate { get; set; } = 128;
    public QualityPreset Quality { get; set; } = QualityPreset.Medium;
}

/// <summary>
/// Example request DTO for trim operations.
/// </summary>
public class TrimRequest
{
    public string InputFile { get; set; } = string.Empty;
    public string OutputFile { get; set; } = string.Empty;
    public double StartSeconds { get; set; }
    public double DurationSeconds { get; set; } = 30;
    public bool PreserveAudio { get; set; } = true;
    public bool PreserveVideo { get; set; } = true;
    public bool Keyframe { get; set; } = true;
}

/// <summary>
/// Example video processing service that uses the FFmpeg wrapper.
/// This demonstrates how to create application-specific services that wrap the library.
/// </summary>
public class VideoProcessingService
{
    private readonly IFFmpegService _ffmpegService;
    private readonly ILogger<VideoProcessingService> _logger;

    public VideoProcessingService(
        IFFmpegService ffmpegService,
        ILogger<VideoProcessingService> logger)
    {
        _ffmpegService = ffmpegService;
        _logger = logger;
    }

    public async Task<ConversionResult> ConvertForWebOptimizationAsync(
        string inputFile,
        string outputFile)
    {
        _logger.LogInformation("Converting {Input} to web-optimized format", inputFile);

        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = 2500,
            AudioBitrate = 128,
            Quality = QualityPreset.High,
            Width = 1280,
            Height = 720,
            EnableTwoPass = true,
            CustomFFmpegArgs = "-movflags +faststart"
        };

        return await _ffmpegService.TranscodeAsync(inputFile, outputFile, settings);
    }

    public async Task<ConversionResult> CreateThumbnailAsync(
        string inputFile,
        string outputFile,
        TimeSpan? timestamp = null)
    {
        _logger.LogInformation("Creating thumbnail from {Input}", inputFile);

        var settings = new ThumbnailSettings
        {
            Times = timestamp.HasValue ? new List<TimeSpan> { timestamp.Value } : new List<TimeSpan>(),
            Width = 320,
            Height = 240,
            Format = ThumbnailFormat.Jpeg,
            JpegQuality = 85
        };

        var thumbnailService = new ThumbnailService();
        return await thumbnailService.ExtractThumbnailsAsync(
            new MediaFile(inputFile),
            outputFile,
            settings);
    }
}

/// <summary>
/// Example media analysis service.
/// </summary>
public class MediaAnalysisService
{
    private readonly IFFmpegService _ffmpegService;
    private readonly ILogger<MediaAnalysisService> _logger;

    public MediaAnalysisService(
        IFFmpegService ffmpegService,
        ILogger<MediaAnalysisService> logger)
    {
        _ffmpegService = ffmpegService;
        _logger = logger;
    }

    public async Task<MediaFile> GetMediaInfoAsync(string filePath)
    {
        _logger.LogInformation("Analyzing media file: {FilePath}", filePath);
        return await _ffmpegService.AnalyzeMediaAsync(filePath);
    }

    public async Task<bool> IsVideoAsync(string filePath)
    {
        try
        {
            var media = await _ffmpegService.AnalyzeMediaAsync(filePath);
            return media.Width > 0 && media.Height > 0;
        }
        catch
        {
            return false;
        }
    }
}