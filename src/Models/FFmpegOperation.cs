// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Represents a single FFmpeg command operation.
/// </summary>
public class FFmpegOperation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public FFmpegOperationType Type { get; set; }
    public List<string> InputFiles { get; set; } = new();
    public string OutputFile { get; set; } = string.Empty;
    public List<string> Arguments { get; set; } = new();
    public TimeSpan? Timeout { get; set; }
    public int? Priority { get; set; }
    public bool IsParallel { get; set; } = false;
    public Dictionary<string, string> CustomProperties { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAt { get; set; }

    /// <summary>
    /// Adds an input file to the operation.
    /// </summary>
    public void AddInputFile(string filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath))
            InputFiles.Add(filePath);
    }

    /// <summary>
    /// Adds a command argument.
    /// </summary>
    public void AddArgument(string argument)
    {
        if (!string.IsNullOrWhiteSpace(argument))
            Arguments.Add(argument);
    }

    /// <summary>
    /// Adds multiple arguments.
    /// </summary>
    public void AddArguments(params string[] arguments)
    {
        foreach (var arg in arguments)
            AddArgument(arg);
    }

    /// <summary>
    /// Builds the complete FFmpeg command line.
    /// </summary>
    public string BuildCommandLine()
    {
        var cmd = new System.Text.StringBuilder("ffmpeg");

        // Add input files
        foreach (var input in InputFiles)
        {
            cmd.Append($" -i \"{input}\"");
        }

        // Add arguments
        foreach (var arg in Arguments)
        {
            cmd.Append($" {arg}");
        }

        // Add output file
        cmd.Append($" \"{OutputFile}\"");

        return cmd.ToString();
    }

    /// <summary>
    /// Validates the operation before execution.
    /// </summary>
    public void Validate()
    {
        if (InputFiles.Count == 0)
            throw new InvalidOperationException("At least one input file is required");

        if (string.IsNullOrWhiteSpace(OutputFile))
            throw new InvalidOperationException("Output file path is required");

        foreach (var input in InputFiles)
        {
            if (!File.Exists(input))
                throw new InvalidOperationException($"Input file does not exist: {input}");
        }

        // Verify output directory exists or can be created
        var outputDir = Path.GetDirectoryName(OutputFile);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            try
            {
                Directory.CreateDirectory(outputDir);
            }
            catch
            {
                throw new InvalidOperationException($"Cannot create output directory: {outputDir}");
            }
        }
    }

    /// <summary>
    /// Clones the operation.
    /// </summary>
    public FFmpegOperation Clone()
    {
        return new FFmpegOperation
        {
            Id = Id,
            Name = Name,
            Type = Type,
            InputFiles = new List<string>(InputFiles),
            OutputFile = OutputFile,
            Arguments = new List<string>(Arguments),
            Timeout = Timeout,
            Priority = Priority,
            IsParallel = IsParallel,
            CustomProperties = new Dictionary<string, string>(CustomProperties),
            CreatedAt = CreatedAt,
            ExecutedAt = ExecutedAt
        };
    }

    /// <summary>
    /// Gets a detailed description of the operation.
    /// </summary>
    public string GetDescription()
    {
        return $"{Type} - Input: {string.Join(", ", InputFiles)} -> Output: {OutputFile}";
    }
}

/// <summary>
/// Enumeration of FFmpeg operation types.
/// </summary>
public enum FFmpegOperationType
{
    Transcode,
    Trim,
    Merge,
    Watermark,
    Demux,
    Mux,
    Analyze,
    Filter,
    Custom
}
