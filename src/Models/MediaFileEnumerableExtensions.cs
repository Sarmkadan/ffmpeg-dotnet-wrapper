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
}
