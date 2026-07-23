using System.Collections.Generic;
using FFmpegDotnetWrapper.Cli;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class CliCommandParserTests
{
    private static CliCommandParser CreateParser()
    {
        var parser = new CliCommandParser();

        var definition = new CliCommandDefinition
        {
            Name = "transcode",
            Description = "Transcode video",
            Arguments = new List<CliArgument>
            {
                new() { Name = "input", Description = "Input file", IsRequired = true },
                new() { Name = "output", Description = "Output file", IsRequired = true }
            }
        };

        parser.RegisterCommand(definition);
        return parser;
    }

    [Fact]
    public void BuildArgumentList_EscapesWhitespacePath()
    {
        var parser = CreateParser();
        var cmd = parser.ParseCommand(new[] { "transcode", "my video (1).mp4", "out.mkv" });
        var args = parser.BuildArgumentList(cmd!);
        Assert.Contains("\"my video (1).mp4\"", args);
    }

    [Fact]
    public void BuildArgumentList_EscapesQuotesInPath()
    {
        var parser = CreateParser();
        var cmd = parser.ParseCommand(new[] { "transcode", "a'b\"c.mkv", "out.mkv" });
        var args = parser.BuildArgumentList(cmd!);
        Assert.Contains("\"a'b\\\"c.mkv\"", args);
    }

    [Fact]
    public void BuildArgumentList_PrefixesDashStartingPath()
    {
        var parser = CreateParser();
        var cmd = parser.ParseCommand(new[] { "transcode", "-vf.mp4", "out.mkv" });
        var args = parser.BuildArgumentList(cmd!);
        Assert.Contains("./-vf.mp4", args);
    }
}
