// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Extension methods for MediaFile providing additional functionality and convenience methods.
/// </summary>
public static class MediaFileExtensions
{
    /// <summary>
    /// Determines if the media file is a high definition video (1080p or higher).
    /// </summary>
    /// <param name="mediaFile">The media file to check</param>
    /// <returns>True if the video is HD or higher, false otherwise</returns>
    public static bool IsHighDefinition(this MediaFile mediaFile)
    {
        if (mediaFile == null)
            throw new ArgumentNullException(nameof(mediaFile));

        if (!mediaFile.IsVideo())
            return false;

        return mediaFile.Height >= 720;
    }

    /// <summary>
    /// Determines if the media file is a 4K video (2160p or higher).
    /// </summary>
    /// <param name="mediaFile">The media file to check</param>
    /// <returns>True if the video is 4K or higher, false otherwise</returns>
    public static bool Is4K(this MediaFile mediaFile)
    {
        if (mediaFile == null)
            throw new ArgumentNullException(nameof(mediaFile));

        if (!mediaFile.IsVideo())
            return false;

        return mediaFile.Height >= 2160;
    }

    /// <summary>
    /// Gets the aspect ratio of the video as a formatted string (e.g., "16:9", "4:3").
    /// </summary>
    /// <param name="mediaFile">The media file to analyze</param>
    /// <returns>Aspect ratio string or null if width/height are not available</returns>
    public static string? GetAspectRatio(this MediaFile mediaFile)
    {
        if (mediaFile == null)
            throw new ArgumentNullException(nameof(mediaFile));

        if (!mediaFile.Width.HasValue || !mediaFile.Height.HasValue || mediaFile.Height.Value == 0)
            return null;

        // Calculate greatest common divisor to simplify the ratio
        int width = mediaFile.Width.Value;
        int height = mediaFile.Height.Value;
        int gcd = GCD(width, height);

        return $"{width / gcd}:{height / gcd}";
    }

