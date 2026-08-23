using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.Logging;

namespace FFmpegDotNetWrapper.Services.Extensions
{
    public static class ProgressExtensions
    {
        public static string ToConsoleString(this FFmpegProgressUpdate update)
        {
            return new string('=', (int)update.ProgressPercentage);
        }

        public static TimeSpan PercentComplete(TimeSpan totalDuration)
        {
            return TimeSpan.FromMilliseconds(totalDuration.TotalMilliseconds * 0.01);
        }

        public static TimeSpan EstimatedTimeRemaining(TimeSpan totalDuration)
        {
            return TimeSpan.FromMilliseconds(totalDuration.TotalMilliseconds * 0.99);
        }
    }
}