using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Extension methods for <see cref="StreamingProfile"/>.
/// </summary>
public static class StreamingProfileExtensions
{
    /// <summary>
    /// Gets the combined video and audio bitrate in kilobits per second.
    /// </summary>
    /// <param name="profile">The streaming profile.</param>
    /// <returns>Total bitrate in kbps.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="profile"/> is <see langword="null"/>.</exception>
    public static int GetTotalBitrateKbps(this StreamingProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return profile.TotalBitrateKbps;
    }

    /// <summary>
    /// Gets the resolution label in the format &#34;WxH&#34; (e.g., &#34;1280x720&#34;).
    /// </summary>
    /// <param name="profile">The streaming profile.</param>
    /// <returns>Resolution string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="profile"/> is <see langword="null"/>.</exception>
    public static string GetResolutionLabel(this StreamingProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return profile.Resolution;
    }

    /// <summary>
    /// Orders a sequence of streaming profiles by ascending total bitrate (video + audio).
    /// </summary>
    /// <param name="profiles">The sequence of streaming profiles.</param>
    /// <returns>Ordered sequence of profiles from lowest to highest bitrate.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="profiles"/> is <see langword="null"/>.</exception>
    public static IEnumerable<StreamingProfile> OrderByBandwidth(this IEnumerable<StreamingProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        return profiles.OrderBy(p => p.TotalBitrateKbps);
    }

    /// <summary>
    /// Gets the streaming profile with the highest total bitrate (video + audio).
    /// Returns the default value if the sequence is empty.
    /// </summary>
    /// <param name="profiles">The sequence of streaming profiles.</param>
    /// <returns>The profile with the highest bitrate, or <see langword="null"/> if empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="profiles"/> is <see langword="null"/>.</exception>
    public static StreamingProfile GetHighestQuality(this IEnumerable<StreamingProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        return profiles.OrderByDescending(p => p.TotalBitrateKbps).FirstOrDefault();
    }

    /// <summary>
    /// Gets the streaming profile with the lowest total bitrate (video + audio).
    /// Returns the default value if the sequence is empty.
    /// </summary>
    /// <param name="profiles">The sequence of streaming profiles.</param>
    /// <returns>The profile with the lowest bitrate, or <see langword="null"/> if empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="profiles"/> is <see langword="null"/>.</exception>
    public static StreamingProfile GetLowestQuality(this IEnumerable<StreamingProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        return profiles.OrderBy(p => p.TotalBitrateKbps).FirstOrDefault();
    }
}