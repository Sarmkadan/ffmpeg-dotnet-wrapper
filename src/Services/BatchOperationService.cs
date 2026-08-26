// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Service for handling batch operations and concurrent processing.
/// </summary>
public class BatchOperationService
{
    private readonly IFFmpegService _ffmpegService;
    private readonly ILogger<BatchOperationService> _logger;

    public BatchOperationService(IFFmpegService ffmpegService, ILogger<BatchOperationService> logger)
    {
        _ffmpegService = ffmpegService ?? throw new ArgumentNullException(nameof(ffmpegService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes multiple files concurrently with a specified transcode settings.
    /// </summary>
    public async Task<BatchOperationResult> TranscodeMultipleAsync(
        IEnumerable<string> inputFiles,
        string outputDirectory,
        TranscodeSettings settings,
        int maxConcurrency = 2,
        CancellationToken cancellationToken = default)
    {
        var files = inputFiles.ToList();
        var result = new BatchOperationResult
        {
            TotalFiles = files.Count,
            OperationType = "Transcode"
        };

        _logger.LogInformation("Starting batch transcode of {Count} files with max concurrency {Concurrency}",
            files.Count, maxConcurrency);

        Directory.CreateDirectory(outputDirectory);

        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var tasks = new List<Task>();

        foreach (var inputFile in files)
        {
            await semaphore.WaitAsync(cancellationToken);
            tasks.Add(ProcessFileAsync(inputFile, outputDirectory, settings, semaphore, result, cancellationToken));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Batch operation was cancelled");
            result.IsCancelled = true;
        }

        result.CompletedAt = DateTime.UtcNow;
        _logger.LogInformation(
            "Batch transcode completed: {Successful}/{Total} successful",
            result.SuccessfulCount,
            result.TotalFiles);

        return result;
    }

    /// <summary>
    /// Processes multiple files for analysis in parallel.
    /// </summary>
    public async Task<BatchAnalysisResult> AnalyzeMultipleAsync(
        IEnumerable<string> filePaths,
        int maxConcurrency = 4,
        CancellationToken cancellationToken = default)
    {
        var files = filePaths.ToList();
        var result = new BatchAnalysisResult { TotalFiles = files.Count };

        _logger.LogInformation("Starting batch analysis of {Count} files", files.Count);

        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var tasks = new ConcurrentBag<Task>();

        foreach (var filePath in files)
        {
            await semaphore.WaitAsync(cancellationToken);
            tasks.Add(AnalyzeFileAsync(filePath, result, semaphore, cancellationToken));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Batch analysis was cancelled");
            result.IsCancelled = true;
        }

        result.CompletedAt = DateTime.UtcNow;
        _logger.LogInformation("Batch analysis completed: {Count} files analyzed", result.AnalyzedFiles.Count);

        return result;
    }

    /// <summary>
    /// Processes files with a custom transformation function.
    /// </summary>
    public async Task<BatchOperationResult> ProcessWithCustomFunctionAsync(
        IEnumerable<string> inputFiles,
        string outputDirectory,
        Func<string, string, CancellationToken, Task<ConversionResult>> processFunc,
        int maxConcurrency = 2,
        CancellationToken cancellationToken = default)
    {
        var files = inputFiles.ToList();
        var result = new BatchOperationResult
        {
            TotalFiles = files.Count,
            OperationType = "Custom"
        };

        _logger.LogInformation("Starting batch custom processing of {Count} files", files.Count);

        Directory.CreateDirectory(outputDirectory);

        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        var tasks = files.Select(async inputFile =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var outputFile = Path.Combine(outputDirectory, Path.GetFileName(inputFile));
                var operationResult = await processFunc(inputFile, outputFile, cancellationToken);

                lock (result.Results)
                {
                    result.Results.Add(operationResult);
                    if (operationResult.IsSuccess)
                        result.SuccessfulCount++;
                    else
                        result.FailedCount++;
                }

                _logger.LogInformation("Processed file: {File} - {Status}",
                    Path.GetFileName(inputFile),
                    operationResult.IsSuccess ? "Success" : "Failed");
            }
            finally
            {
                semaphore.Release();
            }
        });

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            result.IsCancelled = true;
        }

        result.CompletedAt = DateTime.UtcNow;
        return result;
    }

    private async Task ProcessFileAsync(
        string inputFile,
        string outputDirectory,
        TranscodeSettings settings,
        SemaphoreSlim semaphore,
        BatchOperationResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            var fileName = Path.GetFileName(inputFile);
            var outputFile = Path.Combine(outputDirectory, fileName);

            _logger.LogDebug("Processing file: {File}", fileName);

            var mediaFile = new MediaFile(inputFile);
            var conversionResult = await _ffmpegService.TranscodeAsync(
                mediaFile, outputFile, settings, cancellationToken);

            lock (result.Results)
            {
                result.Results.Add(conversionResult);
                if (conversionResult.IsSuccess)
                {
                    result.SuccessfulCount++;
                }
                else
                {
                    result.FailedCount++;
                }
            }

            _logger.LogInformation("Completed file: {File} - {Status}",
                fileName,
                conversionResult.IsSuccess ? "Success" : "Failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file: {File}", inputFile);
            Interlocked.Increment(ref result.FailedCount);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task AnalyzeFileAsync(
        string filePath,
        BatchAnalysisResult result,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            _logger.LogDebug("Analyzing file: {File}", fileName);

            var mediaFile = await _ffmpegService.AnalyzeMediaAsync(filePath, cancellationToken);

            lock (result.AnalyzedFiles)
            {
                result.AnalyzedFiles.Add(mediaFile);
            }

            _logger.LogInformation("Analyzed file: {File}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing file: {File}", filePath);
        }
        finally
        {
            semaphore.Release();
        }
    }
}

/// <summary>
/// Result of batch operation processing.
/// </summary>
public class BatchOperationResult
{
    public string OperationType { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<ConversionResult> Results { get; set; } = new();

    public TimeSpan GetDuration() => CompletedAt.HasValue ? CompletedAt.Value - CreatedAt : TimeSpan.Zero;
    public double GetSuccessRate() => TotalFiles > 0 ? (SuccessfulCount / (double)TotalFiles) * 100 : 0;

    public override string ToString() =>
        $"BatchOperationResult {{ OperationType = {OperationType}, TotalFiles = {TotalFiles}, SuccessfulCount = {SuccessfulCount}, FailedCount = {FailedCount}, IsCancelled = {IsCancelled}, CreatedAt = {CreatedAt} }}";
}

/// <summary>
/// Result of batch analysis processing.
/// </summary>
public class BatchAnalysisResult
{
    public int TotalFiles { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<MediaFile> AnalyzedFiles { get; set; } = new();

    public TimeSpan GetDuration() => CompletedAt.HasValue ? CompletedAt.Value - CreatedAt : TimeSpan.Zero;
}
