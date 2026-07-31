using BenchmarkDotNet.Attributes;
using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Benchmarks;

[MemoryDiagnoser]
public class ValidationExceptionBenchmarks
{
    private Dictionary<string, string[]>? _largeErrors;
    private Dictionary<string, string[]>? _smallErrors;

    [Params(10, 100, 1000)]
    public int ErrorCount;

    [GlobalSetup]
    public void Setup()
    {
        _smallErrors = new Dictionary<string, string[]>
        {
            { "field1", new[] { "Error 1" } },
            { "field2", new[] { "Error 2" } }
        };

        _largeErrors = new Dictionary<string, string[]>();
        for (int i = 0; i < ErrorCount; i++)
        {
            _largeErrors.Add($"field{i}", new[] { $"Error {i}", $"Additional Error {i}" });
        }
    }

    [Benchmark]
    public ValidationException CreateWithErrors()
    {
        return new ValidationException("Test message", _largeErrors!);
    }

    [Benchmark]
    public ValidationException FromDictionarySmall()
    {
        return ValidationException.FromDictionary(_smallErrors!, "Validation failed");
    }

    [Benchmark]
    public ValidationException FromDictionaryLarge()
    {
        return ValidationException.FromDictionary(_largeErrors!, "Validation failed");
    }
}
