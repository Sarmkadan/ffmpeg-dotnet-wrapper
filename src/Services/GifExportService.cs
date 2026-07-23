// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Utilities;

namespace FFmpegDotnetWrapper.Services
{
    /// <summary>
    /// Service that creates an optimized GIF from a segment of a video using the two‑pass
    /// palettegen / paletteuse filter approach.
    /// </summary>
    public class GifExportService
    {
        private readonly string _ffmpegExecutablePath;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="ffmpegExecutablePath">
        /// Full path to the ffmpeg binary. If null or empty the service will rely on the
        /// system PATH to locate ffmpeg.
        /// </param>
        public GifExportService(string? ffmpegExecutablePath = null)
        {
            _ffmpegExecutablePath = ffmpegExecutablePath ?? "ffmpeg";
        }

        /// <summary>
        /// Generates an optimized GIF from a video segment using default settings.
        /// </summary>
        /// <param name="sourcePath">Path to the source video file.</param>
        /// <param name="start">Start time of the segment.</param>
        /// <param name="duration">Length of the segment.</param>
        /// <returns>The full path to the generated GIF file.</returns>
        public async Task<string> ExportGifAsync(
            string sourcePath,
            TimeSpan start,
            TimeSpan duration)
        {
            var settings = new GifExportSettings();
            return await ExportGifAsync(sourcePath, start, duration, settings).ConfigureAwait(false);
        }

        /// <summary>
        /// Generates an optimized GIF from a video segment.
        /// </summary>
        /// <param name="sourcePath">Path to the source video file.</param>
        /// <param name="start">Start time of the segment.</param>
        /// <param name="duration">Length of the segment.</param>
        /// <param name="settings">Configuration settings for the GIF export.</param>
        /// <returns>The full path to the generated GIF file.</returns>
        public async Task<string> ExportGifAsync(
            string sourcePath,
            TimeSpan start,
            TimeSpan duration,
            GifExportSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            settings.EnsureValid();

            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path must be provided.", nameof(sourcePath));

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Source video file not found.", sourcePath);

            int fps = settings.Fps;
            int width = settings.GetEffectiveWidth();

            // Build output file name next to the source file.
            string outputGifPath = Path.Combine(
                Path.GetDirectoryName(sourcePath)!,
                $"{Path.GetFileNameWithoutExtension(sourcePath)}_{start.TotalSeconds:F0}s_{duration.TotalSeconds:F0}s.gif");

            // Temporary palette file.
            string palettePath = Path.GetTempFileName();
            // Ensure the temp file has a .png extension (ffmpeg expects that).
            string palettePngPath = Path.ChangeExtension(palettePath, ".png");
            File.Move(palettePath, palettePngPath);
            palettePath = palettePngPath;

            try
            {
                // ---------- First pass: generate palette with optimized stats mode ----------
                // Use 'diff' stats mode for better palette generation on video segments
                string paletteArgs = $"-y -ss {start.TotalSeconds:F3} -t {duration.TotalSeconds:F3} -i \"{sourcePath}\" " +
                    $"-vf \"fps={fps},scale={width}:-1:flags=lanczos,palettegen=stats_mode=diff:max_colors=256\" \"{palettePath}\" -hide_banner -loglevel error";

                await RunFfmpegAsync(paletteArgs).ConfigureAwait(false);

                // ---------- Second pass: create GIF using palette with selected dither mode ----------
                string ditherValue = settings.DitherMode switch
                {
                    DitherMode.None => "0",
                    DitherMode.Bayer => "bayer",
                    DitherMode.Heckbert => "heckbert",
                    DitherMode.FloydSteinberg => "floyd_steinberg",
                    DitherMode.Sierra2 => "sierra2",
                    DitherMode.Sierra2_4a => "sierra2_4a",
                    DitherMode.Sierra3 => "sierra3",
                    DitherMode.Burkes => "burkes",
                    DitherMode.Atkinson => "atkinson",
                    _ => "sierra2_4a" // default
                };

                string gifArgs = $"-y -ss {start.TotalSeconds:F3} -t {duration.TotalSeconds:F3} -i \"{sourcePath}\" " +
                    $"-i \"{palettePath}\" " +
                    $"-filter_complex \"fps={fps},scale={width}:-1:flags=lanczos[x];[x][1:v]paletteuse=dither={ditherValue}\" " +
                    $"-loop {(settings.Loop == -1 ? 0 : settings.Loop)} \"{outputGifPath}\" -hide_banner -loglevel error";

                await RunFfmpegAsync(gifArgs).ConfigureAwait(false);
            }
            finally
            {
                // Clean up the temporary palette file regardless of success/failure.
                try
                {
                    if (File.Exists(palettePath))
                        File.Delete(palettePath);
                }
                catch
                {
                    // Swallow any exception – failure to delete a temp file should not break the caller.
                }
            }

            return outputGifPath;
        }

        /// <summary>
        /// Executes ffmpeg with the supplied argument string using the shared ProcessUtilities helper.
        /// </summary>
        private Task RunFfmpegAsync(string arguments)
        {
            return Task.Run(() =>
            {
                var result = ProcessUtilities.ExecuteProcess(
                    fileName: _ffmpegExecutablePath,
                    arguments: arguments,
                    workingDirectory: null,
                    timeout: null);

                if (result.ExitCode != 0)
                {
                    // Include both stdout and stderr to aid debugging.
                    string message = $"ffmpeg exited with code {result.ExitCode}. " +
                        $"StdOut: {result.StandardOutput} " +
                        $"StdErr: {result.StandardError}";
                    throw new InvalidOperationException(message);
                }
            });
        }
    }
}
