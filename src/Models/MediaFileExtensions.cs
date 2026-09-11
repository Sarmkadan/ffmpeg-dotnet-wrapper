// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Extension methods for <see cref="MediaFile"/> providing additional functionality and convenience methods
/// for analyzing media files, calculating properties, and formatting metadata.
/// </summary>
public static class MediaFileExtensions
{
    private const int HdHeight = 720;
    private const int FullHdHeight = 1080;
    private const int QhdHeight = 1440;
    private const int UltraHdHeight = 2160;
    private const int SdHeight = 480;

    private const int BytesPerKilobyte = 1024;

    private const int HighBitrateKbps = 5000;
    private const int MediumBitrateKbps = 2500;

    private const int HighSampleRate = 48000;
    private const int CdSampleRate = 44100;
    private const int FmSampleRate = 32000;
    private const int MediumSampleRate = 22050;
    private const int LowSampleRate = 16000;

    private const int SurroundChannels = 6;
    private const int StereoChannels = 2;
    private const int MonoChannels = 1;

    private const int BitsPerByte = 8;
    private const int KbpsToBps = 1000;

    /// <summary>
    /// Determines if the media file is a high definition video (720p or higher).
    /// </summary>
    /// <param name="mediaFile">The media file to check.</param>
    /// <returns>True if the video is HD or higher (720p+), false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mediaFile"/> is null.</exception>
    public static bool IsHighDefinition(this MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

        if (!mediaFile.IsVideo())
            return false;

        return mediaFile.Height >= HdHeight;
    }

    /// <summary>
    /// Determines if the media file is a 4K video (2160p or higher).
    /// </summary>
    /// <param name="mediaFile">The media file to check.</param>
    /// <returns>True if the video is 4K or higher (2160p+), false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mediaFile"/> is null.</exception>
    public static bool Is4K(this MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

        if (!mediaFile.IsVideo())
            return false;

        return mediaFile.Height >= UltraHdHeight;
    }

    /// <summary>
    /// Gets the aspect ratio of the video as a formatted string (e.g., "16:9", "4:3").
    /// </summary>
    /// <param name="mediaFile">The media file to analyze.</param>
    /// <returns>Aspect ratio string or null if width/height are not available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mediaFile"/> is null.</exception>
    public static string? GetAspectRatio(this MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

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
    /// <param name="mediaFile">The media file to format.</param>
    /// <returns>Human-readable duration string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mediaFile"/> is null.</exception>
    public static string GetFormattedDuration(this MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

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
    /// <param name="mediaFile">The media file to format.</param>
    /// <returns>Human-readable file size string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mediaFile"/> is null.</exception>
    public static string GetFormattedFileSize(this MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

        double sizeInMB = mediaFile.GetFileSizeInMegabytes();

        if (sizeInMB >= BytesPerKilobyte)
        {
            double sizeInGB = Math.Round(sizeInMB / BytesPerKilobyte, 2);
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
    /// <param name="mediaFile">The media file to analyze.</param>
    /// <returns>Quality description string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mediaFile"/> is null.</exception>
    public static string GetVideoQualityDescription(this MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

        if (!mediaFile.IsVideo())
            return "Audio only";

        string resolutionQuality = mediaFile.Height switch
        {
            >= UltraHdHeight => "4K Ultra HD",
            >= QhdHeight => "1440p QHD",
            >= FullHdHeight => "1080p Full HD",
            >= HdHeight => "720p HD",
            >= SdHeight => "480p SD",
            _ => "Low resolution"
        };

        if (mediaFile.Bitrate.HasValue && mediaFile.Bitrate.Value >= HighBitrateKbps)
        {
            return $"{resolutionQuality} (High bitrate)";
        }
        else if (mediaFile.Bitrate.HasValue && mediaFile.Bitrate.Value >= MediumBitrateKbps)
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
    /// <param name="mediaFile">The media file to analyze.</param>
    /// <returns>Audio quality description string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mediaFile"/> is null.</exception>
    public static string GetAudioQualityDescription(this MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

        if (!mediaFile.IsAudio())
            return "Not an audio file";

        string sampleRateQuality = mediaFile.AudioSampleRate switch
        {
            >= HighSampleRate => "High quality (48kHz+)",
            >= CdSampleRate => "CD quality (44.1kHz)",
            >= FmSampleRate => "FM radio quality (32kHz)",
            >= MediumSampleRate => "Medium quality (22.05kHz)",
            >= LowSampleRate => "Low quality (16kHz)",
            _ => "Very low quality"
        };

        string channelsDescription = mediaFile.AudioChannels switch
        {
            SurroundChannels => "5.1 Surround",
            StereoChannels => "Stereo",
            MonoChannels => "Mono",
            _ => mediaFile.AudioChannels.HasValue ? $"Multi-channel ({mediaFile.AudioChannels})" : "Unknown channels"
        };

        return $"{sampleRateQuality}, {channelsDescription}";
    }

    /// <summary>
    /// Calculates the frame count based on duration and frame rate.
    /// </summary>
    /// <param name="mediaFile">The media file to analyze.</param>
    /// <returns>Estimated frame count or null if calculation is not possible.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mediaFile"/> is null.</exception>
    public static long? GetFrameCount(this MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

        if (!mediaFile.Duration.HasValue || !mediaFile.FrameRate.HasValue || mediaFile.FrameRate.Value <= 0)
            return null;

        double totalSeconds = mediaFile.Duration.Value.TotalSeconds;
        return (long)(totalSeconds * mediaFile.FrameRate.Value);
    }

    /// <summary>
    /// Gets the estimated file size based on duration and bitrate.
    /// </summary>
    /// <param name="mediaFile">The media file to analyze.</param>
    /// <returns>Estimated file size in bytes or null if calculation is not possible.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mediaFile"/> is null.</exception>
    public static long? GetEstimatedFileSize(this MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

        if (!mediaFile.Duration.HasValue || !mediaFile.Bitrate.HasValue || mediaFile.Bitrate.Value <= 0)
            return null;

        double totalSeconds = mediaFile.Duration.Value.TotalSeconds;
        return (long)(mediaFile.Bitrate.Value * KbpsToBps * totalSeconds / BitsPerByte); // Convert kbps to bytes
    }

    /// <summary>
    /// Checks if the media file has HDR metadata.
    /// </summary>
    /// <param name="mediaFile">The media file to check.</param>
    /// <returns>True if HDR metadata is present, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mediaFile"/> is null.</exception>
    public static bool HasHDRMetadata(this MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

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
    /// <param name="mediaFile">The media file to format.</param>
    /// <returns>Localized date string in yyyy-MM-dd HH:mm:ss format.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mediaFile"/> is null.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="mediaFile"/>.CreatedAt is not a valid DateTime.</exception>
    public static string GetLocalizedCreationDate(this MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

        return mediaFile.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Helper method to calculate greatest common divisor for aspect ratio simplification.
    /// Uses Euclidean algorithm to find GCD of two integers.
    /// </summary>
    /// <param name="a">First integer.</param>
    /// <param name="b">Second integer.</param>
    /// <returns>Greatest common divisor of a and b.</returns>
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