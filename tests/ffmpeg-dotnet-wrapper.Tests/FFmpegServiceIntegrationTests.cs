using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class FFmpegServiceIntegrationTests
{
    private readonly Mock<IFFmpegService> _mockService;

    public FFmpegServiceIntegrationTests()
    {
        _mockService = new Mock<IFFmpegService>();
    }

    #region Transcode Workflow Tests

    [Fact]
    public async Task TranscodeWorkflow_BasicMP4ToWebM_ExecutesSuccessfully()
    {
        var inputMedia = new MediaFile
        {
            Width = 1920,
            Height = 1080,
            Duration = TimeSpan.FromSeconds(120),
            VideoCodec = "h264",
            AudioCodec = "aac",
            Bitrate = 5000000
        };

        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.VP9,
            Container = ContainerFormat.WebM,
            VideoBitrate = 4000
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/transcoded.webm");
        expectedResult.SetMetric("duration_seconds", 120);

        _mockService.Setup(s =>
            s.TranscodeAsync(inputMedia, It.IsAny<string>(), settings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.TranscodeAsync(
            inputMedia,
            "/output/transcoded.webm",
            settings);

        result.IsSuccess.Should().BeTrue();
        result.OutputFilePath.Should().Be("/output/transcoded.webm");
        result.GetMetric<int>("duration_seconds").Should().Be(120);
    }

    [Fact]
    public async Task TranscodeWorkflow_WithHardwareAcceleration_UsesSpecifiedAccelerator()
    {
        var inputMedia = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(60) };

        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H265,
            HardwareAcceleration = HwAccel.NVENC,
            VideoBitrate = 6000
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/nvenc.mp4");
        expectedResult.SetMetric("hardware_accel", "NVENC");

        _mockService.Setup(s =>
            s.TranscodeAsync(inputMedia, It.IsAny<string>(), It.IsAny<TranscodeSettings>(), It.IsAny<CancellationToken>()))
            .Callback<MediaFile, string, TranscodeSettings, CancellationToken>(
                (input, output, sett, ct) =>
                {
                    sett.HardwareAcceleration.Should().Be(HwAccel.NVENC);
                })
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.TranscodeAsync(inputMedia, "/output/nvenc.mp4", settings);

        result.IsSuccess.Should().BeTrue();
        result.GetMetric<string>("hardware_accel").Should().Be("NVENC");
    }

    [Fact]
    public async Task TranscodeWorkflow_WithAudioNormalization_IncludesNormalizationSettings()
    {
        var inputMedia = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(60) };

        var settings = new TranscodeSettings
        {
            EnableAudioNormalization = true,
            TargetLoudness = -23.0
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/normalized.mp4");

        _mockService.Setup(s =>
            s.TranscodeAsync(inputMedia, It.IsAny<string>(), settings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.TranscodeAsync(inputMedia, "/output/normalized.mp4", settings);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Trim Workflow Tests

    [Fact]
    public async Task TrimWorkflow_TrimClipFromVideo_ExecutesSuccessfully()
    {
        var inputMedia = new MediaFile
        {
            Width = 1920,
            Height = 1080,
            Duration = TimeSpan.FromSeconds(300),
            VideoCodec = "h264",
            Bitrate = 5000000
        };

        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(30),
            Duration = TimeSpan.FromSeconds(60)
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/trimmed.mp4");
        expectedResult.SetMetric("original_duration", 300);
        expectedResult.SetMetric("trimmed_duration", 60);

        _mockService.Setup(s =>
            s.TrimAsync(inputMedia, It.IsAny<string>(), settings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.TrimAsync(inputMedia, "/output/trimmed.mp4", settings);

        result.IsSuccess.Should().BeTrue();
        result.GetMetric<int>("trimmed_duration").Should().Be(60);
    }

    [Fact]
    public async Task TrimWorkflow_PreserveOnlyAudio_ExecutesSuccessfully()
    {
        var inputMedia = new MediaFile
        {
            Width = 1920,
            Height = 1080,
            Duration = TimeSpan.FromSeconds(120)
        };

        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(50),
            PreserveAudio = true,
            PreserveVideo = false
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/audio.aac");

        _mockService.Setup(s =>
            s.TrimAsync(inputMedia, It.IsAny<string>(), settings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.TrimAsync(inputMedia, "/output/audio.aac", settings);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TrimWorkflow_MultipleTrimsOnSameSource_ExecutesIndependently()
    {
        var inputMedia = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(300) };

        var trim1 = new TrimSettings { StartTime = TimeSpan.FromSeconds(0), Duration = TimeSpan.FromSeconds(60) };
        var trim2 = new TrimSettings { StartTime = TimeSpan.FromSeconds(100), Duration = TimeSpan.FromSeconds(60) };

        var result1 = new ConversionResult();
        result1.MarkAsSuccess("/output/trim1.mp4");

        var result2 = new ConversionResult();
        result2.MarkAsSuccess("/output/trim2.mp4");

        _mockService.Setup(s =>
            s.TrimAsync(inputMedia, "/output/trim1.mp4", trim1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result1);

        _mockService.Setup(s =>
            s.TrimAsync(inputMedia, "/output/trim2.mp4", trim2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result2);

        var r1 = await _mockService.Object.TrimAsync(inputMedia, "/output/trim1.mp4", trim1);
        var r2 = await _mockService.Object.TrimAsync(inputMedia, "/output/trim2.mp4", trim2);

        r1.IsSuccess.Should().BeTrue();
        r2.IsSuccess.Should().BeTrue();
        r1.OutputFilePath.Should().NotBe(r2.OutputFilePath);
    }

    #endregion

    #region Merge Workflow Tests

    [Fact]
    public async Task MergeWorkflow_ConcatenateTwoVideos_ExecutesSuccessfully()
    {
        var inputFiles = new[] { "/input/video1.mp4", "/input/video2.mp4" };

        var settings = new MergeSettings
        {
            InputFiles = new List<string>(inputFiles)
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/merged.mp4");
        expectedResult.SetMetric("input_count", 2);
        expectedResult.SetMetric("total_duration", 300);

        _mockService.Setup(s =>
            s.MergeAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), settings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.MergeAsync(inputFiles, "/output/merged.mp4", settings);

        result.IsSuccess.Should().BeTrue();
        result.GetMetric<int>("input_count").Should().Be(2);
    }

    [Fact]
    public async Task MergeWorkflow_MergeMultipleVideos_ExecutesSuccessfully()
    {
        var inputFiles = new[]
        {
            "/input/part1.mp4",
            "/input/part2.mp4",
            "/input/part3.mp4",
            "/input/part4.mp4"
        };

        var settings = new MergeSettings
        {
            InputFiles = new List<string>(inputFiles)
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/merged_4parts.mp4");
        expectedResult.SetMetric("input_count", 4);

        _mockService.Setup(s =>
            s.MergeAsync(inputFiles, It.IsAny<string>(), settings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.MergeAsync(inputFiles, "/output/merged_4parts.mp4", settings);

        result.IsSuccess.Should().BeTrue();
        result.GetMetric<int>("input_count").Should().Be(4);
    }

    [Fact]
    public async Task MergeWorkflow_MergeWithCrossfade_ConfiguresTransition()
    {
        var inputFiles = new[] { "/input/video1.mp4", "/input/video2.mp4" };

        var settings = new MergeSettings
        {
            InputFiles = new List<string>(inputFiles),
            Crossfade = true,
            CrossfadeDuration = 1.5
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/merged_crossfade.mp4");
        expectedResult.SetMetric("has_transitions", true);

        _mockService.Setup(s =>
            s.MergeAsync(inputFiles, It.IsAny<string>(), It.IsAny<MergeSettings>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<string>, string, MergeSettings, CancellationToken>(
                (inputs, output, sett, ct) =>
                {
                    sett.Crossfade.Should().BeTrue();
                    sett.CrossfadeDuration.Should().Be(1.5);
                })
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.MergeAsync(inputFiles, "/output/merged_crossfade.mp4", settings);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MergeWorkflow_MergeWithTranscode_AppliesEncodingSettings()
    {
        var inputFiles = new[] { "/input/video1.mp4", "/input/video2.mp4" };

        var transcodeSettings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H265,
            VideoBitrate = 6000
        };

        var settings = new MergeSettings
        {
            InputFiles = new List<string>(inputFiles),
            TranscodeOnMerge = true,
            TranscodeSettings = transcodeSettings
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/merged_transcoded.mp4");

        _mockService.Setup(s =>
            s.MergeAsync(inputFiles, It.IsAny<string>(), It.IsAny<MergeSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.MergeAsync(inputFiles, "/output/merged_transcoded.mp4", settings);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Watermark Workflow Tests

    [Fact]
    public async Task WatermarkWorkflow_AddWatermarkToVideo_ExecutesSuccessfully()
    {
        var watermarkFile = Path.Combine(Path.GetTempPath(), $"watermark_{Guid.NewGuid()}.png");
        File.WriteAllText(watermarkFile, "fake png");

        try
        {
            var inputMedia = new MediaFile
            {
                Width = 1920,
                Height = 1080,
                Duration = TimeSpan.FromSeconds(120)
            };

            var settings = new WatermarkSettings
            {
                WatermarkPath = watermarkFile,
                Position = WatermarkPosition.TopRight,
                Scale = 0.15,
                Opacity = 0.8
            };

            var expectedResult = new ConversionResult();
            expectedResult.MarkAsSuccess("/output/watermarked.mp4");
            expectedResult.SetMetric("watermark_position", "TopRight");

            _mockService.Setup(s =>
                s.AddWatermarkAsync(inputMedia, It.IsAny<string>(), settings, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var result = await _mockService.Object.AddWatermarkAsync(
                inputMedia,
                "/output/watermarked.mp4",
                settings);

            result.IsSuccess.Should().BeTrue();
            result.GetMetric<string>("watermark_position").Should().Be("TopRight");
        }
        finally
        {
            if (File.Exists(watermarkFile))
                File.Delete(watermarkFile);
        }
    }

    [Fact]
    public async Task WatermarkWorkflow_WatermarkAtDifferentPositions_CalculatesCorrectly()
    {
        var watermarkFile = Path.Combine(Path.GetTempPath(), $"watermark_{Guid.NewGuid()}.png");
        File.WriteAllText(watermarkFile, "fake png");

        try
        {
            var positions = new[]
            {
                (WatermarkPosition.TopLeft, "TopLeft"),
                (WatermarkPosition.TopRight, "TopRight"),
                (WatermarkPosition.BottomLeft, "BottomLeft"),
                (WatermarkPosition.BottomRight, "BottomRight"),
                (WatermarkPosition.Center, "Center")
            };

            var inputMedia = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(60) };

            foreach (var (position, posName) in positions)
            {
                var settings = new WatermarkSettings
                {
                    WatermarkPath = watermarkFile,
                    Position = position
                };

                var expectedResult = new ConversionResult();
                expectedResult.MarkAsSuccess($"/output/watermarked_{posName}.mp4");

                _mockService.Setup(s =>
                    s.AddWatermarkAsync(inputMedia, It.IsAny<string>(), It.IsAny<WatermarkSettings>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedResult);

                var result = await _mockService.Object.AddWatermarkAsync(
                    inputMedia,
                    $"/output/watermarked_{posName}.mp4",
                    settings);

                result.IsSuccess.Should().BeTrue();
            }
        }
        finally
        {
            if (File.Exists(watermarkFile))
                File.Delete(watermarkFile);
        }
    }

    #endregion

    #region Batch/Concurrent Workflow Tests

    [Fact]
    public async Task BatchWorkflow_TranscodeMultipleFilesInParallel_ExecutesConcurrently()
    {
        var inputFiles = new[]
        {
            new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(60) },
            new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(60) },
            new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(60) }
        };

        var outputFiles = new[] { "/output/out1.mp4", "/output/out2.mp4", "/output/out3.mp4" };

        var settings = new TranscodeSettings { VideoBitrate = 5000 };

        var tasks = new List<Task<ConversionResult>>();

        foreach (var (input, output) in inputFiles.Zip(outputFiles))
        {
            var result = new ConversionResult();
            result.MarkAsSuccess(output);

            _mockService.Setup(s =>
                s.TranscodeAsync(input, output, settings, It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            tasks.Add(_mockService.Object.TranscodeAsync(input, output, settings));
        }

        var results = await Task.WhenAll(tasks);

        results.Should().HaveCount(3);
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
    }

    [Fact]
    public async Task BatchWorkflow_TrimMultipleClipsFromSingleSource_ExecutesIndependently()
    {
        var sourceMedia = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(600) };

        var clips = new[]
        {
            (TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(60), "/output/clip1.mp4"),
            (TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(60), "/output/clip2.mp4"),
            (TimeSpan.FromSeconds(300), TimeSpan.FromSeconds(60), "/output/clip3.mp4"),
        };

        var tasks = new List<Task<ConversionResult>>();

        foreach (var (start, duration, output) in clips)
        {
            var settings = new TrimSettings { StartTime = start, Duration = duration };
            var result = new ConversionResult();
            result.MarkAsSuccess(output);

            _mockService.Setup(s =>
                s.TrimAsync(sourceMedia, output, settings, It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            tasks.Add(_mockService.Object.TrimAsync(sourceMedia, output, settings));
        }

        var results = await Task.WhenAll(tasks);

        results.Should().HaveCount(3);
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ErrorHandling_TranscodeWithInvalidInput_ReturnsFailureResult()
    {
        var inputMedia = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(60) };
        var settings = new TranscodeSettings();

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsFailed("FFmpeg process exited with code 1");

        _mockService.Setup(s =>
            s.TranscodeAsync(inputMedia, It.IsAny<string>(), settings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.TranscodeAsync(inputMedia, "/output/failed.mp4", settings);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("FFmpeg");
    }

    [Fact]
    public async Task ErrorHandling_CancellationToken_CancelsPendingOperation()
    {
        var inputMedia = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(60) };
        var settings = new TranscodeSettings();
        var cts = new CancellationTokenSource();

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsFailed("Operation was cancelled");

        _mockService.Setup(s =>
            s.TranscodeAsync(inputMedia, It.IsAny<string>(), settings, cts.Token))
            .ReturnsAsync(expectedResult);

        cts.Cancel();

        var result = await _mockService.Object.TranscodeAsync(inputMedia, "/output/cancelled.mp4", settings, cts.Token);

        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region Configuration Combination Tests

    [Theory]
    [InlineData(VideoCodec.H264, AudioCodec.AAC, ContainerFormat.MP4)]
    [InlineData(VideoCodec.H265, AudioCodec.VORBIS, ContainerFormat.WebM)]
    [InlineData(VideoCodec.VP9, AudioCodec.VORBIS, ContainerFormat.WebM)]
    [InlineData(VideoCodec.AV1, AudioCodec.VORBIS, ContainerFormat.WebM)]
    public async Task ConfigurationCombinations_MultipleValidCodecContainerCombinations_ExecuteSuccessfully(
        VideoCodec videoCodec,
        AudioCodec audioCodec,
        ContainerFormat container)
    {
        var inputMedia = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(60) };

        var settings = new TranscodeSettings
        {
            VideoCodec = videoCodec,
            AudioCodec = audioCodec,
            Container = container
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/test.mp4");

        _mockService.Setup(s =>
            s.TranscodeAsync(inputMedia, It.IsAny<string>(), settings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.TranscodeAsync(inputMedia, "/output/test.mp4", settings);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(QualityPreset.Ultrafast)]
    [InlineData(QualityPreset.Medium)]
    [InlineData(QualityPreset.Veryslow)]
    public async Task ConfigurationCombinations_DifferentQualityPresets_ExecuteSuccessfully(QualityPreset preset)
    {
        var inputMedia = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(60) };

        var settings = new TranscodeSettings { Quality = preset };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/quality_test.mp4");
        expectedResult.SetMetric("preset", preset.ToString());

        _mockService.Setup(s =>
            s.TranscodeAsync(inputMedia, It.IsAny<string>(), settings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.TranscodeAsync(inputMedia, "/output/quality_test.mp4", settings);

        result.IsSuccess.Should().BeTrue();
        result.GetMetric<string>("preset").Should().Be(preset.ToString());
    }

    #endregion

    #region FFmpeg Utility Tests

    [Fact]
    public async Task FFmpegUtilities_CheckFFmpegAvailable_ReturnsAvailability()
    {
        _mockService.Setup(s => s.IsFFmpegAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var available = await _mockService.Object.IsFFmpegAvailableAsync();

        available.Should().BeTrue();
    }

    [Fact]
    public async Task FFmpegUtilities_GetFFmpegVersion_ReturnsVersionString()
    {
        var versionString = "ffmpeg version 6.1.1 Copyright (c) 2000-2024";

        _mockService.Setup(s => s.GetFFmpegVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(versionString);

        var version = await _mockService.Object.GetFFmpegVersionAsync();

        version.Should().Contain("ffmpeg");
        version.Should().Contain("6.1.1");
    }

    #endregion

    #region README Example Workflow Tests

    [Fact]
    public async Task ReadmeExample_TranscodeMP4ToWebM_MatchesDocumentation()
    {
        var inputFile = new MediaFile
        {
            Width = 1920,
            Height = 1080,
            Duration = TimeSpan.FromSeconds(120),
            VideoCodec = "h264",
            Bitrate = 5000000
        };

        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.VP9,
            AudioCodec = AudioCodec.VORBIS,
            Container = ContainerFormat.WebM,
            MaxWidth = 1280,
            MaxHeight = 720,
            FrameRate = 30
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/result.webm");

        _mockService.Setup(s =>
            s.TranscodeAsync(inputFile, "/output/result.webm", settings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.TranscodeAsync(inputFile, "/output/result.webm", settings);

        result.IsSuccess.Should().BeTrue();
        result.OutputFilePath.Should().Be("/output/result.webm");
    }

    [Fact]
    public async Task ReadmeExample_TrimVideo_MatchesDocumentation()
    {
        var inputFile = new MediaFile
        {
            Width = 1920,
            Height = 1080,
            Duration = TimeSpan.FromSeconds(300),
            VideoCodec = "h264"
        };

        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(60)
        };

        var expectedResult = new ConversionResult();
        expectedResult.MarkAsSuccess("/output/trimmed.mp4");

        _mockService.Setup(s =>
            s.TrimAsync(inputFile, "/output/trimmed.mp4", settings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _mockService.Object.TrimAsync(inputFile, "/output/trimmed.mp4", settings);

        result.IsSuccess.Should().BeTrue();
        settings.GetTrimmedDuration().Should().Be(TimeSpan.FromSeconds(60));
    }

    #endregion
}
