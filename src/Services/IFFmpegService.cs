// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Main interface for orchestrating FFmpeg media operations. Provides a high-level
/// .NET API over FFmpeg CLI commands for transcoding, trimming, merging, watermarking,
/// and media analysis with progress tracking and cancellation support.
/// </summary>
public interface IFFmpegService
{
    /// <summary>
    /// Transcodes a media file to a different codec, container, or bitrate using the
    /// provided settings. Supports hardware acceleration when configured.
    /// </summary>
    /// <param name="inputMedia">The source media file with pre-analyzed metadata.</param>
    /// <param name="outputPath">Destination file path for the transcoded output.</param>
    /// <param name="settings">Transcoding settings including codec, bitrate, resolution, and audio parameters.</param>
    /// <param name="cancellationToken">Token to cancel the FFmpeg process.</param>
    /// <returns>A <see cref="ConversionResult"/> with output file info, duration, and success status.</returns>
    Task<ConversionResult> TranscodeAsync(
        MediaFile inputMedia,
        string outputPath,
        TranscodeSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transcodes a media file exactly like <see cref="TranscodeAsync(MediaFile, string, TranscodeSettings, CancellationToken)"/>,
    /// while additionally streaming incremental <see cref="FFmpegProgressUpdate"/> snapshots to
    /// <paramref name="progress"/> as FFmpeg reports them via its <c>-progress pipe:1</c> machine-readable
    /// output. Progress lines are parsed one at a time as they arrive, so memory usage stays constant
    /// regardless of job length.
    /// </summary>
    /// <param name="inputMedia">The source media file with pre-analyzed metadata, used to derive total duration for percentage calculation.</param>
    /// <param name="outputPath">Destination file path for the transcoded output.</param>
    /// <param name="settings">Transcoding settings including codec, bitrate, resolution, and audio parameters.</param>
    /// <param name="progress">Receiver of incremental progress snapshots. Must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Token to cancel the FFmpeg process.</param>
    /// <returns>A <see cref="ConversionResult"/> with output file info, duration, and success status.</returns>
    Task<ConversionResult> TranscodeAsync(
        MediaFile inputMedia,
        string outputPath,
        TranscodeSettings settings,
        IProgress<FFmpegProgressUpdate> progress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Trims a media file to the time range specified in <paramref name="settings"/>,
    /// using stream copy when possible to avoid re-encoding.
    /// </summary>
    /// <param name="inputMedia">The source media file to trim.</param>
    /// <param name="outputPath">Destination file path for the trimmed output.</param>
    /// <param name="settings">Trim settings including start time, end time, and re-encoding preference.</param>
    /// <param name="cancellationToken">Token to cancel the FFmpeg process.</param>
    /// <returns>A <see cref="ConversionResult"/> with output metadata.</returns>
    Task<ConversionResult> TrimAsync(
        MediaFile inputMedia,
        string outputPath,
        TrimSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Concatenates multiple media files into a single output file. Input files must
    /// share the same codec and stream parameters for concat demuxer compatibility.
    /// </summary>
    /// <param name="inputFiles">Ordered collection of file paths to merge.</param>
    /// <param name="outputPath">Destination file path for the merged output.</param>
    /// <param name="settings">Merge settings including transition effects and re-encoding options.</param>
    /// <param name="cancellationToken">Token to cancel the FFmpeg process.</param>
    /// <returns>A <see cref="ConversionResult"/> with the merged file metadata.</returns>
    Task<ConversionResult> MergeAsync(
        IEnumerable<string> inputFiles,
        string outputPath,
        MergeSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Overlays a watermark image or text onto a video using FFmpeg's overlay filter.
    /// </summary>
    /// <param name="inputMedia">The source video file.</param>
    /// <param name="outputPath">Destination file path for the watermarked output.</param>
    /// <param name="settings">Watermark configuration including image path, position, opacity, and scaling.</param>
    /// <param name="cancellationToken">Token to cancel the FFmpeg process.</param>
    /// <returns>A <see cref="ConversionResult"/> with output metadata.</returns>
    Task<ConversionResult> AddWatermarkAsync(
        MediaFile inputMedia,
        string outputPath,
        WatermarkSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Probes a media file using ffprobe and extracts detailed metadata including
    /// codec info, duration, bitrate, resolution, and stream details.
    /// </summary>
    /// <param name="filePath">Path to the media file to analyze.</param>
    /// <param name="cancellationToken">Token to cancel the probe operation.</param>
    /// <returns>A <see cref="MediaFile"/> populated with the extracted metadata.</returns>
    Task<MediaFile> AnalyzeMediaAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a custom FFmpeg operation with user-defined input/output arguments.
    /// Use for operations not covered by the typed methods above.
    /// </summary>
    /// <param name="operation">The custom operation definition with raw FFmpeg arguments.</param>
    /// <param name="cancellationToken">Token to cancel the FFmpeg process.</param>
    /// <returns>A <see cref="ConversionResult"/> with process exit code and output.</returns>
    Task<ConversionResult> ExecuteCustomOperationAsync(
        FFmpegOperation operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the installed FFmpeg version string (e.g., "ffmpeg version 6.1.1").
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The FFmpeg version string from stdout.</returns>
    Task<string> GetFFmpegVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether FFmpeg is installed and accessible on the system PATH.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if FFmpeg is available; otherwise <c>false</c>.</returns>
    Task<bool> IsFFmpegAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Embeds a subtitle file into a video, either as a soft-coded subtitle stream
    /// or burned directly into the video frames based on <see cref="SubtitleSettings.HardEmbed"/>.
    /// </summary>
    /// <param name="inputMedia">The source video file with pre-analyzed metadata.</param>
    /// <param name="outputPath">Destination file path for the output with embedded subtitles.</param>
    /// <param name="settings">Subtitle settings including path, encoding mode, font, and language.</param>
    /// <param name="cancellationToken">Token to cancel the FFmpeg process.</param>
    /// <returns>A <see cref="ConversionResult"/> with the output file metadata.</returns>
    Task<ConversionResult> EmbedSubtitlesAsync(
        MediaFile inputMedia,
        string outputPath,
        SubtitleSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts one or more thumbnail images from a video file.
    /// </summary>
    /// <param name="inputMedia">The source video file with pre-analyzed metadata.</param>
    /// <param name="outputPattern">
    /// Output file path pattern. Use <c>%03d</c> for sequential numbering when extracting
    /// multiple thumbnails (e.g. <c>/output/thumb_%03d.jpg</c>).
    /// </param>
    /// <param name="settings">Thumbnail settings including timestamps, format, and dimensions.</param>
    /// <param name="cancellationToken">Token to cancel the FFmpeg process.</param>
    /// <returns>A <see cref="ThumbnailResult"/> containing the paths of all extracted images.</returns>
    Task<ThumbnailResult> ExtractThumbnailsAsync(
        MediaFile inputMedia,
        string outputPattern,
        ThumbnailSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts the audio track from a media file, discarding video, and encodes it
    /// using the specified audio codec and bitrate.
    /// </summary>
    /// <param name="inputMedia">The source media file with pre-analyzed metadata.</param>
    /// <param name="outputPath">Destination file path for the extracted audio.</param>
    /// <param name="audioCodec">The audio codec to encode the extracted track with.</param>
    /// <param name="audioBitrate">The target audio bitrate in kbps.</param>
    /// <param name="cancellationToken">Token to cancel the FFmpeg process.</param>
    /// <returns>A <see cref="ConversionResult"/> with the extracted audio file metadata.</returns>
    Task<ConversionResult> ExtractAudioAsync(
        MediaFile inputMedia,
        string outputPath,
        AudioCodec audioCodec = AudioCodec.MP3,
        int audioBitrate = 192,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transcodes multiple media files sequentially into <paramref name="outputDirectory"/>,
    /// applying the same <see cref="TranscodeSettings"/> to each input.
    /// </summary>
    /// <param name="inputFiles">The source media files to transcode.</param>
    /// <param name="outputDirectory">Directory that will receive the transcoded outputs.</param>
    /// <param name="settings">Transcoding settings applied to every input file.</param>
    /// <param name="cancellationToken">Token to cancel the batch operation.</param>
    /// <returns>A <see cref="ConversionResult"/> for each input file, in input order.</returns>
    Task<List<ConversionResult>> BatchTranscodeAsync(
        IEnumerable<MediaFile> inputFiles,
        string outputDirectory,
        TranscodeSettings settings,
        CancellationToken cancellationToken = default);
}
