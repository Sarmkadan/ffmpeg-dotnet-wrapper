using FFmpegDotnetWrapper.Cli;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Abstraction
{
    public interface IFFmpegProcessRunner
    {
        /// <summary>
        /// Executes the specified FFmpeg command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The result of the process execution.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="command"/><see cref="CliCommand.FileName"/> is <see langword="null"/> or empty.</exception>
        Task<ProcessResult> RunAsync(
            CliCommand command,
            IProgress<FFmpegProgressUpdate>? progress,
            CancellationToken cancellationToken);
    }
}