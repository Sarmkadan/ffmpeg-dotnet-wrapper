// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Utilities;

namespace FFmpegDotnetWrapper.Configuration;

/// <summary>
/// Configuration for launching the FFmpeg and FFprobe executables. Call <see cref="EnsureValid"/>
/// to check the configuration eagerly; <c>AddFfmpeg</c> does this at registration time.
/// </summary>
public sealed class FfmpegRunnerOptions
{
    /// <summary>The log levels accepted by <c>ffmpeg -loglevel</c>, in order of increasing verbosity.</summary>
    public static IReadOnlyList<string> AllowedLogLevels { get; } =
        new[] { "quiet", "panic", "fatal", "error", "warning", "info", "verbose", "debug", "trace" };

    /// <summary>
    /// Successful <c>ffmpeg -version</c> probes, keyed by binary path and last write time, so a binary is
    /// launched once per process and launched again only if the file is replaced.
    /// </summary>
    private static readonly ConcurrentDictionary<(string Path, DateTime WriteTimeUtc), FfmpegVersion> VersionCache = new();

    /// <summary>
    /// Path to the <c>ffmpeg</c> executable. When <see langword="null"/> or whitespace, <c>ffmpeg</c> is resolved from <c>PATH</c>.
    /// </summary>
    public string? FfmpegPath { get; set; }

    /// <summary>
    /// Path to the <c>ffprobe</c> executable. When <see langword="null"/> or whitespace, <c>ffprobe</c> is resolved from <c>PATH</c>.
    /// </summary>
    public string? FfprobePath { get; set; }

    /// <summary>
    /// Maximum time allowed for a single FFmpeg process. Must be greater than zero.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(FFmpegConstants.DefaultTimeoutSeconds);

    /// <summary>
    /// The value passed to <c>ffmpeg -loglevel</c>. Must be one of <see cref="AllowedLogLevels"/> (lowercase).
    /// </summary>
    public string LogLevel { get; set; } = "info";

    /// <summary>
    /// Directory FFmpeg processes run in. When <see langword="null"/>, the current process directory is used;
    /// when set, it must exist.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// The version reported by <c>ffmpeg -version</c>. Set by a successful <see cref="EnsureValid"/>; <see langword="null"/> before that.
    /// </summary>
    public FfmpegVersion? DetectedVersion { get; private set; }

    /// <summary>
    /// Checks the configuration and verifies that FFmpeg is usable, then caches the parsed version.
    /// </summary>
    /// <returns>The version reported by <c>ffmpeg -version</c>.</returns>
    /// <exception cref="FfmpegNotFoundException">
    /// Thrown when <c>ffmpeg</c> or <c>ffprobe</c> does not exist, or <c>ffmpeg</c> cannot be launched,
    /// exits with a non-zero code, times out, or does not print an FFmpeg version banner.
    /// </exception>
    /// <exception cref="OptionsValidationException">
    /// Thrown when <see cref="Timeout"/>, <see cref="LogLevel"/>, <see cref="WorkingDirectory"/>, or an executable's permissions are invalid.
    /// <see cref="OptionsValidationException.Violations"/> lists every rule that failed.
    /// </exception>
    public FfmpegVersion EnsureValid()
    {
        var ffmpegPath = ResolveExecutable(FfmpegPath, FFmpegConstants.FFmpegExecutableName, nameof(FfmpegPath));
        var ffprobePath = ResolveExecutable(FfprobePath, FFmpegConstants.FFprobeExecutableName, nameof(FfprobePath));

        var violations = new List<string>();

        if (Timeout <= TimeSpan.Zero)
            violations.Add($"{nameof(Timeout)} must be greater than zero, but was {Timeout}.");

        if (!AllowedLogLevels.Contains(LogLevel, StringComparer.Ordinal))
            violations.Add($"{nameof(LogLevel)} '{LogLevel}' is not a valid FFmpeg log level. Allowed values: {string.Join(", ", AllowedLogLevels)}.");

        if (WorkingDirectory is not null && (string.IsNullOrWhiteSpace(WorkingDirectory) || !Directory.Exists(WorkingDirectory)))
            violations.Add($"{nameof(WorkingDirectory)} '{WorkingDirectory}' does not exist or is not a directory.");

        if (!IsExecutable(ffmpegPath))
            violations.Add($"{nameof(FfmpegPath)} '{ffmpegPath}' is not executable.");

        if (!IsExecutable(ffprobePath))
            violations.Add($"{nameof(FfprobePath)} '{ffprobePath}' is not executable.");

        if (violations.Count > 0)
            throw new OptionsValidationException(violations);

        var version = ProbeVersion(ffmpegPath);
        DetectedVersion = version;
        return version;
    }

