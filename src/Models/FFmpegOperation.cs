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
        ArgumentNullException.ThrowIfNull(arguments);
        foreach (var arg in arguments)
            AddArgument(arg);
    }

    /// <summary>
    /// Builds the complete FFmpeg command line. Input and output paths are wrapped in double quotes;
    /// an output path starting with <c>-</c> is anchored with <c>./</c> so ffmpeg does not read it as an option.
    /// </summary>
    /// <returns>The command line string passed to the process runner.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when an input or output path is null, empty, or whitespace; contains a double quote, NUL, or line break;
    /// ends with a backslash; or is a bare <c>-</c> (stdin for inputs, stdout for outputs).
    /// </exception>
    public string BuildCommandLine()
    {
        var cmd = new System.Text.StringBuilder("ffmpeg");

        // Add input files
        foreach (var input in InputFiles)
        {
            cmd.Append($" -i {QuoteInputPath(input)}");
        }

        // Add arguments
        foreach (var arg in Arguments)
        {
            cmd.Append($" {arg}");
        }

        // Add output file
        cmd.Append($" {QuoteOutputPath(OutputFile)}");

        return cmd.ToString();
    }

    /// <summary>
    /// Path that ffmpeg interprets as standard input (for inputs) or standard output (for outputs).
    /// </summary>
    private const string StdioPath = "-";

    /// <summary>
    /// Characters that cannot appear inside a quoted path on the command line.
    /// </summary>
    private static readonly char[] UnquotablePathChars = ['"', '\0', '\r', '\n'];

    /// <summary>
    /// Validates and quotes an input path for the <c>-i</c> option.
    /// </summary>
    private static string QuoteInputPath(string path)
    {
        ValidateQuotablePath(path, nameof(InputFiles));

        if (path == StdioPath)
            throw new ArgumentException("ffmpeg would read from standard input, which the process runner never closes.", nameof(InputFiles));

        return Quote(path);
    }

    /// <summary>
    /// Validates and quotes the output path. A leading dash is anchored with <c>./</c> so it is treated as a path.
    /// </summary>
    private static string QuoteOutputPath(string path)
    {
        ValidateQuotablePath(path, nameof(OutputFile));

        if (path == StdioPath)
            throw new ArgumentException("ffmpeg would write to standard output, which carries progress data.", nameof(OutputFile));

        return Quote(path.StartsWith('-') ? $"./{path}" : path);
    }

    /// <summary>
    /// Ensures a path can be wrapped in double quotes without the quoting being broken out of.
    /// </summary>
    private static void ValidateQuotablePath(string path, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, paramName);

        if (path.AsSpan().IndexOfAny(UnquotablePathChars) >= 0)
            throw new ArgumentException("Path contains a double quote, NUL, or line break and cannot be passed safely to ffmpeg.", paramName);

        // A trailing backslash would be read as an escape for the closing quote on Windows-style argument parsing.
        if (path[^1] == '\\')
            throw new ArgumentException("Path ends with a backslash, which would escape its closing quote.", paramName);
    }

    private static string Quote(string path) => $"\"{path}\"";

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

    /// <summary>
    /// Returns a string representation of the FFmpegOperation.
    /// </summary>
    public override string ToString()
    {
        return $"{Type} (Id={Id}, Inputs={InputFiles.Count}, Output={OutputFile})";
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
