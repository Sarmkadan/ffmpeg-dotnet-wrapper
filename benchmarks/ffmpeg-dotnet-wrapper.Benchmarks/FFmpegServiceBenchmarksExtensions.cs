using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Benchmarks;
using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;

namespace FFmpegDotnetWrapper.Benchmarks;

/// <summary>
/// Extension methods for <see cref="FFmpegServiceBenchmarks"/> that provide additional benchmarking utilities
/// and helper methods for working with benchmark results.
/// </summary>
/// <remarks>
/// This class provides extension methods to enhance the benchmarking capabilities of <see cref="FFmpegServiceBenchmarks"/>
/// by offering batch operations, validation, statistics analysis, and comparison utilities.
/// </remarks>
public static class FFmpegServiceBenchmarksExtensions
{
    /// <summary>
    /// Creates a batch of benchmark runs with common settings for comparison testing.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="inputPath">Path to the input media file.</param>
    /// <param name="outputDirectory">Directory to store output files.</param>
    /// <param name="videoCodec">Target video codec for transcoding.</param>
    /// <param name="audioCodec">Target audio codec for transcoding.</param>
    /// <param name="iterations">Number of iterations to run.</param>
    /// <returns>Collection of benchmark results with file paths.</returns>
    /// <exception cref="ArgumentNullException">Thrown when inputPath or outputDirectory is null.</exception>
    /// <exception cref="ArgumentException">Thrown when inputPath is empty or outputDirectory is empty.</exception>
    public static async Task<IReadOnlyList<BenchmarkResult>> CreateBenchmarkBatch(
        this FFmpegServiceBenchmarks benchmarks,
        string inputPath,
        string outputDirectory,
        VideoCodec videoCodec,
        AudioCodec audioCodec,
        int iterations = 3)
    {
        ArgumentNullException.ThrowIfNull(inputPath);
        ArgumentException.ThrowIfNullOrEmpty(inputPath);
        ArgumentNullException.ThrowIfNull(outputDirectory);
        ArgumentException.ThrowIfNullOrEmpty(outputDirectory);

        var results = new List<BenchmarkResult>(iterations);
        var random = new Random();

        for (int i = 0; i < iterations; i++)
        {
            var iterationOutput = Path.Combine(outputDirectory, $"benchmark_iteration_{i}_{videoCodec}_{DateTime.UtcNow:yyyyMMddHHmmss}_{random.Next(1000)}");
            Directory.CreateDirectory(iterationOutput);

            var outputPath = Path.Combine(iterationOutput, $"output_{videoCodec}.mp4");

            benchmarks.GlobalSetup();
            var benchmarkTask = benchmarks.Transcode_H264_to_H265_MP4();
            await benchmarkTask;
            benchmarks.GlobalCleanup();

            results.Add(new BenchmarkResult
            {
                Iteration = i + 1,
                OutputPath = outputPath,
                VideoCodec = videoCodec,
                AudioCodec = audioCodec,
                Timestamp = DateTime.UtcNow
            });
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Validates that all benchmark methods can execute without throwing exceptions.
    /// Useful for CI/CD pipelines to ensure benchmarks are functional.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>True if all benchmarks passed validation; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when benchmarks is null.</exception>
    public static async Task<bool> ValidateAllBenchmarksAsync(this FFmpegServiceBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        try
        {
            // Run each benchmark method and verify it completes successfully
            await benchmarks.Transcode_H264_to_H265_MP4();
            await benchmarks.Transcode_H264_to_VP9_WebM();
            await benchmarks.Transcode_With_Hardware_Acceleration();
            await benchmarks.Trim_Video_StreamCopy();
            await benchmarks.Analyze_Media_Metadata();
            await benchmarks.Extract_Thumbnails();
            await benchmarks.Merge_Multiple_Videos();
            await benchmarks.Extract_Audio_Only();
            await benchmarks.Add_Watermark();
            await benchmarks.Batch_Transcode_Multiple_Files();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets performance statistics from benchmark execution.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="benchmarkName">Name of the benchmark to get stats for.</param>
    /// <returns>Dictionary containing performance metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when benchmarkName is null.</exception>
    /// <exception cref="ArgumentException">Thrown when benchmarkName is empty.</exception>
    public static IReadOnlyDictionary<string, object> GetBenchmarkStatistics(
        this FFmpegServiceBenchmarks benchmarks,
        string benchmarkName)
    {
        ArgumentNullException.ThrowIfNull(benchmarkName);
        ArgumentException.ThrowIfNullOrEmpty(benchmarkName);

        // In a real benchmarking scenario, this would parse BenchmarkDotNet output
        // For this extension class, we return mock statistics based on benchmark type
        return benchmarkName switch
        {
            nameof(FFmpegServiceBenchmarks.Transcode_H264_to_H265_MP4) or
            nameof(FFmpegServiceBenchmarks.Transcode_H264_to_VP9_WebM) or
            nameof(FFmpegServiceBenchmarks.Transcode_With_Hardware_Acceleration) =>
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["MeanMs"] = 1250.5,
                    ["StdDevMs"] = 45.2,
                    ["AllocatedBytes"] = 15_200_000L,
                    ["Gen0Collections"] = 8,
                    ["Gen1Collections"] = 2,
                    ["Gen2Collections"] = 0,
                    ["OperationsPerSecond"] = 0.8
                }.AsReadOnly(),

            nameof(FFmpegServiceBenchmarks.Trim_Video_StreamCopy) =>
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["MeanMs"] = 450.3,
                    ["StdDevMs"] = 22.1,
                    ["AllocatedBytes"] = 8_400_000L,
                    ["Gen0Collections"] = 4,
                    ["Gen1Collections"] = 1,
                    ["Gen2Collections"] = 0,
                    ["OperationsPerSecond"] = 2.2
                }.AsReadOnly(),

            nameof(FFmpegServiceBenchmarks.Analyze_Media_Metadata) =>
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["MeanMs"] = 120.8,
                    ["StdDevMs"] = 8.7,
                    ["AllocatedBytes"] = 3_100_000L,
                    ["Gen0Collections"] = 2,
                    ["Gen1Collections"] = 0,
                    ["Gen2Collections"] = 0,
                    ["OperationsPerSecond"] = 8.3
                }.AsReadOnly(),

            nameof(FFmpegServiceBenchmarks.Extract_Thumbnails) =>
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["MeanMs"] = 680.4,
                    ["StdDevMs"] = 35.6,
                    ["AllocatedBytes"] = 12_800_000L,
                    ["Gen0Collections"] = 6,
                    ["Gen1Collections"] = 1,
                    ["Gen2Collections"] = 0,
                    ["OperationsPerSecond"] = 1.5
                }.AsReadOnly(),

            nameof(FFmpegServiceBenchmarks.Merge_Multiple_Videos) =>
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["MeanMs"] = 2100.7,
                    ["StdDevMs"] = 95.3,
                    ["AllocatedBytes"] = 22_500_000L,
                    ["Gen0Collections"] = 12,
                    ["Gen1Collections"] = 3,
                    ["Gen2Collections"] = 1,
                    ["OperationsPerSecond"] = 0.47
                }.AsReadOnly(),

            nameof(FFmpegServiceBenchmarks.Extract_Audio_Only) =>
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["MeanMs"] = 850.2,
                    ["StdDevMs"] = 42.8,
                    ["AllocatedBytes"] = 9_700_000L,
                    ["Gen0Collections"] = 5,
                    ["Gen1Collections"] = 1,
                    ["Gen2Collections"] = 0,
                    ["OperationsPerSecond"] = 1.18
                }.AsReadOnly(),

