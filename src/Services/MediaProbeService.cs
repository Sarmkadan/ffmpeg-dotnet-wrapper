using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FFmpegDotnetWrapper.Services
{
    /// <summary>
    /// Service that probes media files using <c>ffprobe</c> and returns a strongly‑typed result.
    /// </summary>
    public class MediaProbeService
    {
        private const string FFprobeExecutable = "ffprobe";
        private readonly ILogger<MediaProbeService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaProbeService"/> class.
        /// </summary>
        /// <param name="logger">The logger to use. If null, <see cref="NullLogger{T}"/> is used.</param>
        public MediaProbeService(ILogger<MediaProbeService>? logger = null)
        {
            _logger = logger ?? NullLogger<MediaProbeService>.Instance;
        }

        /// <summary>
        /// Returns a string representation of the media probe service.
        /// </summary>
        /// <returns>A string identifying the service and its executable.</returns>
        public override string ToString() => $"MediaProbeService {{ FFprobeExecutable = {FFprobeExecutable} }}";

        /// <summary>
        /// Probes the specified media file and returns information about its duration, bitrate and streams.
        /// </summary>
        /// <param name="filePath">Path to the media file to probe.</param>
        /// <returns>A <see cref="MediaProbeResult"/> describing the media file.</returns>
        /// <exception cref="ProcessExecutionException">
        /// Thrown when <c>ffprobe</c> exits with a non‑zero exit code or its output cannot be parsed.
        /// </exception>
        public MediaProbeResult Probe(string filePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            // Build ffprobe arguments
            var arguments = $"-v error -print_format json -show_streams -show_format \"{filePath}\"";

            using (_logger.BeginScope(new Dictionary<string, object> { ["FilePath"] = filePath }))
            {
                _logger.LogDebug("Executing ffprobe with arguments: {Arguments}", arguments);

                var stopwatch = Stopwatch.StartNew();
                // Execute ffprobe using the shared process utility
                var result = ProcessUtilities.ExecuteProcess(
                    FFprobeExecutable,
                    arguments,
                    workingDirectory: null,
                    timeout: null);
                stopwatch.Stop();

                // If ffprobe failed, surface the expected exception type
                if (result.ExitCode != 0)
                {
                    _logger.LogWarning("ffprobe failed with exit code {ExitCode}. StandardError: {StandardError}", result.ExitCode, result.StandardError);
                    throw new ProcessExecutionException($"ffprobe failed with exit code {result.ExitCode}. Error: {result.StandardError}", result.ExitCode);
                }

                _logger.LogInformation("ffprobe completed in {ElapsedMilliseconds} ms. Stream count: {StreamCount}", stopwatch.ElapsedMilliseconds, GetStreamCount(result.StandardOutput));

                // Parse the JSON output
                var probeData = JsonSerializer.Deserialize<ProbeJson>(result.StandardOutput, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (probeData is null)
                {
                    _logger.LogWarning("Failed to deserialize ffprobe output.");
                    throw new ProcessExecutionException("Failed to deserialize ffprobe output.");
                }

                // Map to the public result type
                var duration = ParseDuration(probeData.Format?.Duration);
                var bitrate = ParseBitrate(probeData.Format?.BitRate);
                var streams = probeData.Streams?
                    .Select(s => new MediaStreamInfo(
                        Codec: s.CodecName ?? string.Empty,
                        Width: s.Width,
                        Height: s.Height,
                        Channels: s.Channels))
                    .ToList() ?? new List<MediaStreamInfo>();

                return new MediaProbeResult(duration, bitrate, streams);
            }
        }

        private static int GetStreamCount(string jsonOutput)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(jsonOutput);
                if (jsonDoc.RootElement.TryGetProperty("streams", out var streamsElement))
                {
                    return streamsElement.EnumerateArray().Count();
                }
            }
            catch
            {
                // Ignore parsing errors; we'll return 0 if we can't determine the count
            }

            return 0;
        }

        private static TimeSpan? ParseDuration(string? durationString)
        {
            if (string.IsNullOrWhiteSpace(durationString))
                return null;

            if (double.TryParse(durationString, out var seconds))
                return TimeSpan.FromSeconds(seconds);

            return null;
        }

        private static long? ParseBitrate(string? bitrateString)
        {
            if (string.IsNullOrWhiteSpace(bitrateString))
                return null;

            if (long.TryParse(bitrateString, out var bitrate))
                return bitrate;

            return null;
        }

        // Internal classes that match ffprobe's JSON structure
        private sealed class ProbeJson
        {
            public FormatInfo? Format { get; set; }
            public List<StreamInfo>? Streams { get; set; }
        }

        private sealed class FormatInfo
        {
            public string? Duration { get; set; }
            public string? BitRate { get; set; }
        }

        private sealed class StreamInfo
        {
            public string? CodecName { get; set; }
            public int? Width { get; set; }
            public int? Height { get; set; }
            public int? Channels { get; set; }
        }
    }

    /// <summary>
    /// Result returned by <see cref="MediaProbeService"/>.
    /// </summary>
    /// <param name="Duration">Overall duration of the media file, if available.</param>
    /// <param name="Bitrate">Overall bitrate of the media file, if available.</param>
    /// <param name="Streams">Collection of stream information.</param>
    public record MediaProbeResult(
        TimeSpan? Duration,
        long? Bitrate,
        IReadOnlyList<MediaStreamInfo> Streams);

    /// <summary>
    /// Information about a single media stream.
    /// </summary>
    /// <param name="Codec">Codec name (e.g., h264, aac).</param>
    /// <param name="Width">Video width, if applicable.</param>
    /// <param name="Height">Video height, if applicable.</param>
    /// <param name="Channels">Audio channel count, if applicable.</param>
    public record MediaStreamInfo(
        string Codec,
        int? Width,
        int? Height,
        int? Channels);
}
