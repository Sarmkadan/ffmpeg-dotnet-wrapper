// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;

namespace FFmpegDotnetWrapper.Cli
{
    /// <summary>
    /// Represents a single CLI command with its arguments and options.
    /// </summary>
    public class CliCommand
    {
        /// <summary>
        /// The command name (e.g., "transcode").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Positional arguments supplied to the command.
        /// </summary>
        public List<string> Arguments { get; set; } = new();

        /// <summary>
        /// Named options (flags or parameters) supplied to the command.
        /// </summary>
        public Dictionary<string, string?> Options { get; set; } = new();

        /// <summary>
        /// Optional sub‑command name.
        /// </summary>
        public string? SubCommand { get; set; }

        /// <summary>
        /// Returns a concise, single-line summary of the CLI command.
        /// </summary>
        public override string ToString()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Name))
                parts.Add(Name);
            if (!string.IsNullOrEmpty(SubCommand))
                parts.Add(SubCommand);
            var @base = string.Join(" ", parts);
            return string.IsNullOrEmpty(@base)
                ? $"(Args: {Arguments.Count}, Options: {Options.Count})"
                : $"{@base} (Args: {Arguments.Count}, Options: {Options.Count})";
        }
    }
}