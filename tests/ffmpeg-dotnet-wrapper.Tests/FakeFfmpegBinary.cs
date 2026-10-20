// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Creates throwaway executable stand-ins for <c>ffmpeg</c>/<c>ffprobe</c> in a unique temporary directory.
/// Scripts are POSIX shell, so tests that launch them must run on Linux or macOS (CI runs on ubuntu-latest).
/// Dispose to delete the directory.
/// </summary>
public sealed class FakeFfmpegBinary : IDisposable
{
    /// <summary>The directory that holds every file created by this instance.</summary>
    public string Directory { get; }

    /// <summary>Creates a new, empty temporary directory.</summary>
    public FakeFfmpegBinary()
    {
        Directory = Path.Combine(Path.GetTempPath(), "ffmpeg-runner-tests-" + Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(Directory);
    }

    /// <summary>
    /// Writes an executable script at <paramref name="name"/> that prints <paramref name="stdout"/> and exits with <paramref name="exitCode"/>.
    /// </summary>
    public string CreateScript(string name, string stdout, int exitCode = 0, string? extraShellLines = null)
    {
        var script = "#!/bin/sh\n"
            + (extraShellLines is null ? string.Empty : extraShellLines + "\n")
            + $"printf '%s\\n' '{stdout.Replace("'", "'\\''")}'\n"
            + $"exit {exitCode}\n";

        var path = Path.Combine(Directory, name);
        File.WriteAllText(path, script);
        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
            | UnixFileMode.GroupRead | UnixFileMode.GroupExecute
            | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
        return path;
    }

    /// <summary>
    /// Writes a healthy <c>ffmpeg</c> stand-in whose <c>-version</c> banner reports <paramref name="versionToken"/>.
    /// </summary>
    public string CreateHealthyFfmpeg(string versionToken = "6.1.1-1ubuntu1")
    {
        return CreateScript(
            "ffmpeg",
            $"ffmpeg version {versionToken} Copyright (c) 2000-2023 the FFmpeg developers");
    }

    /// <summary>
    /// Writes a healthy <c>ffprobe</c> stand-in.
    /// </summary>
    public string CreateHealthyFfprobe()
    {
        return CreateScript(
            "ffprobe",
            "ffprobe version 6.1.1-1ubuntu1 Copyright (c) 2007-2023 the FFmpeg developers");
    }

    /// <summary>
    /// Deletes the temporary directory and everything in it.
    /// </summary>
    public void Dispose()
    {
        try
        {
            System.IO.Directory.Delete(Directory, recursive: true);
        }
        catch (IOException)
        {
            // Best-effort cleanup of a temp directory.
        }
    }
}
