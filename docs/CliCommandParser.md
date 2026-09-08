# CliCommandParser Documentation

This document describes the `CliCommandParser` class and related types in the `FFmpegDotnetWrapper.Cli` namespace.

## Overview

The `CliCommandParser` is responsible for parsing command-line arguments into structured command objects. It supports commands, sub-commands, arguments, and named options. It also provides help generation and validation.

## Classes

### CliCommandParser

The main parser class that registers command definitions and parses input arguments.

#### Purpose
- Register command definitions via `RegisterCommand`.
- Parse raw string arguments into a `CliCommand` object via `ParseCommand`.
- Generate help text for all commands or a specific command.
- Validate that a parsed command has all required arguments.
- Build a safe argument list for `System.Diagnostics.ProcessStartInfo`.

#### Public API

| Member | Type | Description |
|--------|------|-------------|
| `RegisterCommand(CliCommandDefinition definition)` | `void` | Registers a command that can be parsed from CLI arguments. Throws `ArgumentNullException` if `definition` is null. |
| `ParseCommand(string[] args)` | `CliCommand?` | Parses raw command-line arguments into a structured `CliCommand` object. Returns null if command is not recognized. Throws `ArgumentNullException` if `args` is null. |
| `GenerateHelpText()` | `string` | Generates help text for all registered commands. |
| `GenerateCommandHelpText(string commandName)` | `string` | Generates help text for a specific command. Returns an error message if the command is unknown. |
| `ValidateCommand(CliCommand command)` | `List<string>` | Validates that a parsed command has all required arguments. Returns a list of missing argument names. |
| `BuildArgumentList(CliCommand command)` | `List<string>` | Builds a safe argument list suitable for `System.Diagnostics.ProcessStartInfo.ArgumentList`. Throws `ArgumentNullException` if `command` is null. |

### CliCommandDefinition

Defines the structure of a CLI command including arguments and options.

#### Purpose
- Holds the command name, description, positional arguments, and named options.

#### Public API

| Member | Type | Description |
|--------|------|-------------|
| `Name` | `string` | The command name. |
| `Description` | `string` | Human‑readable description. |
| `Arguments` | `List<CliArgument>` | Expected positional arguments. |
| `Options` | `List<CliOption>` | Expected named options. |

### CliArgument

Defines a positional argument expected by a command.

#### Purpose
- Describes a single positional argument (name, description, whether it's required, and default value).

#### Public API

| Member | Type | Description |
|--------|------|-------------|
| `Name` | `string` | Argument name. |
| `Description` | `string` | Argument description. |
| `IsRequired` | `bool` | Whether the argument is required. |
| `DefaultValue` | `string?` | Default value if the argument is optional. |

### CliOption

Defines a named option (flag or parameter) for a command.

#### Purpose
- Describes a single named option (long form, optional short form, description, whether it requires a value, and default value).

#### Public API

| Member | Type | Description |
|--------|------|-------------|
| `LongForm` | `string` | Long form name (e.g., "codec"). |
| `ShortForm` | `string?` | Optional short form (e.g., "c"). |
| `Description` | `string` | Description of the option. |
| `RequiresValue` | `bool` | Whether the option expects a value. |
| `DefaultValue` | `string?` | Default value if the option is optional. |

## Extension Methods (CliCommandExtensions)

Provides extension methods for `CliCommand` to check and retrieve option values.

#### Purpose
- Null-safe methods to check for the existence of an option, get its value, or try to get its value.

#### Public API

| Member | Type | Description |
|--------|------|-------------|
| `HasOption(this CliCommand command, string optionName)` | `bool` | Checks if the command has the specified option. Throws `ArgumentNullException` if `command` is null. Throws `ArgumentException` if `optionName` is null or empty. |
| `GetOptionValue(this CliCommand command, string optionName)` | `string?` | Gets the value of the specified option. Returns null if the option does not exist. Throws `ArgumentNullException` if `command` is null. Throws `ArgumentException` if `optionName` is null or empty. |
| `TryGetOptionValue(this CliCommand command, string optionName, out string? value)` | `bool` | Tries to get the value of the specified option. Returns true if the option exists; otherwise, false. Throws `ArgumentNullException` if `command` is null. Throws `ArgumentException` if `optionName` is null or empty. |

## Usage Example

The following example shows how to register a command, parse arguments, validate the command, and generate help text.

```csharp
using FFmpegDotnetWrapper.Cli;

// Create the parser
var parser = new CliCommandParser();

// Define a command (e.g., "transcode")
var transcodeDef = new CliCommandDefinition
{
    Name = "transcode",
    Description = "Transcode a media file to another format.",
    Arguments = new List<CliArgument>
    {
        new CliArgument { Name = "input", Description = "Input file path", IsRequired = true },
        new CliArgument { Name = "output", Description = "Output file path", IsRequired = true }
    },
    Options = new List<CliOption>
    {
        new CliOption { LongForm = "codec", ShortForm = "c", Description = "Codec to use (e.g., h264, h265)", RequiresValue = true },
        new CliOption { LongForm = "bitrate", ShortForm = "b", Description = "Bitrate in kbps", RequiresValue = true },
        new CliOption { LongForm = "help", ShortForm = "h", Description = "Show help", RequiresValue = false }
    }
};

// Register the command
parser.RegisterCommand(transcodeDef);

// Simulate command-line input: transcode input.mp4 output.mkv --codec h265 --bitrate 5000
string[] args = { "transcode", "input.mp4", "output.mkv", "--codec", "h265", "--bitrate", "5000" };

// Parse the command
CliCommand? command = parser.ParseCommand(args);
if (command == null)
{
    Console.WriteLine("Command not recognized.");
    return;
}

// Validate the command
List<string> missing = parser.ValidateCommand(command);
if (missing.Count > 0)
{
    Console.WriteLine($"Missing required arguments: {string.Join(", ", missing)}");
    return;
}

// Use extension methods to get option values
string? codec = command.GetOptionValue("codec");
string? bitrate = command.GetOptionValue("bitrate");

Console.WriteLine($"Transcoding {command.Arguments[0]} to {command.Arguments[1]}");
Console.WriteLine($"Codec: {codec}, Bitrate: {bitrate}");

// Build argument list for ProcessStartInfo
List<string> argList = parser.BuildArgumentList(command);
// argList can be passed to ProcessStartInfo.ArgumentList

// Generate help text
string help = parser.GenerateHelpText();
Console.WriteLine(help);
```

## Notes

- The parser is case-insensitive for command names (converts to lower invariant).
- Options can be specified in long form (`--option`) or short form (`-o`).
- Positional arguments that start with a dash (`-`) are prefixed with `./` in the built argument list to avoid being interpreted as options.
- The `BuildArgumentList` method escapes arguments for safe use in a shell (quotes and escapes internal quotes).