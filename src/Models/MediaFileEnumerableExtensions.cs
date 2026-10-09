using System;
using System.Collections.Generic;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides aggregate operations for sequences of <see cref="MediaFile"/> instances.
/// </summary>
public static class MediaFileEnumerableExtensions
{
    /// <summary>
    /// Calculates the total file size of the media files in a sequence, skipping null elements.
    /// </summary>
    /// <param name="files">The media files whose sizes are summed.</param>
    /// <returns>
    /// The sum of the <see cref="MediaFile.FileSize"/> values for all non-null elements,
    /// or <c>0</c> when the sequence is empty or contains only null elements.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="files"/> is <see langword="null"/>.
    /// </exception>
    public static long GetTotalFileSize(this IEnumerable<MediaFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        long totalFileSize = 0;

        foreach (var file in files)
        {
            if (file is not null)
            {
                totalFileSize += file.FileSize;
            }
        }

        return totalFileSize;
    }

    /// <summary>
    /// Calculates the total playback duration of the media files in a sequence, skipping null elements
    /// and files without a known duration.
    /// </summary>
    /// <param name="files">The media files whose durations are summed.</param>
    /// <returns>
    /// The sum of the <see cref="MediaFile.Duration"/> values that are set for all non-null elements,
    /// or <see cref="TimeSpan.Zero"/> when the sequence is empty or no element has a duration.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="files"/> is <see langword="null"/>.
    /// </exception>
    public static TimeSpan GetTotalDuration(this IEnumerable<MediaFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        var totalDuration = TimeSpan.Zero;

        foreach (var file in files)
        {
            if (file is not null && file.Duration.HasValue)
            {
                totalDuration += file.Duration.Value;
            }
        }

        return totalDuration;
    }

    /// <summary>
    /// Filters a sequence of media files to those whose file extension matches the given value.
    /// Comparison is case-insensitive and a leading dot in <paramref name="extension"/> is optional.
    /// </summary>
    /// <param name="files">The media files to filter.</param>
    /// <param name="extension">The extension to match, such as <c>".mp4"</c> or <c>"mp4"</c>.</param>
    /// <returns>A lazily evaluated sequence of the non-null media files with a matching extension.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="files"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="extension"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public static IEnumerable<MediaFile> WithExtension(this IEnumerable<MediaFile> files, string extension)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);

        var normalizedExtension = extension.StartsWith('.') ? extension : "." + extension;

        return files.Where(file => file is not null &&
                                   string.Equals(file.Extension, normalizedExtension, StringComparison.OrdinalIgnoreCase));
    }
}
