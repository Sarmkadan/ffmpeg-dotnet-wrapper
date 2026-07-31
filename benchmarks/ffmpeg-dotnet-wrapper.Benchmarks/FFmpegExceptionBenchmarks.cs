using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Benchmarks
{
    [MemoryDiagnoser]
    public class FFmpegExceptionBenchmarks
    {
        private string _message;
        private string _errorOutput;
        private List<KeyValuePair<string, string>> _contextEntries;

        [Params(10, 100, 1000)]
        public int ContextSize;

        [GlobalSetup]
        public void Setup()
        {
            _message = "Test exception message";
            _errorOutput = "Test error output";
            _contextEntries = new List<KeyValuePair<string, string>>(ContextSize);
            for (int i = 0; i < ContextSize; i++)
            {
                _contextEntries.Add(new KeyValuePair<string, string>($"key{i}", $"value{i}"));
            }
        }

        [Benchmark]
        public FFmpegException ConstructWithMessage()
        {
            return new FFmpegException(_message);
        }

        [Benchmark]
        public FFmpegException ConstructWithExitCode()
        {
            return new FFmpegException(_message, 1, _errorOutput);
        }

        [Benchmark]
        public FFmpegException AddContextEntries()
        {
            var ex = new FFmpegException(_message);
            foreach (var kv in _contextEntries)
            {
                ex.Context.Add(kv.Key, kv.Value);
            }
            return ex;
        }

        [Benchmark]
        public string BuildContextString()
        {
            var ex = new FFmpegException(_message);
            foreach (var kv in _contextEntries)
            {
                ex.Context.Add(kv.Key, kv.Value);
            }
            // Build a string representation of the context dictionary
            return string.Join(", ", ex.Context.Select(kv => $"{kv.Key}={kv.Value}"));
        }
    }
}
