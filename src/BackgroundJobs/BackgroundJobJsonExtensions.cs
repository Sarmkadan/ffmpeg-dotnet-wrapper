using System;
using System.Text.Json;

namespace FFmpegDotnetWrapper.BackgroundJobs
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="BackgroundJob"/> instances.
    /// </summary>
    public static class BackgroundJobJsonExtensions
    {
        /// <summary>
        /// Serializes the data properties of a background job to JSON.
        /// </summary>
        /// <param name="job">The background job to serialize.</param>
        /// <param name="indented">
        /// <see langword="true"/> to format the JSON with indentation; otherwise,
        /// <see langword="false"/>.
        /// </param>
        /// <returns>A JSON string containing the background job data.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="job"/> is <see langword="null"/>.
        /// </exception>
        public static string ToJson(this BackgroundJob job, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(job);

            var data = new
            {
                job.JobId,
                job.JobName,
                job.State,
                job.ProgressPercentage,
                job.StatusMessage,
                job.CreatedAt,
                job.StartedAt,
                job.CompletedAt,
                job.ErrorMessage,
                job.StackTrace,
                job.Metadata,
                job.EstimatedTimeRemaining,
                job.ExecutionTime
            };

            return JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = indented
            });
        }
    }
}
