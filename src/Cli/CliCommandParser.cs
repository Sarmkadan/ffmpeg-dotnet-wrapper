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
    /// Parser for command-line arguments into structured command objects.
    /// Supports commands, sub‑commands, arguments, and named options.
    /// Provides help generation and validation.
    /// </summary>
    public class CliCommandParser
    {
        private readonly Dictionary<string, CliCommandDefinition> _commands = new();

        /// <summary>
        /// Registers a command that can be parsed from CLI arguments.
        /// Defines the command name and expected arguments/options.
        /// </summary>
        /// <param name="definition">The definition to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
        public void RegisterCommand(CliCommandDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);
            _commands[definition.Name] = definition;
        }

        /// <summary>
        /// Parses raw command-line arguments into a structured <see cref="CliCommand"/> object.
        /// Validates arguments against registered command definitions.
        /// Returns <c>null</c> if command is not recognized.
        /// </summary>
        /// <param name="args">The raw argument array.</param>
        /// <returns>A populated <see cref="CliCommand"/> or <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is null.</exception>
        public CliCommand? ParseCommand(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);

            if (args.Length == 0)
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
        /// Builds a safe argument list suitable for <see cref="System.Diagnostics.ProcessStartInfo.ArgumentList"/>.
        /// It escapes whitespace, quotes and other shell‑sensitive characters and prefixes
        /// positional arguments that start with a dash to avoid being interpreted as options.
        /// </summary>
        /// <param name="command">The parsed command.</param>
        /// <returns>A list of escaped arguments ready for ProcessStartInfo.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
        public List<string> BuildArgumentList(CliCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            var result = new List<string> { command.Name };

            // Positional arguments
            foreach (var rawArg in command.Arguments)
            {
                var safeArg = rawArg;

                // If a positional argument looks like an option, treat it as a file path.
                if (safeArg.StartsWith("-"))
                {
                    safeArg = "./" + safeArg;
                }

                result.Add(EscapeArgument(safeArg));
            }

            // Options
            foreach (var kvp in command.Options)
            {
                var optName = kvp.Key;
                var optValue = kvp.Value;

                // Use long form (--) for names longer than one character, short form (-) otherwise.
                var prefix = optName.Length == 1 ? "-" : "--";
                result.Add($"{prefix}{optName}");

                if (optValue != null)
                {
                    result.Add(EscapeArgument(optValue));
                }
            }

            return result;
        }

        /// <summary>
        /// Escapes a single argument so that it can be safely passed to a shell.
        /// On Windows it wraps the argument in double quotes if it contains whitespace
        /// or a double‑quote character, escaping internal double quotes with a backslash.
        /// On Unix‑like systems it performs a similar quoting using single quotes.
        /// </summary>
        /// <param name="argument">The raw argument.</param>
        /// <returns>The escaped argument.</returns>
        private static string EscapeArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument))
                return argument;

            // Detect characters that require quoting.
            bool needsQuoting = argument.Any(ch => char.IsWhiteSpace(ch) || ch == '"' || ch == '\'');

            if (!needsQuoting)
                return argument;

            // Simple cross‑platform quoting: wrap in double quotes and escape internal double quotes.
            var escaped = argument.Replace("\"", "\\\"");
            return $"\"{escaped}\"";
        }

        /// <summary>
        /// Generates help text for all registered commands.
        /// Includes descriptions, arguments, and options.
        /// </summary>
        /// <returns>The formatted help text.</returns>
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
        /// <param name="commandName">The name of the command.</param>
        /// <returns>The formatted help text, or an error message if the command is unknown.</returns>
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
        /// <param name="command">The command to validate.</param>
        /// <returns>A list of missing argument names.</returns>
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
        /// <summary>
        /// The command name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Human‑readable description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Expected positional arguments.
        /// </summary>
        public List<CliArgument> Arguments { get; set; } = new();

        /// <summary>
        /// Expected named options.
        /// </summary>
        public List<CliOption> Options { get; set; } = new();
    }

    /// <summary>
    /// Defines a positional argument expected by a command.
    /// </summary>
    public class CliArgument
    {
        /// <summary>
        /// Argument name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Argument description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Whether the argument is required.
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Default value if the argument is optional.
        /// </summary>
        public string? DefaultValue { get; set; }
    }

    /// <summary>
    /// Defines a named option (flag or parameter) for a command.
    /// </summary>
    public class CliOption
    {
        /// <summary>
        /// Long form name (e.g., "codec").
        /// </summary>
        public string LongForm { get; set; } = string.Empty;

        /// <summary>
        /// Optional short form (e.g., "c").
        /// </summary>
        public string? ShortForm { get; set; }

        /// <summary>
        /// Description of the option.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Whether the option expects a value.
        /// </summary>
        public bool RequiresValue { get; set; } = true;

        /// <summary>
        /// Default value if the option is optional.
        /// </summary>
        public string? DefaultValue { get; set; }
    }
}
