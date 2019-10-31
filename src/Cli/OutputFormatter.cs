// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using FFmpegDotnetWrapper.Api.DTOs;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Utilities;

namespace FFmpegDotnetWrapper.Cli
{
    /// <summary>
    /// Output formatting for CLI applications.
    /// Provides table, summary, and colored console output.
    /// </summary>
    public class CliOutputFormatter
    {
        private readonly bool _useColors;
        private readonly int _consoleWidth;

        public CliOutputFormatter(bool useColors = true, int consoleWidth = 80)
        {
            _useColors = useColors;
            _consoleWidth = consoleWidth;
        }

        /// <summary>
        /// Formats a success message with optional color.
        /// </summary>
        public string FormatSuccess(string message)
        {
            return _useColors ? $"[32m✓ {message}[0m" : $"✓ {message}";
        }

        /// <summary>
        /// Formats an error message with optional color.
        /// </summary>
        public string FormatError(string message)
        {
            return _useColors ? $"[31m✗ {message}[0m" : $"✗ {message}";
        }

        /// <summary>
        /// Formats a warning message with optional color.
        /// </summary>
        public string FormatWarning(string message)
        {
            return _useColors ? $"[33m⚠ {message}[0m" : $"⚠ {message}";
        }

        /// <summary>
        /// Formats an informational message.
        /// </summary>
        public string FormatInfo(string message)
        {
            return _useColors ? $"[36mℹ {message}[0m" : $"ℹ {message}";
        }

        /// <summary>
        /// Formats a conversion result as a simple status line.
        /// </summary>
        public string FormatConversionResult(ConversionResult result)
        {
            var status = result.Success
                ? FormatSuccess($"{result.OutputFile}")
                : FormatError($"{result.OutputFile} ({result.ErrorMessage})");

            var duration = result.Success
                ? $" - {result.ExecutionTime.TotalSeconds:0.0}s"
                : string.Empty;

            return $"{status}{duration}";
        }

        /// <summary>
        /// Formats a table with multiple results.
        /// </summary>
        public string FormatResultsTable(List<ConversionResult> results)
        {
            var lines = new StringBuilder();

            // Header
            lines.AppendLine(CreateTableHeader("Input File", "Output File", "Status", "Time"));
            lines.AppendLine(CreateTableSeparator());

            // Rows
            foreach (var result in results)
            {
                var status = result.Success ? "✓ Success" : "✗ Failed";
                var time = result.Success ? $"{result.ExecutionTime.TotalSeconds:0.0}s" : "-";

                var input = TruncateString(System.IO.Path.GetFileName(result.InputFile), 20);
                var output = TruncateString(System.IO.Path.GetFileName(result.OutputFile), 20);

                var row = $"| {input,-20} | {output,-20} | {status,-10} | {time,-8} |";
                lines.AppendLine(row);
            }

            lines.AppendLine(CreateTableSeparator());

            return lines.ToString();
        }

        /// <summary>
        /// Formats a progress bar showing percentage completion.
        /// </summary>
        public string FormatProgressBar(double percentage, int width = 40)
        {
            percentage = Math.Clamp(percentage, 0, 100);
            var filledWidth = (int)((percentage / 100) * width);

            var bar = new StringBuilder();
            bar.Append("[");
            bar.Append(new string('█', filledWidth));
            bar.Append(new string('░', width - filledWidth));
            bar.Append("] ");
            bar.Append($"{percentage:0.0}%");

            return bar.ToString();
        }

        /// <summary>
        /// Formats operation summary statistics.
        /// </summary>
        public string FormatSummary(List<ConversionResult> results)
        {
            var successful = 0;
            var failed = 0;
            var totalTime = TimeSpan.Zero;

            foreach (var result in results)
            {
                if (result.Success)
                {
                    successful++;
                    totalTime += result.ExecutionTime;
                }
                else
                {
                    failed++;
                }
            }

            var lines = new StringBuilder();
            lines.AppendLine();
            lines.AppendLine("Summary:");
            lines.AppendLine($"  Total: {results.Count} operations");
            lines.AppendLine(FormatSuccess($"Succeeded: {successful}"));
            if (failed > 0)
                lines.AppendLine(FormatError($"Failed: {failed}"));
            lines.AppendLine($"  Total Time: {FormattingUtilities.FormatDuration(totalTime)}");

            return lines.ToString();
        }

        /// <summary>
        /// Creates a help box with title and content.
        /// </summary>
        public string FormatHelpBox(string title, List<string> lines)
        {
            var box = new StringBuilder();
            var width = Math.Min(_consoleWidth - 4, 80);

            // Top border
            box.AppendLine(new string('═', width + 2));

            // Title
            var centeredTitle = CenterText(title, width);
            box.AppendLine($"║ {centeredTitle} ║");

            // Separator
            box.AppendLine(new string('─', width + 2));

            // Content
            foreach (var line in lines)
            {
                var wrappedLines = WrapText(line, width);
                foreach (var wrappedLine in wrappedLines)
                {
                    box.AppendLine($"║ {wrappedLine,-{width}} ║");
                }
            }

            // Bottom border
            box.AppendLine(new string('═', width + 2));

            return box.ToString();
        }

        /// <summary>
        /// Formats a key-value pair for display (e.g., "Name:  value").
        /// </summary>
        public string FormatKeyValue(string key, string value, int keyWidth = 20)
        {
            return $"{key,-{keyWidth}} {value}";
        }

        /// <summary>
        /// Formats API response for CLI display.
        /// </summary>
        public string FormatApiResponse<T>(ApiResponse<T> response)
        {
            var lines = new StringBuilder();

            lines.AppendLine(FormatKeyValue("Status", response.Success ? "Success" : "Failed"));
            lines.AppendLine(FormatKeyValue("Code", response.StatusCode.ToString()));
            lines.AppendLine(FormatKeyValue("Message", response.Message));

            if (response.Errors.Count > 0)
            {
                lines.AppendLine("Errors:");
                foreach (var error in response.Errors)
                {
                    lines.AppendLine($"  - {error.Message}");
                }
            }

            return lines.ToString();
        }

        // Helper methods
        private string CreateTableHeader(params string[] columns)
        {
            var header = new StringBuilder();
            header.Append("|");
            foreach (var col in columns)
            {
                header.Append($" {col,-20} |");
            }
            return header.ToString();
        }

        private string CreateTableSeparator()
        {
            return "+" + new string('-', 23) + "+" + new string('-', 23) + "+" + new string('-', 12) + "+" + new string('-', 10) + "+";
        }

        private string TruncateString(string value, int maxLength)
        {
            if (value == null || value.Length <= maxLength)
                return value ?? string.Empty;
            return value.Substring(0, maxLength - 3) + "...";
        }

        private string CenterText(string text, int width)
        {
            var padding = (width - text.Length) / 2;
            return new string(' ', Math.Max(0, padding)) + text;
        }

        private List<string> WrapText(string text, int maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 <= maxWidth)
                {
                    if (currentLine.Length > 0)
                        currentLine.Append(' ');
                    currentLine.Append(word);
                }
                else
                {
                    if (currentLine.Length > 0)
                        lines.Add(currentLine.ToString());
                    currentLine = new StringBuilder(word);
                }
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            return lines;
        }
    }
}
