using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegDotnetWrapper.Services
{
    /// <summary>
    /// Extension methods for <see cref="MediaProbeResult"/>.
    /// </summary>
    public static class MediaProbeResultExtensions
    {
        /// <summary>
        /// Determines whether the media probe result contains at least one video stream.
        /// </summary>
        /// <param name="result">The media probe result.</param>
        /// <returns>true if the result contains a video stream; otherwise, false.</returns>
        public static bool HasVideo(this MediaProbeResult result)
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            return result.Streams?.Any(s => s.Width.HasValue && s.Height.HasValue) == true;
        }

        /// <summary>
        /// Determines whether the media probe result contains at least one audio stream.
        /// </summary>
        /// <param name="result">The media probe result.</param>
        /// <returns>true if the result contains an audio stream; otherwise, false.</returns>
        public static bool HasAudio(this MediaProbeResult result)
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            return result.Streams?.Any(s => s.Channels.HasValue && !s.Width.HasValue && !s.Height.HasValue) == true;
        }

        /// <summary>
        /// Gets the primary video stream (the first video stream encountered).
        /// </summary>
        /// <param name="result">The media probe result.</param>
        /// <returns>The primary video stream, or null if no video stream is present.</returns>
        public static MediaStreamInfo? GetPrimaryVideoStream(this MediaProbeResult result)
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            return result.Streams?.FirstOrDefault(s => s.Width.HasValue && s.Height.HasValue);
        }

        /// <summary>
        /// Gets the primary audio stream (the first audio stream encountered).
        /// </summary>
        /// <param name="result">The media probe result.</param>
        /// <returns>The primary audio stream, or null if no audio stream is present.</returns>
        public static MediaStreamInfo? GetPrimaryAudioStream(this MediaProbeResult result)
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            return result.Streams?.FirstOrDefault(s => s.Channels.HasValue && !s.Width.HasValue && !s.Height.HasValue);
        }

        /// <summary>
        /// Gets the aspect ratio of the primary video stream, if available.
        /// </summary>
        /// <param name="result">The media probe result.</param>
        /// <returns>The aspect ratio (width / height) as a nullable double, or null if no video stream or dimensions are unavailable.</returns>
        public static double? GetAspectRatio(this MediaProbeResult result)
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            var primaryVideo = result.GetPrimaryVideoStream();
            if (primaryVideo is null) return null;
            if (!primaryVideo.Width.HasValue || !primaryVideo.Height.HasValue || primaryVideo.Height.Value == 0) return null;
            return (double)primaryVideo.Width.Value / primaryVideo.Height.Value;
        }

        /// <summary>
        /// Determines whether the media is high definition (height >= 720 pixels) based on the primary video stream.
        /// </summary>
        /// <param name="result">The media probe result.</param>
        /// <returns>true if the media is high definition; otherwise, false.</returns>
        public static bool IsHighDefinition(this MediaProbeResult result)
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            var primaryVideo = result.GetPrimaryVideoStream();
            return primaryVideo?.Height >= 720;
        }
    }
}