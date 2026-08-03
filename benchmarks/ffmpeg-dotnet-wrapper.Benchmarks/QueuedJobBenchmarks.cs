using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using FFmpegDotnetWrapper.BackgroundJobs;

namespace FFmpegDotnetWrapper.Benchmarks
{
    [MemoryDiagnoser]
    public class QueuedJobBenchmarks
    {
        private QueuedJob _job = null!;

        [Params(10, 100, 1000)]
        public int PayloadSize;

        [Params(0, 5, 10)]
        public int TagCount;

        [GlobalSetup]
        public void Setup()
        {
            _job = new QueuedJob();
        }

        [Benchmark]
        public void SetPayload()
        {
            _job.Payload = new string('x', PayloadSize);
        }

        [Benchmark]
        public void SetTags()
        {
            _job.Tags = Enumerable.Range(0, TagCount)
                .ToDictionary(i => $"Tag{i}", i => $"Value{i}");
        }

        [Benchmark]
        public void SetPayloadAndTags()
        {
            _job.Payload = new string('x', PayloadSize);
            _job.Tags = Enumerable.Range(0, TagCount)
                .ToDictionary(i => $"Tag{i}", i => $"Value{i}");
        }
    }
}