using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.Logging;

namespace FFmpegDotNetWrapper.Services.Extensions
{
    /// <summary>
    /// Provides formatting and duration calculation helpers for FFmpeg progress updates.
    /// </summary>
    public static class ProgressExtensions
    {
        /// <summary>
        /// Creates a progress bar containing a number of equals-sign characters determined by the update's progress percentage converted to an integer.
        /// </summary>
        /// <param name="update">The progress update whose progress percentage determines the length of the returned string.</param>
        /// <returns>A string of equals-sign characters sized by the integer value of <see cref="FFmpegProgressUpdate.ProgressPercentage"/>.</returns>
        public static string ToConsoleString(this FFmpegProgressUpdate update)
        {
            ArgumentNullException.ThrowIfNull(update);
            return new string('=', (int)update.ProgressPercentage);
        }

        /// <summary>
        /// Calculates one percent of the total duration using its total milliseconds.
        /// </summary>
        /// <param name="totalDuration">The total duration from which to calculate one percent.</param>
        /// <returns>A time span equal to one percent of <paramref name="totalDuration"/>.</returns>
        public static TimeSpan PercentComplete(TimeSpan totalDuration)
        {
            return TimeSpan.FromMilliseconds(totalDuration.TotalMilliseconds * 0.01);
        }

        /// <summary>
        /// Calculates ninety-nine percent of the total duration using its total milliseconds.
        /// </summary>
        /// <param name="totalDuration">The total duration from which to calculate ninety-nine percent.</param>
        /// <returns>A time span equal to ninety-nine percent of <paramref name="totalDuration"/>.</returns>
        public static TimeSpan EstimatedTimeRemaining(TimeSpan totalDuration)
        {
            return TimeSpan.FromMilliseconds(totalDuration.TotalMilliseconds * 0.99);
        }
    }
}
