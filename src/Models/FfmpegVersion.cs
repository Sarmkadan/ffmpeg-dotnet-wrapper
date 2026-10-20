// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// The version reported by the first line of <c>ffmpeg -version</c>, e.g. <c>ffmpeg version 6.1.1-1ubuntu1 Copyright ...</c>.
/// </summary>
public sealed record FfmpegVersion
{
    private static readonly Regex BannerRegex = new(@"^ffmpeg version (?<token>\S+)", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex NumberRegex = new(@"^n?(?<major>\d+)(?:\.(?<minor>\d+))?(?:\.(?<patch>\d+))?", RegexOptions.Compiled);

    /// <summary>
    /// The version token exactly as printed, e.g. <c>6.1.1-1ubuntu1</c>, <c>n7.0</c> or <c>N-12345-gabcdef</c>.
    /// </summary>
    public required string Raw { get; init; }

    /// <summary>
    /// The numeric <c>major.minor.patch</c> part of <see cref="Raw"/>, or <see langword="null"/> for
    /// development builds whose version token has no numeric release (e.g. git snapshots).
    /// </summary>
    public Version? Number { get; init; }

    /// <summary>
    /// Parses the banner printed by <c>ffmpeg -version</c>.
    /// </summary>
    /// <param name="output">The standard output of <c>ffmpeg -version</c>.</param>
    /// <returns>The parsed version, or <see langword="null"/> if the output has no <c>ffmpeg version</c> banner.</returns>
    public static FfmpegVersion? TryParse(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return null;

        var banner = BannerRegex.Match(output);
        if (!banner.Success)
            return null;

        var raw = banner.Groups["token"].Value;
        var number = NumberRegex.Match(raw);

        Version? parsed = null;
        if (number.Success)
        {
            parsed = new Version(
                int.Parse(number.Groups["major"].Value),
                number.Groups["minor"].Success ? int.Parse(number.Groups["minor"].Value) : 0,
                number.Groups["patch"].Success ? int.Parse(number.Groups["patch"].Value) : 0);
        }

        return new FfmpegVersion { Raw = raw, Number = parsed };
    }

    /// <summary>
    /// Returns the version token as printed by FFmpeg.
    /// </summary>
    public override string ToString() => Raw;
}
