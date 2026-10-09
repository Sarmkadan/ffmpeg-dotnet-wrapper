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
    // Constants for default values
    private const bool DefaultPreserveAudio = true;
    private const bool DefaultPreserveVideo = true;
    private const bool DefaultTranscodeOnMerge = false;
    private const double DefaultCrossfadeDuration = 1.0; // seconds

    // Constants for validation messages
    private const string AtLeastOneInputFileRequired = "At least one input file is required";
    private const string FilePathCannotBeNullOrEmpty = "File path cannot be null or empty";
    private const string FileDoesNotExistFormat = "File does not exist: {0}";
    private const string AtLeastTwoInputFilesRequired = "At least two input files are required for merging";
    private const string InputFileDoesNotExistFormat = "Input file does not exist: {0}";
    private const string AtLeastAudioOrVideoMustBePreserved = "At least audio or video must be preserved";
    private const string TranscodeSettingsRequiredWhenEnabled = "TranscodeSettings is required when TranscodeOnMerge is enabled";
    private const string CrossfadeDurationMustBeGreaterThanZero = "Crossfade duration must be greater than zero";

    private List<string> _inputFiles = new();

    public List<string> InputFiles
    {
        get => _inputFiles;
        set
        {
            if (value == null || value.Count == 0)
                throw new InvalidOperationConfigurationException(AtLeastOneInputFileRequired);
            _inputFiles = value;
        }
    }

    public bool PreserveAudio { get; set; } = DefaultPreserveAudio;
    public bool PreserveVideo { get; set; } = DefaultPreserveVideo;
    public bool TranscodeOnMerge { get; set; } = DefaultTranscodeOnMerge;
    public TranscodeSettings? TranscodeSettings { get; set; }
    public bool Crossfade { get; set; } = false;
    public double CrossfadeDuration { get; set; } = DefaultCrossfadeDuration;

    public override string ToString() => $"MergeSettings {{ PreserveAudio = {PreserveAudio}, PreserveVideo = {PreserveVideo}, TranscodeOnMerge = {TranscodeOnMerge}, TranscodeSettings = {TranscodeSettings}, Crossfade = {Crossfade}, CrossfadeDuration = {CrossfadeDuration} }}";

    /// <summary>
    /// Adds an input file to the merge list.
    /// </summary>
    public void AddInputFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new InvalidOperationConfigurationException(FilePathCannotBeNullOrEmpty);

        if (!File.Exists(filePath))
            throw new InvalidOperationConfigurationException(string.Format(FileDoesNotExistFormat, filePath));

        _inputFiles.Add(filePath);
    }

    /// <summary>
    /// Removes an input file from the merge list.
    /// </summary>
    public void RemoveInputFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _inputFiles.Remove(filePath);
    }

    /// <summary>
    /// Validates the merge settings for consistency.
    /// </summary>
    public void Validate()
    {
        if (InputFiles.Count < 2)
            throw new InvalidOperationConfigurationException(AtLeastTwoInputFilesRequired);

        foreach (var file in InputFiles)
        {
            if (!File.Exists(file))
                throw new InvalidOperationConfigurationException(string.Format(InputFileDoesNotExistFormat, file));
        }

        if (!PreserveAudio && !PreserveVideo)
            throw new InvalidOperationConfigurationException(AtLeastAudioOrVideoMustBePreserved);

        if (TranscodeOnMerge && TranscodeSettings == null)
            throw new InvalidOperationConfigurationException(TranscodeSettingsRequiredWhenEnabled);

        if (Crossfade && CrossfadeDuration <= 0)
            throw new InvalidOperationConfigurationException(CrossfadeDurationMustBeGreaterThanZero);

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
