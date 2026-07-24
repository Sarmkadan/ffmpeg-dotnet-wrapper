using System;
using System.Collections.Generic;
using FFmpegDotnetWrapper.Cli;
using Xunit;

namespace ffmpeg_dotnet_wrapper.Tests;

public class CliCommandTests
{
    private CliCommandParser CreateParserWithTestCommand()
    {
        var parser = new CliCommandParser();

        var definition = new CliCommandDefinition
        {
            Name = "test",
            Description = "Test command",
            Arguments = new List<CliArgument>
            {
                new CliArgument { Name = "input", Description = "Input file", IsRequired = true },
                new CliArgument { Name = "output", Description = "Output file", IsRequired = false }
            },
            Options = new List<CliOption>
            {
                new CliOption { LongForm = "verbose", ShortForm = "v", Description = "Verbose mode", RequiresValue = false },
                new CliOption { LongForm = "count", ShortForm = "c", Description = "Count", RequiresValue = true, DefaultValue = "1" }
            }
        };

        parser.RegisterCommand(definition);
        return parser;
    }

    [Fact]
    public void RegisterCommand_Throws_WhenDefinitionIsNull()
    {
        var parser = new CliCommandParser();
        Assert.Throws<ArgumentNullException>(() => parser.RegisterCommand(null!));
    }

    [Fact]
    public void ParseCommand_ReturnsNull_ForUnknownCommand()
    {
        var parser = new CliCommandParser();
        var result = parser.ParseCommand(new[] { "unknown" });
        Assert.Null(result);
    }

    [Fact]
    public void ParseCommand_ReturnsNull_ForEmptyArgs()
    {
        var parser = new CliCommandParser();
        var result = parser.ParseCommand(Array.Empty<string>());
        Assert.Null(result);
    }

    [Fact]
    public void ParseCommand_ParsesArgumentsAndOptions_Correctly()
    {
        var parser = CreateParserWithTestCommand();

        var args = new[]
        {
            "test",
            "input.mp4",
            "output.mp4",
            "--verbose",
            "-c", "5"
        };

        var command = parser.ParseCommand(args);
        Assert.NotNull(command);
        Assert.Equal("test", command!.Name);
        Assert.Equal(2, command.Arguments.Count);
        Assert.Equal("input.mp4", command.Arguments[0]);
        Assert.Equal("output.mp4", command.Arguments[1]);

        Assert.True(command.Options.ContainsKey("verbose"));
        Assert.Null(command.Options["verbose"]);

        Assert.True(command.Options.ContainsKey("c"));
        Assert.Equal("5", command.Options["c"]);
    }

    [Fact]
    public void ParseCommand_ParsesLongOptionWithValue()
    {
        var parser = CreateParserWithTestCommand();

        var args = new[]
        {
            "test",
            "input.mp4",
            "--count", "10"
        };

        var command = parser.ParseCommand(args);
        Assert.NotNull(command);
        Assert.True(command!.Options.ContainsKey("count"));
        Assert.Equal("10", command.Options["count"]);
    }

    [Fact]
    public void BuildArgumentList_EscapesArgumentsAndOptions()
    {
        var parser = CreateParserWithTestCommand();

        var command = new CliCommand
        {
            Name = "test",
            Arguments = new List<string> { "file with space.mp4", "-hidden" },
            Options = new Dictionary<string, string?>
            {
                { "verbose", null },
                { "c", "5" }
            }
        };

        var list = parser.BuildArgumentList(command);

        // First element is command name
        Assert.Equal("test", list[0]);

        // Positional arguments
        Assert.Equal("\"file with space.mp4\"", list[1]); // quoted
        Assert.Equal("./-hidden", list[2]); // prefixed with "./"

        // Options
        Assert.Contains("--verbose", list);
        Assert.Contains("-c", list);
        Assert.Contains("5", list);
    }

    [Fact]
    public void GenerateHelpText_IncludesRegisteredCommand()
    {
        var parser = CreateParserWithTestCommand();
        var help = parser.GenerateHelpText();
        Assert.Contains("test", help);
        Assert.Contains("Test command", help);
    }

    [Fact]
    public void GenerateCommandHelpText_ReturnsDetailedHelp()
    {
        var parser = CreateParserWithTestCommand();
        var help = parser.GenerateCommandHelpText("test");
        Assert.Contains("Command: test", help);
        Assert.Contains("Input file", help);
        Assert.Contains("--verbose", help);
        Assert.Contains("-c", help);
    }

    [Fact]
    public void GenerateCommandHelpText_UnknownCommand_ReturnsError()
    {
        var parser = new CliCommandParser();
        var help = parser.GenerateCommandHelpText("unknown");
        Assert.Equal("Unknown command: unknown", help);
    }

    [Fact]
    public void ValidateCommand_MissingRequired_ReturnsMissingNames()
    {
        var parser = CreateParserWithTestCommand();

        var command = new CliCommand
        {
            Name = "test",
            Arguments = new List<string> { "" } // missing second argument but it's optional
        };

        var missing = parser.ValidateCommand(command);
        Assert.Empty(missing); // no required missing

        // Now remove required argument
        command.Arguments = new List<string>();
        missing = parser.ValidateCommand(command);
        Assert.Single(missing);
        Assert.Contains("input", missing);
    }

    [Fact]
    public void ValidateCommand_UnknownCommand_ReturnsError()
    {
        var parser = new CliCommandParser();
        var command = new CliCommand { Name = "unknown" };
        var missing = parser.ValidateCommand(command);
        Assert.Single(missing);
        Assert.Contains("Unknown command: unknown", missing);
    }

    [Fact]
    public void ParseCommand_Throws_WhenArgsIsNull()
    {
        var parser = new CliCommandParser();
        Assert.Throws<ArgumentNullException>(() => parser.ParseCommand(null!));
    }

    [Fact]
    public void BuildArgumentList_Throws_WhenCommandIsNull()
    {
        var parser = new CliCommandParser();
        Assert.Throws<ArgumentNullException>(() => parser.BuildArgumentList(null!));
    }
}
