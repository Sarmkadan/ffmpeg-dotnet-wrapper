// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Repository;

/// <summary>
/// Provides validation helpers for <see cref="MediaRepository"/> instances.
/// </summary>
public static class MediaRepositoryValidation
{
    /// <summary>
    /// Validates the specified media repository.
    /// </summary>
    /// <param name="value">The media repository to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MediaRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Repository should not be null (already checked above)
        // No additional validation needed for the repository itself beyond null check
        // since it's just a container for the actual media files

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified media repository is valid.
    /// </summary>
    /// <param name="value">The media repository to check.</param>
    /// <returns><see langword="true"/> if the repository is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this MediaRepository? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified media repository is valid.
    /// </summary>
    /// <param name="value">The media repository to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the repository contains invalid data.</exception>
    public static void EnsureValid(this MediaRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Media repository validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates a collection of media files.
    /// </summary>
    /// <param name="mediaFiles">The media files to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if all files are valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaFiles"/> is null.</exception>
    public static IReadOnlyList<string> ValidateMediaFiles(this IEnumerable<MediaFile> mediaFiles)
    {
        ArgumentNullException.ThrowIfNull(mediaFiles);

        var problems = new List<string>();
        var index = 0;

        foreach (var mediaFile in mediaFiles)
        {
            if (mediaFile == null)
            {
                problems.Add($"Media file at index {index} is null");
                continue;
            }

            ValidateMediaFile(mediaFile, index, problems);
            index++;
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a single media file.
    /// </summary>
    /// <param name="mediaFile">The media file to validate.</param>
    /// <param name="index">The index of the file in the collection (for error reporting).</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    private static void ValidateMediaFile(MediaFile mediaFile, int index, List<string> problems)
    {
        if (string.IsNullOrWhiteSpace(mediaFile.Id))
        {
            problems.Add($"Media file at index {index} has null or empty Id");
        }

        if (string.IsNullOrWhiteSpace(mediaFile.Name))
        {
            problems.Add($"Media file at index {index} has null or empty Name");
        }

        if (string.IsNullOrWhiteSpace(mediaFile.FilePath))
        {
            problems.Add($"Media file at index {index} has null or empty FilePath");
        }

        if (mediaFile.FileSize < 0)
        {
            problems.Add($"Media file at index {index} has negative FileSize: {mediaFile.FileSize}");
        }

        if (mediaFile.CreatedAt == default)
        {
            problems.Add($"Media file at index {index} has default CreatedAt date");
        }

        if (mediaFile.CreatedAt > DateTime.UtcNow)
        {
            problems.Add($"Media file at index {index} has CreatedAt in the future: {mediaFile.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        if (mediaFile.ModifiedAt.HasValue && mediaFile.ModifiedAt.Value == default)
        {
            problems.Add($"Media file at index {index} has default ModifiedAt date");
        }

        if (mediaFile.ModifiedAt.HasValue && mediaFile.ModifiedAt.Value > DateTime.UtcNow)
        {
            problems.Add($"Media file at index {index} has ModifiedAt in the future: {mediaFile.ModifiedAt.Value:yyyy-MM-dd HH:mm:ss}");
        }

        if (mediaFile.Duration.HasValue)
        {
            if (mediaFile.Duration.Value.TotalSeconds <= 0)
            {
                problems.Add($"Media file at index {index} has non-positive Duration: {mediaFile.Duration}");
            }
        }

        if (mediaFile.Width.HasValue && mediaFile.Width <= 0)
        {
            problems.Add($"Media file at index {index} has non-positive Width: {mediaFile.Width}");
        }

        if (mediaFile.Height.HasValue && mediaFile.Height <= 0)
        {
            problems.Add($"Media file at index {index} has non-positive Height: {mediaFile.Height}");
        }

        if (mediaFile.FrameRate.HasValue && mediaFile.FrameRate <= 0)
        {
            problems.Add($"Media file at index {index} has non-positive FrameRate: {mediaFile.FrameRate}");
        }

        if (mediaFile.Bitrate.HasValue && mediaFile.Bitrate <= 0)
        {
            problems.Add($"Media file at index {index} has non-positive Bitrate: {mediaFile.Bitrate}");
        }

        if (mediaFile.AudioSampleRate.HasValue && mediaFile.AudioSampleRate <= 0)
        {
            problems.Add($"Media file at index {index} has non-positive AudioSampleRate: {mediaFile.AudioSampleRate}");
        }

        if (mediaFile.AudioChannels.HasValue && mediaFile.AudioChannels <= 0)
        {
            problems.Add($"Media file at index {index} has non-positive AudioChannels: {mediaFile.AudioChannels}");
        }

        if (mediaFile.Metadata != null)
        {
            if (mediaFile.Metadata.Count == 0 && mediaFile.Metadata.Keys.Any(k => string.IsNullOrWhiteSpace(k)))
            {
                problems.Add($"Media file at index {index} has null or empty metadata keys");
            }
        }
    }

    /// <summary>
    /// Validates that a media file ID is valid.
    /// </summary>
    /// <param name="id">The ID to validate.</param>
    /// <returns><see langword="true"/> if the ID is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidId(string? id) => !string.IsNullOrWhiteSpace(id);

    /// <summary>
    /// Validates that a file path is valid.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns><see langword="true"/> if the file path is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidFilePath(string? filePath) => !string.IsNullOrWhiteSpace(filePath);

    /// <summary>
    /// Validates that a date is not default and not in the future.
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <param name="paramName">The name of the parameter for error messages.</param>
    /// <returns><see langword="true"/> if the date is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidDate(DateTime date, string paramName)
    {
        if (date == default)
        {
            return false;
        }

        if (date > DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a nullable date is not default and not in the future.
    /// </summary>
    /// <param name="date">The nullable date to validate.</param>
    /// <param name="paramName">The name of the parameter for error messages.</param>
    /// <returns><see langword="true"/> if the date is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidDate(DateTime? date, string paramName)
    {
        if (!date.HasValue)
        {
            return true; // Nullable dates are considered valid when null
        }

        return IsValidDate(date.Value, paramName);
    }

    /// <summary>
    /// Validates that a positive number is valid.
    /// </summary>
    /// <param name="value">The number to validate.</param>
    /// <param name="paramName">The name of the parameter for error messages.</param>
    /// <returns><see langword="true"/> if the number is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidPositiveNumber(long value, string paramName) => value >= 0;

    /// <summary>
    /// Validates that a positive number is valid.
    /// </summary>
    /// <param name="value">The number to validate.</param>
    /// <param name="paramName">The name of the parameter for error messages.</param>
    /// <returns><see langword="true"/> if the number is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidPositiveNumber(int value, string paramName) => value >= 0;

    /// <summary>
    /// Validates that a positive number is valid.
    /// </summary>
    /// <param name="value">The number to validate.</param>
    /// <param name="paramName">The name of the parameter for error messages.</param>
    /// <returns><see langword="true"/> if the number is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidPositiveNumber(double value, string paramName) => value >= 0;
}