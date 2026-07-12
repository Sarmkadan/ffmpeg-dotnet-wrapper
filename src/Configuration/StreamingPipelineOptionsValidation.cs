using System;
using System.Collections.Generic;
using System.IO;

namespace FFmpegDotnetWrapper.Configuration
{
    /// <summary>
    /// Provides validation methods for <see cref="StreamingPipelineOptions"/> instances.
    /// </summary>
    public static class StreamingPipelineOptionsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="StreamingPipelineOptions"/> instance.
        /// </summary>
        /// <param name="value">The options to validate.</param>
        /// <returns>A read-only list of validation error messages. Empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this StreamingPipelineOptions value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            if (value.DefaultSegmentDurationSeconds <= 0)
            {
                errors.Add($"DefaultSegmentDurationSeconds must be a positive number, but was {value.DefaultSegmentDurationSeconds}.");
            }

            if (value.DefaultPlaylistWindowSize < 0)
            {
                errors.Add($"DefaultPlaylistWindowSize must be non-negative, but was {value.DefaultPlaylistWindowSize}.");
            }

            if (value.MaxConcurrentPipelines <= 0)
            {
                errors.Add($"MaxConcurrentPipelines must be a positive number, but was {value.MaxConcurrentPipelines}.");
            }

            if (value.MaxConcurrentRenditionsPerPipeline <= 0)
            {
                errors.Add($"MaxConcurrentRenditionsPerPipeline must be a positive number, but was {value.MaxConcurrentRenditionsPerPipeline}.");
            }

            if (value.BitrateDecisionWindowSegments <= 0)
            {
                errors.Add($"BitrateDecisionWindowSegments must be a positive number, but was {value.BitrateDecisionWindowSegments}.");
            }

            if (value.DowngradeSpeedThreshold <= 0)
            {
                errors.Add($"DowngradeSpeedThreshold must be a positive number, but was {value.DowngradeSpeedThreshold}.");
            }

            if (value.UpgradeSpeedThreshold <= 0)
            {
                errors.Add($"UpgradeSpeedThreshold must be a positive number, but was {value.UpgradeSpeedThreshold}.");
            }

            if (value.DefaultProfiles is null)
            {
                errors.Add("DefaultProfiles cannot be null.");
            }
            else
            {
                // Validate each profile in the list
                for (int i = 0; i < value.DefaultProfiles.Count; i++)
                {
                    var profile = value.DefaultProfiles[i];
                    if (profile is null)
                    {
                        errors.Add($"DefaultProfiles[{i}] cannot be null.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(profile.Name))
                    {
                        errors.Add($"DefaultProfiles[{i}].Name cannot be null or whitespace.");
                    }

                    if (profile.Width <= 0)
                    {
                        errors.Add($"DefaultProfiles[{i}].Width must be a positive number, but was {profile.Width}.");
                    }

                    if (profile.Height <= 0)
                    {
                        errors.Add($"DefaultProfiles[{i}].Height must be a positive number, but was {profile.Height}.");
                    }

                    if (profile.VideoBitrateKbps <= 0)
                    {
                        errors.Add($"DefaultProfiles[{i}].VideoBitrateKbps must be a positive number, but was {profile.VideoBitrateKbps}.");
                    }

                    if (profile.AudioBitrateKbps < 0)
                    {
                        errors.Add($"DefaultProfiles[{i}].AudioBitrateKbps must be non-negative, but was {profile.AudioBitrateKbps}.");
                    }

                    if (profile.FrameRate < 0)
                    {
                        errors.Add($"DefaultProfiles[{i}].FrameRate must be non-negative, but was {profile.FrameRate}.");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(value.DefaultOutputBaseDirectory))
            {
                try
                {
                    // Basic validation that the path is valid
                    var path = value.DefaultOutputBaseDirectory.Trim();
                    if (path.Length > 260)
                    {
                        errors.Add("DefaultOutputBaseDirectory path exceeds maximum length of 260 characters.");
                    }
                    else if (!Path.IsPathRooted(path))
                    {
                        errors.Add("DefaultOutputBaseDirectory must be an absolute path.");
                    }
                }
                catch (Exception ex) when (ex is ArgumentException or PathTooLongException or NotSupportedException)
                {
                    errors.Add($"DefaultOutputBaseDirectory is invalid: {ex.Message}");
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="StreamingPipelineOptions"/> is valid.
        /// </summary>
        /// <param name="value">The options to check.</param>
        /// <returns><see langword="true"/> if the options are valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this StreamingPipelineOptions value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Validates the specified <see cref="StreamingPipelineOptions"/> and throws an exception if invalid.
        /// </summary>
        /// <param name="value">The options to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        public static void EnsureValid(this StreamingPipelineOptions value)
        {
            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"StreamingPipelineOptions validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
        }
    }
}
