// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegDotnetWrapper.Utilities
{
    /// <summary>
    /// Process execution utilities for running external commands like FFmpeg and FFprobe.
    /// Handles process creation, output capture, error handling, and timeout management.
    /// Ensures safe execution with proper resource cleanup and error reporting.
    /// </summary>
    public static class ProcessUtilities
    {
        /// <summary>
        /// Represents the result of a process execution.
        /// Includes exit code, stdout, and stderr for comprehensive error diagnostics.
        /// </summary>
        public class ProcessResult
        {
            /// <summary>
            /// Exit code from the process (0 = success, non-zero = error).
            /// FFmpeg returns 0 on success, >0 on various error conditions.
            /// </summary>
            public int ExitCode { get; set; }

            /// <summary>
            /// Standard output produced by the process.
            /// Contains progress information and operational output.
            /// </summary>
            public string StandardOutput { get; set; } = string.Empty;

            /// <summary>
            /// Standard error output produced by the process.
            /// Contains error messages, warnings, and diagnostic information.
            /// </summary>
            public string StandardError { get; set; } = string.Empty;

            /// <summary>
            /// Total execution time of the process.
            /// Used for performance monitoring and timeout detection.
            /// </summary>
            public TimeSpan ExecutionTime { get; set; }

            /// <summary>
            /// Whether the process was terminated due to timeout.
            /// </summary>
            public bool TimedOut { get; set; }

            /// <summary>
            /// Indicates successful execution (exit code == 0).
            /// </summary>
            public bool Success => ExitCode == 0 && !TimedOut;

            public override string ToString()
            {
                return $"ProcessResult {{ ExitCode = {ExitCode}, StandardOutput = {StandardOutput}, StandardError = {StandardError}, ExecutionTime = {ExecutionTime}, TimedOut = {TimedOut} }}";
            }
        }

        /// <summary>
        /// Executes a process synchronously with output capture and error handling.
        /// Blocks until the process completes or timeout is reached.
        /// Automatically kills the process if timeout expires.
        /// </summary>
        public static ProcessResult ExecuteProcess(
            string fileName,
            string arguments,
            string? workingDirectory = null,
            TimeSpan? timeout = null,
            string? input = null)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            timeout ??= TimeSpan.FromMinutes(10); // Default 10-minute timeout

            var startTime = DateTime.UtcNow;
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = !string.IsNullOrEmpty(input),
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };

            try
            {
                process.Start();

                // Write input if provided
                if (!string.IsNullOrEmpty(input) && process.StandardInput != null)
                {
                    process.StandardInput.Write(input);
                    process.StandardInput.Close();
                }

                // Capture output asynchronously to prevent deadlock
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                // Wait for completion with timeout
                var completed = process.WaitForExit((int)timeout.Value.TotalMilliseconds);

                var output = outputTask.Result;
                var error = errorTask.Result;

                var executionTime = DateTime.UtcNow - startTime;

                if (!completed)
                {
                    // Process timed out - kill it
                    try
                    {
                        process.Kill(entireProcessTree: true);
                    }
                    catch
                    {
                        // Already terminated
                    }

                    return new ProcessResult
                    {
                        ExitCode = -1,
                        StandardOutput = output,
                        StandardError = error,
                        ExecutionTime = executionTime,
                        TimedOut = true
                    };
                }

                return new ProcessResult
                {
                    ExitCode = process.ExitCode,
                    StandardOutput = output,
                    StandardError = error,
                    ExecutionTime = executionTime,
                    TimedOut = false
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to execute process '{fileName}' with arguments '{arguments}'",
                    ex);
            }
        }

        /// <summary>
        /// Executes a process asynchronously with cancellation support.
        /// Suitable for long-running operations that need to be cancelled mid-execution.
        /// </summary>
        public static async Task<ProcessResult> ExecuteProcessAsync(
            string fileName,
            string arguments,
            string? workingDirectory = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            timeout ??= TimeSpan.FromMinutes(10);

            var startTime = DateTime.UtcNow;
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };

            try
            {
                process.Start();

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeout.Value);

                try
                {
                    // Capture output asynchronously to prevent deadlock
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    // Wait for completion with cancellation support
                    await process.WaitForExitAsync(cts.Token);

                    var output = await outputTask;
                    var error = await errorTask;
                    var executionTime = DateTime.UtcNow - startTime;

                    return new ProcessResult
                    {
                        ExitCode = process.ExitCode,
                        StandardOutput = output,
                        StandardError = error,
                        ExecutionTime = executionTime,
                        TimedOut = false
                    };
                }
                catch (OperationCanceledException)
                {
                    // Process timed out or was cancelled
                    try
                    {
                        process.Kill(entireProcessTree: true);
                    }
                    catch
                    {
                        // Already terminated
                    }

                    var executionTime = DateTime.UtcNow - startTime;
                    return new ProcessResult
                    {
                        ExitCode = -1,
                        StandardError = "Process was cancelled or timed out",
                        ExecutionTime = executionTime,
                        TimedOut = true
                    };
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to execute async process '{fileName}'",
                    ex);
            }
        }

        /// <summary>
        /// Checks if a given executable exists in the system PATH.
        /// Used to verify FFmpeg and FFprobe are installed before attempting operations.
        /// </summary>
        public static bool IsExecutableAvailable(string executableName)
        {
            try
            {
                var result = ExecuteProcess(
                    executableName,
                    "-version",
                    timeout: TimeSpan.FromSeconds(5)
                );

                return result.Success || !result.TimedOut;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Parses FFmpeg stderr output to extract progress information.
        /// Calculates percentage completion based on frame count and total frames.
        /// </summary>
        public static double ExtractProgressPercentage(string ffmpegOutput, long estimatedTotalFrames)
        {
            if (estimatedTotalFrames <= 0)
                return 0;

            // Parse "frame=X" from ffmpeg output
            var frameMatch = System.Text.RegularExpressions.Regex.Match(ffmpegOutput, @"frame=\s*(\d+)");

            if (frameMatch.Success && long.TryParse(frameMatch.Groups[1].Value, out var currentFrame))
            {
                var percentage = (double)currentFrame / estimatedTotalFrames * 100;
                return Math.Min(percentage, 100);
            }

            return 0;
        }

        /// <summary>
        /// Escapes command-line arguments properly for shell execution.
        /// Prevents command injection attacks when building dynamic FFmpeg commands.
        /// </summary>
        public static string EscapeArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument))
                return "\"\"";

            // If argument contains spaces or special chars, wrap in quotes
            if (argument.Contains(" ") || argument.Contains("\"") || argument.Contains("\\"))
            {
                return $"\"{argument.Replace("\"", "\\\"")}\"";
            }

            return argument;
        }
    }
}
