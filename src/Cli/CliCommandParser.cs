// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFmpegDotnetWrapper.Cli
{
    /// <summary>
    /// Represents a single CLI command with its arguments and options.
    /// </summary>
    public class CliCommand
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Arguments { get; set; } = new();
        public Dictionary<string, string?> Options { get; set; } = new();
        public string? SubCommand { get; set; }
    }

    /// <summary>
    /// Parser for command-line arguments into structured command objects.
    /// Supports commands, sub-commands, arguments, and named options.
    /// Provides help generation and validation.
    /// </summary>
    public class CliCommandParser
    {
        private readonly Dictionary<string, CliCommandDefinition> _commands = new();

        /// <summary>
        /// Registers a command that can be parsed from CLI arguments.
        /// Defines the command name and expected arguments/options.
        /// </summary>
        public void RegisterCommand(CliCommandDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            _commands[definition.Name] = definition;
        }

        /// <summary>
        /// Parses raw command-line arguments into a structured CliCommand object.
        /// Validates arguments against registered command definitions.
        /// Returns null if command is not recognized.
        /// </summary>
        public CliCommand? ParseCommand(string[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            var commandName = args[0].ToLowerInvariant();

            if (!_commands.TryGetValue(commandName, out var definition))
                return null;

            var command = new CliCommand { Name = commandName };
            var positionalArgs = new List<string>();
            var i = 1;

            while (i < args.Length)
            {
                var arg = args[i];

                // Parse options (--option or -o)
                if (arg.StartsWith("--"))
                {
                    var optionName = arg.Substring(2);
                    var optionValue = null as string;

                    // Check if next arg is value or another option
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        optionValue = args[i + 1];
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }

                    command.Options[optionName] = optionValue;
                }
                else if (arg.StartsWith("-") && arg.Length == 2)
                {
                    var optionName = arg.Substring(1);
                    var optionValue = null as string;

                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        optionValue = args[i + 1];
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }

                    command.Options[optionName] = optionValue;
                }
                else
                {
                    // Positional argument
                    positionalArgs.Add(arg);
                    i++;
                }
            }

            command.Arguments = positionalArgs;
            return command;
        }

        /// <summary>
        /// Generates help text for all registered commands.
        /// Includes descriptions, arguments, and options.
        /// </summary>
        public string GenerateHelpText()
        {
            var help = new StringBuilder();
            help.AppendLine("FFmpeg .NET Wrapper - Command Line Interface");
            help.AppendLine("==============================================");
            help.AppendLine();
            help.AppendLine("Usage: ffmpeg-cli <command> [arguments] [options]");
            help.AppendLine();
            help.AppendLine("Commands:");

            foreach (var cmd in _commands.Values.OrderBy(c => c.Name))
            {
                help.AppendLine($"  {cmd.Name,-20} {cmd.Description}");
            }

            help.AppendLine();
            help.AppendLine("Examples:");
            help.AppendLine("  ffmpeg-cli transcode input.mp4 output.mkv --codec h265 --bitrate 5000");
            help.AppendLine("  ffmpeg-cli trim input.mp4 output.mp4 --start 00:00:10 --duration 00:01:00");
            help.AppendLine("  ffmpeg-cli watermark input.mp4 output.mp4 --watermark logo.png --position 10,10");
            help.AppendLine();

            return help.ToString();
        }

        /// <summary>
        /// Generates help text for a specific command.
        /// Includes detailed argument and option descriptions.
        /// </summary>
        public string GenerateCommandHelpText(string commandName)
        {
            if (!_commands.TryGetValue(commandName, out var definition))
                return $"Unknown command: {commandName}";

            var help = new StringBuilder();
            help.AppendLine($"Command: {definition.Name}");
            help.AppendLine(definition.Description);
            help.AppendLine();
            help.AppendLine("Arguments:");

            foreach (var arg in definition.Arguments)
            {
                var required = arg.IsRequired ? "[REQUIRED]" : "[OPTIONAL]";
                help.AppendLine($"  {arg.Name,-20} {required,-12} {arg.Description}");
            }

            if (definition.Options.Count > 0)
            {
                help.AppendLine();
                help.AppendLine("Options:");

                foreach (var opt in definition.Options)
                {
                    var format = string.IsNullOrEmpty(opt.ShortForm)
                        ? $"  --{opt.LongForm,-17}"
                        : $"  -{opt.ShortForm}, --{opt.LongForm,-14}";

                    help.AppendLine($"{format} {opt.Description}");
                }
            }

            return help.ToString();
        }

        /// <summary>
        /// Validates that a parsed command has all required arguments.
        /// Returns list of missing argument names if validation fails.
        /// </summary>
        public List<string> ValidateCommand(CliCommand command)
        {
            if (command == null)
                return new List<string> { "Command not provided" };

            if (!_commands.TryGetValue(command.Name, out var definition))
                return new List<string> { $"Unknown command: {command.Name}" };

            var missingArgs = new List<string>();

            for (int i = 0; i < definition.Arguments.Count; i++)
            {
                var argDef = definition.Arguments[i];
                if (argDef.IsRequired && (command.Arguments.Count <= i || string.IsNullOrEmpty(command.Arguments[i])))
                {
                    missingArgs.Add(argDef.Name);
                }
            }

            return missingArgs;
        }
    }

    /// <summary>
    /// Defines the structure of a CLI command including arguments and options.
    /// </summary>
    public class CliCommandDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<CliArgument> Arguments { get; set; } = new();
        public List<CliOption> Options { get; set; } = new();
    }

    /// <summary>
    /// Defines a positional argument expected by a command.
    /// </summary>
    public class CliArgument
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
        public string? DefaultValue { get; set; }
    }

    /// <summary>
    /// Defines a named option (flag or parameter) for a command.
    /// </summary>
    public class CliOption
    {
        public string LongForm { get; set; } = string.Empty;
        public string? ShortForm { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool RequiresValue { get; set; } = true;
        public string? DefaultValue { get; set; }
    }
}