    /// <summary>
    /// Gets the duration in a human-readable format (e.g., "2:30", "1:15:45").
    /// </summary>
    /// <param name="mediaFile">The media file to format</param>
    /// <returns>Human-readable duration string</returns>
    public static string GetFormattedDuration(this MediaFile mediaFile)
    {
        if (mediaFile == null)
            throw new ArgumentNullException(nameof(mediaFile));

        if (!mediaFile.Duration.HasValue || mediaFile.Duration.Value.TotalSeconds <= 0)
            return "0:00";

        var duration = mediaFile.Duration.Value;
        if (duration.TotalHours >= 1)
        {
            return $"{duration.Hours}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
        else
        {
            return $"{duration.Minutes}:{duration.Seconds:D2}";
        }
    }

    /// <summary>
    /// Gets the file size in a human-readable format (e.g., "2.5 MB", "1.2 GB").
    /// </summary>
    /// <param name="mediaFile">The media file to format</param>
    /// <returns>Human-readable file size string</returns>
    public static string GetFormattedFileSize(this MediaFile mediaFile)
    {
        if (mediaFile == null)
            throw new ArgumentNullException(nameof(mediaFile));

        double sizeInMB = mediaFile.GetFileSizeInMegabytes();

        if (sizeInMB >= 1024)
        {
            double sizeInGB = Math.Round(sizeInMB / 1024, 2);
            return $"{sizeInGB} GB";
        }
        else
        {
            return $"{sizeInMB} MB";
        }
    }

    /// <summary>
    /// Gets the video quality description based on resolution and bitrate.
    /// </summary>
    /// <param name="mediaFile">The media file to analyze</param>
    /// <returns>Quality description string</returns>
    public static string GetVideoQualityDescription(this MediaFile mediaFile)
    {
        if (mediaFile == null)
            throw new ArgumentNullException(nameof(mediaFile));

        if (!mediaFile.IsVideo())
            return "Audio only";

        string resolutionQuality = mediaFile.Height switch
        {
            >= 2160 => "4K Ultra HD",
            >= 1440 => "1440p QHD",
            >= 1080 => "1080p Full HD",
            >= 720 => "720p HD",
            >= 480 => "480p SD",
            _ => "Low resolution"
        };

        if (mediaFile.Bitrate.HasValue && mediaFile.Bitrate.Value >= 5000)
        {
            return $"{resolutionQuality} (High bitrate)";
        }
        else if (mediaFile.Bitrate.HasValue && mediaFile.Bitrate.Value >= 2500)
        {
            return $"{resolutionQuality} (Medium bitrate)";
        }
        else
        {
            return resolutionQuality;
        }
    }

    /// <summary>
    /// Gets the audio quality description based on sample rate and channels.
    /// </summary>
    /// <param name="mediaFile">The media file to analyze</param>
    /// <returns>Audio quality description string</returns>
    public static string GetAudioQualityDescription(this MediaFile mediaFile)
    {
        if (mediaFile == null)
            throw new ArgumentNullException(nameof(mediaFile));

        if (!mediaFile.IsAudio())
            return "Not an audio file";

        string sampleRateQuality = mediaFile.AudioSampleRate switch
        {
            >= 48000 => "High quality (48kHz+)",
            >= 44100 => "CD quality (44.1kHz)",
            >= 32000 => "FM radio quality (32kHz)",
            >= 22050 => "Medium quality (22.05kHz)",
            >= 16000 => "Low quality (16kHz)",
            _ => "Very low quality"
        };

        string channelsDescription = mediaFile.AudioChannels switch
        {
            6 => "5.1 Surround",
            2 => "Stereo",
            1 => "Mono",
            _ => $"Multi-channel ({mediaFile.AudioChannels})"
        };

        return $"{sampleRateQuality}, {channelsDescription}";
    }

    /// <summary>
    /// Calculates the frame count based on duration and frame rate.
    /// </summary>
    /// <param name="mediaFile">The media file to analyze</param>
    /// <returns>Estimated frame count or null if calculation is not possible</returns>
    public static long? GetFrameCount(this MediaFile mediaFile)
    {
        if (mediaFile == null)
            throw new ArgumentNullException(nameof(mediaFile));

        if (!mediaFile.Duration.HasValue || !mediaFile.FrameRate.HasValue || mediaFile.FrameRate.Value <= 0)
            return null;

        double totalSeconds = mediaFile.Duration.Value.TotalSeconds;
        return (long)(totalSeconds * mediaFile.FrameRate.Value);
    }

    /// <summary>
    /// Gets the estimated file size based on duration and bitrate.
    /// </summary>
    /// <param name="mediaFile">The media file to analyze</param>
    /// <returns>Estimated file size in bytes or null if calculation is not possible</returns>
    public static long? GetEstimatedFileSize(this MediaFile mediaFile)
    {
        if (mediaFile == null)
            throw new ArgumentNullException(nameof(mediaFile));

        if (!mediaFile.Duration.HasValue || !mediaFile.Bitrate.HasValue || mediaFile.Bitrate.Value <= 0)
            return null;

        double totalSeconds = mediaFile.Duration.Value.TotalSeconds;
        return (long)(mediaFile.Bitrate.Value * 1000 * totalSeconds / 8); // Convert kbps to bytes
    }

    /// <summary>
    /// Checks if the media file has HDR metadata.
    /// </summary>
    /// <param name="mediaFile">The media file to check</param>
    /// <returns>True if HDR metadata is present, false otherwise</returns>
    public static bool HasHDRMetadata(this MediaFile mediaFile)
    {
        if (mediaFile == null)
            throw new ArgumentNullException(nameof(mediaFile));

        if (mediaFile.Metadata == null || mediaFile.Metadata.Count == 0)
            return false;

        return mediaFile.Metadata.ContainsKey("HDR") ||
               mediaFile.Metadata.ContainsKey("HDR10") ||
               mediaFile.Metadata.ContainsKey("Dolby Vision") ||
               mediaFile.Metadata.ContainsKey("HLG");
    }

    /// <summary>
    /// Gets the creation date in a localized format.
    /// </summary>
    /// <param name="mediaFile">The media file to format</param>
    /// <returns>Localized date string</returns>
    public static string GetLocalizedCreationDate(this MediaFile mediaFile)
    {
        if (mediaFile == null)
            throw new ArgumentNullException(nameof(mediaFile));

        return mediaFile.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Helper method to calculate greatest common divisor for aspect ratio simplification.
    /// </summary>
    private static int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}