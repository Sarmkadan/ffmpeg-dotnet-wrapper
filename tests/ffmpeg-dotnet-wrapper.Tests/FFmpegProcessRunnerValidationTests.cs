namespace FFmpegDotnetWrapper.Tests;

using System;
using System.Threading.Tasks;

using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;

using FluentAssertions;

using Xunit;

public class FFmpegProcessRunnerValidationTests
{
    [Fact]
    public async Task RunAsync_WhenConversionOptionsInvalid_ThrowsBeforeStartingProcess()
    {
        // The executable does not exist: if validation ran after Process.Start, the test would fail
        // with a process-start error instead of OptionsValidationException.
        var runner = new FFmpegProcessRunner();
        var request = new FFmpegProcessRequest
        {
            FileName = "nonexistent-ffmpeg-binary-for-validation-test",
            Arguments = string.Empty,
            ConversionOptions = new ConversionOptions { Crf = 60 }
        };

        Func<Task> act = () => runner.RunAsync(request, progress: null);

        await act.Should().ThrowAsync<OptionsValidationException>();
    }
}
