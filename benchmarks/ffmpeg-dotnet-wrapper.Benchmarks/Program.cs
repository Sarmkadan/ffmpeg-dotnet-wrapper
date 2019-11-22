using BenchmarkDotNet.Running;
using FFmpegDotnetWrapper.Benchmarks;

var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
