#nullable enable
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public sealed class FFmpegOperationEdgeCaseTests
{
    [Fact]
    public void AddInputFile_NullPath_DoesNotAdd()
    {
        var op = new FFmpegOperation();
        op.AddInputFile(null!);
        op.InputFiles.Should().BeEmpty();
    }

    [Fact]
    public void AddInputFile_EmptyPath_DoesNotAdd()
    {
        var op = new FFmpegOperation();
        op.AddInputFile("");
        op.InputFiles.Should().BeEmpty();
    }

    [Fact]
    public void AddInputFile_WhitespacePath_DoesNotAdd()
    {
        var op = new FFmpegOperation();
        op.AddInputFile("   ");
        op.InputFiles.Should().BeEmpty();
    }

    [Fact]
    public void AddInputFile_ValidPath_AddsToList()
    {
        var op = new FFmpegOperation();
        op.AddInputFile("/input/video.mp4");
        op.InputFiles.Should().ContainSingle().Which.Should().Be("/input/video.mp4");
    }

    [Fact]
    public void AddArgument_NullArgument_DoesNotAdd()
    {
        var op = new FFmpegOperation();
        op.AddArgument(null!);
        op.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void AddArgument_EmptyArgument_DoesNotAdd()
    {
        var op = new FFmpegOperation();
        op.AddArgument("");
        op.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void AddArguments_MixedNullAndValid_OnlyAddsValid()
    {
        var op = new FFmpegOperation();
        op.AddArguments("-c:v h264", "", null!, "-crf 23");
        op.Arguments.Should().HaveCount(2);
        op.Arguments.Should().Contain("-c:v h264");
        op.Arguments.Should().Contain("-crf 23");
    }

    [Fact]
    public void BuildCommandLine_NoInputs_StartsWithFfmpeg()
    {
        var op = new FFmpegOperation { OutputFile = "out.mp4" };
        var cmd = op.BuildCommandLine();
        cmd.Should().StartWith("ffmpeg");
    }

    [Fact]
    public void BuildCommandLine_WithInputAndOutput_ContainsBoth()
    {
        var op = new FFmpegOperation { OutputFile = "out.mp4" };
        op.AddInputFile("in.mp4");
        var cmd = op.BuildCommandLine();
        cmd.Should().Contain("in.mp4");
        cmd.Should().Contain("out.mp4");
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var op = new FFmpegOperation();
        op.Id.Should().NotBeNullOrEmpty();
        op.Name.Should().BeEmpty();
        op.OutputFile.Should().BeEmpty();
        op.InputFiles.Should().BeEmpty();
        op.Arguments.Should().BeEmpty();
        op.IsParallel.Should().BeFalse();
        op.Timeout.Should().BeNull();
        op.Priority.Should().BeNull();
    }

    [Fact]
    public void MultipleInputFiles_AllIncludedInCommandLine()
    {
        var op = new FFmpegOperation { OutputFile = "merged.mp4" };
        op.AddInputFile("part1.mp4");
        op.AddInputFile("part2.mp4");
        op.AddInputFile("part3.mp4");

        var cmd = op.BuildCommandLine();

        cmd.Should().Contain("part1.mp4");
        cmd.Should().Contain("part2.mp4");
        cmd.Should().Contain("part3.mp4");
    }
}