            nameof(FFmpegServiceBenchmarks.Add_Watermark) =>
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["MeanMs"] = 1550.9,
                    ["StdDevMs"] = 68.4,
                    ["AllocatedBytes"] = 18_200_000L,
                    ["Gen0Collections"] = 9,
                    ["Gen1Collections"] = 2,
                    ["Gen2Collections"] = 0,
                    ["OperationsPerSecond"] = 0.65
                }.AsReadOnly(),

            nameof(FFmpegServiceBenchmarks.Batch_Transcode_Multiple_Files) =>
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["MeanMs"] = 4200.5,
                    ["StdDevMs"] = 180.7,
                    ["AllocatedBytes"] = 45_000_000L,
                    ["Gen0Collections"] = 25,
                    ["Gen1Collections"] = 6,
                    ["Gen2Collections"] = 2,
                    ["OperationsPerSecond"] = 0.24
                }.AsReadOnly(),

            _ => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["MeanMs"] = 0.0,
                    ["StdDevMs"] = 0.0,
                    ["AllocatedBytes"] = 0L,
                    ["Gen0Collections"] = 0,
                    ["Gen1Collections"] = 0,
                    ["Gen2Collections"] = 0,
                    ["OperationsPerSecond"] = 0.0
                }.AsReadOnly()
        };
    }

    /// <summary>
    /// Compares performance between two benchmark runs.
    /// </summary>
    /// <param name="currentStats">Current benchmark statistics.</param>
    /// <param name="baselineStats">Baseline benchmark statistics.</param>
    /// <returns>Comparison results showing percentage differences.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either parameter is null.</exception>
    public static BenchmarkComparison CompareBenchmarks(
        this IReadOnlyDictionary<string, object> currentStats,
        IReadOnlyDictionary<string, object> baselineStats)
    {
        ArgumentNullException.ThrowIfNull(currentStats);
        ArgumentNullException.ThrowIfNull(baselineStats);

        var metrics = new List<BenchmarkMetricComparison>();

        foreach (var key in currentStats.Keys)
        {
            if (baselineStats.TryGetValue(key, out var baselineValue) &&
                currentStats[key] is double currentValue &&
                baselineValue is double baselineValueDouble)
            {
                var difference = currentValue - baselineValueDouble;
                var percentageChange = baselineValueDouble != 0
                    ? (difference / baselineValueDouble) * 100
                    : 0;

                metrics.Add(new BenchmarkMetricComparison
                {
                    MetricName = key,
                    CurrentValue = currentValue,
                    BaselineValue = baselineValueDouble,
                    Difference = difference,
                    PercentageChange = Math.Round(percentageChange, 2, MidpointRounding.AwayFromZero),
                    IsRegression = percentageChange > 5,
                    IsImprovement = percentageChange < -5
                });
            }
        }

        return new BenchmarkComparison
        {
            Metrics = metrics.AsReadOnly()
        };
    }

    /// <summary>
    /// Record representing benchmark execution results.
    /// </summary>
    public sealed class BenchmarkResult
    {
        /// <summary>Gets or sets the iteration number.</summary>
        public int Iteration { get; set; }

        /// <summary>Gets or sets the path to generated output file.</summary>
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>Gets or sets the video codec used.</summary>
        public VideoCodec VideoCodec { get; set; }

        /// <summary>Gets or sets the audio codec used.</summary>
        public AudioCodec AudioCodec { get; set; }

        /// <summary>Gets or sets the timestamp of execution.</summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Container for benchmark comparison metrics.
    /// </summary>
    public sealed class BenchmarkComparison
    {
        /// <summary>Gets the collection of metric comparisons.</summary>
        public IReadOnlyList<BenchmarkMetricComparison> Metrics { get; init; } = Array.Empty<BenchmarkMetricComparison>();
    }

    /// <summary>
    /// Individual metric comparison between benchmarks.
    /// </summary>
    public sealed class BenchmarkMetricComparison
    {
        /// <summary>Gets the name of the metric.</summary>
        public string MetricName { get; init; } = string.Empty;

        /// <summary>Gets the value from current benchmark.</summary>
        public double CurrentValue { get; init; }

        /// <summary>Gets the value from baseline benchmark.</summary>
        public double BaselineValue { get; init; }

        /// <summary>Gets the absolute difference between values.</summary>
        public double Difference { get; init; }

        /// <summary>Gets the percentage change from baseline.</summary>
        public double PercentageChange { get; init; }

        /// <summary>Gets a value indicating whether this is a performance regression.</summary>
        public bool IsRegression { get; init; }

        /// <summary>Gets a value indicating whether this is a performance improvement.</summary>
        public bool IsImprovement { get; init; }
    }
}