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
    }
}