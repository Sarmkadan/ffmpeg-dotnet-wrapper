using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Benchmarks;

/// <summary>
/// Performance benchmarks for FFmpegService operations.
/// Measures throughput of main operations and memory allocations.
/// </summary>
[MemoryDiagnoser(displayGenColumns: true)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
public class FFmpegServiceBenchmarks
{
    private IFFmpegService? _ffmpegService;
    private MediaFile? _sampleMedia;
    private string? _outputPath;
    private string? _tempDir;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning));
        services.AddFFmpegWrapper(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(30);
        });

        var serviceProvider = services.BuildServiceProvider();
        _ffmpegService = serviceProvider.GetRequiredService<IFFmpegService>();

        // Load sample media file
        _sampleMedia = _ffmpegService.AnalyzeMediaAsync(
            "../../benchmarks/SampleMedia/sample_1280x720_30fps_10s.mp4"
        ).GetAwaiter().GetResult();

        // Create temp directory for outputs
        _tempDir = Path.Combine(Path.GetTempPath(), "ffmpeg-benchmarks-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        // Cleanup temp files
        if (Directory.Exists(_tempDir))
        {
            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    [Benchmark]
    public async Task Transcode_H264_to_H265_MP4()
    {
        _outputPath = Path.Combine(_tempDir!, "transcode_h265_output.mp4");
        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H265,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = 2000,
            Quality = QualityPreset.Medium,
            HardwareAcceleration = HwAccel.None
        };

        var result = await _ffmpegService!.TranscodeAsync(_sampleMedia!, _outputPath, settings);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("Transcode failed: " + result.ErrorMessage);
        }
    }

    [Benchmark]
    public async Task Transcode_H264_to_VP9_WebM()
    {
        _outputPath = Path.Combine(_tempDir!, "transcode_vp9_output.webm");
        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.VP9,
            AudioCodec = AudioCodec.OPUS,
            Container = ContainerFormat.WebM,
            VideoBitrate = 1500,
            Quality = QualityPreset.Medium,
            HardwareAcceleration = HwAccel.None
        };

        var result = await _ffmpegService!.TranscodeAsync(_sampleMedia!, _outputPath, settings);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("Transcode failed: " + result.ErrorMessage);
        }
    }

    [Benchmark]
    public async Task Transcode_With_Hardware_Acceleration()
    {
        _outputPath = Path.Combine(_tempDir!, "transcode_hwaccel_output.mp4");
        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = 2000,
            Quality = QualityPreset.Fast,
            HardwareAcceleration = HwAccel.Auto
        };

        var result = await _ffmpegService!.TranscodeAsync(_sampleMedia!, _outputPath, settings);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("Hardware accelerated transcode failed: " + result.ErrorMessage);
        }
    }

    [Benchmark]
    public async Task Trim_Video_StreamCopy()
    {
        _outputPath = Path.Combine(_tempDir!, "trim_streamcopy_output.mp4");
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(2),
            EndTime = TimeSpan.FromSeconds(8)
        };

        var result = await _ffmpegService!.TrimAsync(_sampleMedia!, _outputPath, settings);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("Trim failed: " + result.ErrorMessage);
        }
    }

    [Benchmark]
    public async Task Analyze_Media_Metadata()
    {
        var mediaFile = await _ffmpegService!.AnalyzeMediaAsync(
            "../../benchmarks/SampleMedia/sample_1280x720_30fps_10s.mp4"
        );

        if (!mediaFile.Duration.HasValue || mediaFile.Duration.Value.TotalSeconds < 1)
        {
            throw new InvalidOperationException("Media analysis returned invalid duration");
        }
    }

    [Benchmark]
    public async Task Extract_Thumbnails()
    {
        var outputPattern = Path.Combine(_tempDir!, "thumbnail_%03d.jpg");
        var settings = new ThumbnailSettings
        {
            Times = [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(9)],
            Width = 320,
            Height = 240,
            Format = ThumbnailFormat.Jpeg
        };

        var result = await _ffmpegService!.ExtractThumbnailsAsync(_sampleMedia!, outputPattern, settings);
        if (result.Thumbnails.Count != 3)
        {
            throw new InvalidOperationException("Thumbnail extraction failed");
        }
    }

    [Benchmark]
    public async Task Merge_Multiple_Videos()
    {
        _outputPath = Path.Combine(_tempDir!, "merge_output.mp4");
        var inputFiles = new List<string>
        {
            _sampleMedia!.FilePath,
            _sampleMedia!.FilePath,
            _sampleMedia!.FilePath
        };

        var settings = new MergeSettings
        {
            TranscodeOnMerge = true,
            TranscodeSettings = new TranscodeSettings
            {
                VideoCodec = VideoCodec.H264,
                AudioCodec = AudioCodec.AAC,
                Container = ContainerFormat.MP4
            }
        };

        var result = await _ffmpegService!.MergeAsync(inputFiles, _outputPath, settings);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("Merge failed: " + result.ErrorMessage);
        }
    }

    [Benchmark]
    public async Task Extract_Audio_Only()
    {
        _outputPath = Path.Combine(_tempDir!, "audio_extract_output.mp3");

        var result = await _ffmpegService!.ExtractAudioAsync(_sampleMedia!, _outputPath, AudioCodec.MP3, 192);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("Audio extraction failed: " + result.ErrorMessage);
        }
    }

    [Benchmark]
    public async Task Add_Watermark()
    {
        _outputPath = Path.Combine(_tempDir!, "watermark_output.mp4");
        var settings = new WatermarkSettings
        {
            WatermarkPath = "../../benchmarks/SampleMedia/watermark.png",
            Position = WatermarkPosition.BottomRight,
            Opacity = 0.5,
            Scale = 0.2
        };

        var result = await _ffmpegService!.AddWatermarkAsync(_sampleMedia!, _outputPath, settings);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("Watermark addition failed: " + result.ErrorMessage);
        }
    }

    [Benchmark]
    public async Task Batch_Transcode_Multiple_Files()
    {
        var inputFiles = new List<MediaFile>();
        for (int i = 0; i < 3; i++)
        {
            var mediaFile = await _ffmpegService!.AnalyzeMediaAsync(
                "../../benchmarks/SampleMedia/sample_1280x720_30fps_10s.mp4"
            );
            inputFiles.Add(mediaFile);
        }

        var outputDir = Path.Combine(_tempDir!, "batch_outputs");
        Directory.CreateDirectory(outputDir);

        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H265,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = 1500,
            Quality = QualityPreset.Medium
        };

        var results = await _ffmpegService!.BatchTranscodeAsync(inputFiles, outputDir, settings);
        if (results.Any(r => !r.IsSuccess))
        {
            throw new InvalidOperationException("Batch transcode failed: " + string.Join(", ", results.Where(r => !r.IsSuccess).Select(r => r.ErrorMessage)));
        }
    }
}
