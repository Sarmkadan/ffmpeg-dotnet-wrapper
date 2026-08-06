using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Monitoring;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class StreamingPipelineMetricsTests
{
    private readonly StreamingPipelineMetrics _metrics = new();
    private readonly StreamingProfile _profile = new("TestProfile", 1920, 1080, 5000, 192);

    [Fact]
    public void InitialState_ShouldBeEmpty()
    {
        _metrics.TotalSegmentsProduced.Should().Be(0);
        _metrics.TotalBytesProduced.Should().Be(0);
        _metrics.TotalBitrateSwitches.Should().Be(0);
        _metrics.CompletedPipelines.Should().Be(0);
        _metrics.FailedPipelines.Should().Be(0);
        _metrics.AveragePipelineDuration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void RecordSegmentProduced_ShouldUpdateMetrics()
    {
        _metrics.RecordSegmentProduced(_profile, 1024);

        _metrics.TotalSegmentsProduced.Should().Be(1);
        _metrics.TotalBytesProduced.Should().Be(1024);
        
        var breakdown = _metrics.GetProfileBreakdown();
        breakdown.Should().ContainKey("TestProfile");
        breakdown["TestProfile"].TotalSegments.Should().Be(1);
        breakdown["TestProfile"].TotalBytes.Should().Be(1024);
    }

    [Fact]
    public void RecordBitrateSwitch_ShouldUpdateMetrics()
    {
        _metrics.RecordBitrateSwitch(true);
        _metrics.RecordBitrateSwitch(false);

        _metrics.TotalBitrateSwitches.Should().Be(2);
        _metrics.TotalUpgrades.Should().Be(1);
        _metrics.TotalDowngrades.Should().Be(1);
    }

    [Fact]
    public void RecordPipelineCompleted_ShouldUpdateMetrics()
    {
        var duration = TimeSpan.FromSeconds(30);
        _metrics.RecordPipelineCompleted("pipe-1", duration);

        _metrics.CompletedPipelines.Should().Be(1);
        _metrics.AveragePipelineDuration.Should().Be(duration);
    }

    [Fact]
    public void RecordPipelineFailed_ShouldUpdateMetrics()
    {
        _metrics.RecordPipelineFailed("pipe-1");
        _metrics.FailedPipelines.Should().Be(1);
    }

    [Fact]
    public void Reset_ShouldClearAllMetrics()
    {
        _metrics.RecordSegmentProduced(_profile, 1024);
        _metrics.RecordBitrateSwitch(true);
        _metrics.RecordPipelineCompleted("pipe-1", TimeSpan.FromSeconds(10));

        _metrics.Reset();

        InitialState_ShouldBeEmpty();
        _metrics.GetProfileBreakdown().Should().BeEmpty();
    }
}