    private FfmpegVersion ProbeVersion(string ffmpegPath)
    {
        var key = (ffmpegPath, File.GetLastWriteTimeUtc(ffmpegPath));
        if (VersionCache.TryGetValue(key, out var cached))
            return cached;

        ProcessUtilities.ProcessResult result;
        try
        {
            result = ProcessUtilities.ExecuteProcess(ffmpegPath, "-version", WorkingDirectory, Timeout);
        }
        catch (InvalidOperationException ex)
        {
            throw new FfmpegNotFoundException(
                $"{nameof(FfmpegPath)} '{ffmpegPath}' could not be launched: {ex.InnerException?.Message ?? ex.Message}",
                ex);
        }

        if (result.TimedOut)
        {
            throw new FfmpegNotFoundException(
                $"{nameof(FfmpegPath)} '{ffmpegPath}' did not finish '-version' within {Timeout}.");
        }

        if (result.ExitCode != 0)
        {
            throw new FfmpegNotFoundException(
                $"{nameof(FfmpegPath)} '{ffmpegPath}' exited with code {result.ExitCode} for '-version': {FirstLine(result.StandardError)}");
        }

        var version = FfmpegVersion.TryParse(result.StandardOutput);
        if (version is null)
        {
            throw new FfmpegNotFoundException(
                $"{nameof(FfmpegPath)} '{ffmpegPath}' did not print an 'ffmpeg version' banner for '-version'. Output started with: {FirstLine(result.StandardOutput)}");
        }

        VersionCache[key] = version;
        return version;
    }

    private static string ResolveExecutable(string? configured, string executableName, string settingName)
    {
        if (string.IsNullOrWhiteSpace(configured))
        {
            return FindOnPath(executableName)
                ?? throw new FfmpegNotFoundException(
                    $"'{executableName}' was not found on PATH. Install FFmpeg or set {settingName} to the full path of the executable.");
        }

        if (!File.Exists(configured))
        {
            throw new FfmpegNotFoundException(
                $"{settingName} '{configured}' does not exist or is not a file.");
        }

        return configured;
    }

    private static string? FindOnPath(string executableName)
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
            return null;

        var fileName = OperatingSystem.IsWindows() && Path.GetExtension(executableName).Length == 0
            ? executableName + ".exe"
            : executableName;

        foreach (var directory in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(directory.Trim('"'), fileName);
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private static bool IsExecutable(string path)
    {
        // Windows decides executability from the file extension; the existence check covers it.
        if (OperatingSystem.IsWindows())
            return true;

        var mode = File.GetUnixFileMode(path);
        return (mode & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
    }

    private static string FirstLine(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "(no output)";

        var line = text.Split('\n', 2)[0].Trim();
        return line.Length <= 200 ? line : line[..200] + "...";
    }

    /// <summary>
    /// Returns a string that represents the current options.
    /// </summary>
    public override string ToString()
    {
        return $"FfmpegRunnerOptions {{ FfmpegPath = {FfmpegPath}, FfprobePath = {FfprobePath}, Timeout = {Timeout}, LogLevel = {LogLevel}, WorkingDirectory = {WorkingDirectory} }}";
    }
}
