// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Cli;

/// <summary>
/// Provides CLI command definitions for GIF export operations.
/// </summary>
public static class GifExportCliCommands
{
    /// <summary>
    /// Registers all GIF export related CLI commands with the parser.
    /// </summary>
    /// <param name="parser">The command parser instance.</param>
    public static void RegisterGifExportCommands(this CliCommandParser parser)
    {
        ArgumentNullException.ThrowIfNull(parser);

        // Register 'gif' command
        var gifCommand = new CliCommandDefinition
        {
            Name = "gif",
            Description = "Export a video segment as an optimized GIF using two-pass palette generation."
        };

        gifCommand.Arguments.Add(new CliArgument
        {
            Name = "source-path",
            Description = "Path to the source video file.",
            IsRequired = true
        });

        gifCommand.Arguments.Add(new CliArgument
        {
            Name = "output-path",
            Description = "Path where the GIF file will be saved.",
            IsRequired = true
        });

        gifCommand.Options.Add(new CliOption
        {
            LongForm = "start",
            ShortForm = "s",
            Description = "Start time of the segment in HH:MM:SS format.",
            RequiresValue = true,
            DefaultValue = "00:00:00"
        });

        gifCommand.Options.Add(new CliOption
        {
            LongForm = "duration",
            ShortForm = "d",
            Description = "Duration of the segment in HH:MM:SS format.",
            RequiresValue = true,
            DefaultValue = "00:00:10"
        });

        gifCommand.Options.Add(new CliOption
        {
            LongForm = "fps",
            ShortForm = "f",
            Description = "Frames per second for the output GIF (default: 10).",
            RequiresValue = true,
            DefaultValue = "10"
        });

        gifCommand.Options.Add(new CliOption
        {
            LongForm = "width",
            ShortForm = "w",
            Description = "Target width of the GIF (height is scaled to preserve aspect ratio).",
            RequiresValue = true,
            DefaultValue = "640"
        });

        gifCommand.Options.Add(new CliOption
        {
            LongForm = "max-width",
            ShortForm = "mw",
            Description = "Maximum width of the output GIF. Takes precedence over --width.",
            RequiresValue = true
        });

        gifCommand.Options.Add(new CliOption
        {
            LongForm = "dither",
            ShortForm = "dt",
            Description = "Dithering mode for palette conversion (none, bayer, heckbert, floyd_steinberg, sierra2, sierra2_4a, sierra3, burkes, atkinson). Default: sierra2_4a",
            RequiresValue = true,
            DefaultValue = "sierra2_4a"
        });

        gifCommand.Options.Add(new CliOption
        {
            LongForm = "loop",
            ShortForm = "l",
            Description = "Number of times the GIF should loop. Use -1 for infinite loop (default).",
            RequiresValue = true,
            DefaultValue = "-1"
        });

        gifCommand.Options.Add(new CliOption
        {
            LongForm = "quality",
            ShortForm = "q",
            Description = "Quality preset (low, medium, high).",
            RequiresValue = true,
            DefaultValue = "medium"
        });

        parser.RegisterCommand(gifCommand);
    }

    /// <summary>
    /// Parses a time string in HH:MM:SS format into a TimeSpan.
    /// </summary>
    /// <param name="timeString">The time string to parse.</param>
    /// <returns>A TimeSpan representing the parsed time.</returns>
    /// <exception cref="ArgumentException">Thrown when the time string is invalid.</exception>
    public static TimeSpan ParseTimeString(string timeString)
    {
        ArgumentException.ThrowIfNullOrEmpty(timeString);

        var parts = timeString.Split(':', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 2)
        {
            // MM:SS format
            if (int.TryParse(parts[0], out var minutes) &&
                int.TryParse(parts[1], out var seconds))
            {
                return TimeSpan.FromSeconds(minutes * 60 + seconds);
            }
        }
        else if (parts.Length == 3)
        {
            // HH:MM:SS format
            if (int.TryParse(parts[0], out var hours) &&
                int.TryParse(parts[1], out var minutes) &&
                int.TryParse(parts[2], out var seconds))
            {
                return TimeSpan.FromSeconds(hours * 3600 + minutes * 60 + seconds);
            }
        }

        throw new ArgumentException($"Invalid time format: {timeString}. Expected HH:MM:SS or MM:SS");
    }

    /// <summary>
    /// Converts a dither mode string to the DitherMode enum.
    /// </summary>
    /// <param name="ditherString">The dither mode string.</param>
    /// <returns>The corresponding DitherMode enum value.</returns>
    /// <exception cref="ArgumentException">Thrown when the dither string is invalid.</exception>
    public static DitherMode ParseDitherMode(string ditherString)
    {
        ArgumentException.ThrowIfNullOrEmpty(ditherString);

        return ditherString.ToLowerInvariant() switch
        {
            "none" => DitherMode.None,
            "bayer" => DitherMode.Bayer,
            "heckbert" => DitherMode.Heckbert,
            "floyd_steinberg" or "floyd-steinberg" => DitherMode.FloydSteinberg,
            "sierra2" => DitherMode.Sierra2,
            "sierra2_4a" or "sierra2-4a" => DitherMode.Sierra2_4a,
            "sierra3" => DitherMode.Sierra3,
            "burkes" => DitherMode.Burkes,
            "atkinson" => DitherMode.Atkinson,
            _ => throw new ArgumentException($"Invalid dither mode: {ditherString}. Valid values: none, bayer, heckbert, floyd_steinberg, sierra2, sierra2_4a, sierra3, burkes, atkinson")
        };
    }

    /// <summary>
    /// Converts a quality preset string to the GifQualityPreset enum.
    /// </summary>
    /// <param name="qualityString">The quality preset string.</param>
    /// <returns>The corresponding GifQualityPreset enum value.</returns>
    /// <exception cref="ArgumentException">Thrown when the quality string is invalid.</exception>
    public static GifQualityPreset ParseQualityPreset(string qualityString)
    {
        ArgumentException.ThrowIfNullOrEmpty(qualityString);

        return qualityString.ToLowerInvariant() switch
        {
            "low" => GifQualityPreset.Low,
            "medium" => GifQualityPreset.Medium,
            "high" => GifQualityPreset.High,
            _ => throw new ArgumentException($"Invalid quality preset: {qualityString}. Valid values: low, medium, high")
        };
    }
}
