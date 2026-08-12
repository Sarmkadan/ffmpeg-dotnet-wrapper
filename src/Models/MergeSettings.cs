// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Configuration settings for merging/concatenating media files.
/// </summary>
public class MergeSettings
{
    private List<string> _inputFiles = new();

    public List<string> InputFiles
    {
        get => _inputFiles;
        set
        {
            if (value == null || value.Count == 0)
                throw new InvalidOperationConfigurationException("At least one input file is required");
            _inputFiles = value;
        }
    }

    public bool PreserveAudio { get; set; } = true;
    public bool PreserveVideo { get; set; } = true;
    public bool TranscodeOnMerge { get; set; } = false;
    public TranscodeSettings? TranscodeSettings { get; set; }
    public bool Crossfade { get; set; } = false;
    public double CrossfadeDuration { get; set; } = 1.0; // seconds

    public override string ToString() => $"MergeSettings {{ PreserveAudio = {PreserveAudio}, PreserveVideo = {PreserveVideo}, TranscodeOnMerge = {TranscodeOnMerge}, TranscodeSettings = {TranscodeSettings}, Crossfade = {Crossfade}, CrossfadeDuration = {CrossfadeDuration} }}";

    /// <summary>
    /// Adds an input file to the merge list.
    /// </summary>
    public void AddInputFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new InvalidOperationConfigurationException("File path cannot be null or empty");

        if (!File.Exists(filePath))
            throw new InvalidOperationConfigurationException($"File does not exist: {filePath}");

        _inputFiles.Add(filePath);
    }

    /// <summary>
    /// Removes an input file from the merge list.
    /// </summary>
    public void RemoveInputFile(string filePath)
    {
        _inputFiles.Remove(filePath);
    }

    /// <summary>
    /// Validates the merge settings for consistency.
    /// </summary>
    public void Validate()
    {
        if (InputFiles.Count < 2)
            throw new InvalidOperationConfigurationException("At least two input files are required for merging");

        foreach (var file in InputFiles)
        {
            if (!File.Exists(file))
                throw new InvalidOperationConfigurationException($"Input file does not exist: {file}");
        }

        if (!PreserveAudio && !PreserveVideo)
            throw new InvalidOperationConfigurationException("At least audio or video must be preserved");

        if (TranscodeOnMerge && TranscodeSettings == null)
            throw new InvalidOperationConfigurationException("TranscodeSettings is required when TranscodeOnMerge is enabled");

        if (Crossfade && CrossfadeDuration <= 0)
            throw new InvalidOperationConfigurationException("Crossfade duration must be greater than zero");

        TranscodeSettings?.Validate();
    }

    /// <summary>
    /// Gets the total number of input files.
    /// </summary>
    public int GetInputFileCount() => InputFiles.Count;

    /// <summary>
    /// Clears all input files.
    /// </summary>
    public void ClearInputFiles() => InputFiles.Clear();

    /// <summary>
    /// Creates a clone of the current settings.
    /// </summary>
    public MergeSettings Clone()
    {
        return new MergeSettings
        {
            InputFiles = new List<string>(InputFiles),
            PreserveAudio = PreserveAudio,
            PreserveVideo = PreserveVideo,
            TranscodeOnMerge = TranscodeOnMerge,
            TranscodeSettings = TranscodeSettings?.Clone(),
            Crossfade = Crossfade,
            CrossfadeDuration = CrossfadeDuration
        };
    }
}
