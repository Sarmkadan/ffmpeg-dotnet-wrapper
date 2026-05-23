// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Represents the outcome of a thumbnail extraction operation.
/// Contains the paths of all extracted images and timing information.
/// </summary>
public class ThumbnailResult
{
    /// <summary>Whether the extraction completed without errors.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Paths of the successfully extracted thumbnail files, in extraction order.</summary>
    public List<string> Thumbnails { get; set; } = [];

    /// <summary>Wall-clock time taken to complete the extraction.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Error message when <see cref="IsSuccess"/> is <c>false</c>.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Number of thumbnails extracted successfully.</summary>
    public int Count => Thumbnails.Count;

    /// <summary>Returns the first extracted thumbnail path, or <c>null</c> if none.</summary>
    public string? FirstThumbnail => Thumbnails.Count > 0 ? Thumbnails[0] : null;
}
