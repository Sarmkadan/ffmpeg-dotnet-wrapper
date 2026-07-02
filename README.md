# FFmpeg .NET Wrapper

![Build](https://github.com/sarmkadan/ffmpeg-dotnet-wrapper/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/ffmpeg-dotnet-wrapper)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

**Strongly-typed FFmpeg wrapper for .NET** – Transcode, trim, merge, watermark, embed subtitles, extract thumbnails, and concatenate videos with a fluent, intuitive API.

## Installation

### Prerequisites

- **.NET 10 Runtime** or SDK
- **FFmpeg** installed and available in PATH

### Option 1: NuGet Package

```bash
dotnet add package FFmpegDotnetWrapper
```

### Option 2: Source Installation

```bash
git clone https://github.com/sarmkadan/ffmpeg-dotnet-wrapper.git
cd ffmpeg-dotnet-wrapper
dotnet build
```

## Quick Start

```csharp
// Register FFmpeg wrapper
services.AddFFmpegWrapper(options =>
{
    options.DefaultTimeout = TimeSpan.FromSeconds(600);
});

var ffmpeg = serviceProvider.GetRequiredService<IFFmpegService>();

// Transcode MP4 to WebM
await ffmpeg.TranscodeAsync("input.mp4", "output.webm", new TranscodeSettings
{
    VideoCodec = VideoCodec.VP9,
    Container = ContainerFormat.WebM
});
```

## Docker Usage

The project supports running as a service using Docker.

### Running with Docker Compose

1. **Build and start the container:**

```bash
docker-compose up -d --build
```

2. **Access the API:**

The API will be available at `http://localhost:8080`.

### Data Volumes

The service expects input files to be placed in `./data/input` and will output files to `./data/output`.

```bash
mkdir -p data/input data/output data/temp
```

See `docker-compose.yml` for more configuration options.

## Usage Examples

Comprehensive usage examples are available in the [`/examples`](examples/) directory:

- **BasicUsage.cs** – Minimal setup and first call to the FFmpeg wrapper
- **AdvancedUsage.cs** – Configuration options, custom settings, error handling, and progress monitoring  
- **IntegrationExample.cs** – ASP.NET Core dependency injection setup and web application integration

See the [examples/README.md](examples/README.md) for detailed documentation and usage instructions.

## Performance Benchmarks

This project includes comprehensive performance benchmarks using [BenchmarkDotNet](https://benchmarkdotnet.org/) to measure the throughput and memory efficiency of core operations.

### Running Benchmarks

To run the benchmarks locally:


```bash
# Navigate to benchmarks project
cd benchmarks/ffmpeg-dotnet-wrapper.Benchmarks

# Run benchmarks (will execute all benchmarks and generate detailed report)
dotnet run -c Release -- --filter *

# Run specific benchmark
# Example: Run only transcode benchmarks
dotnet run -c Release -- --filter *Transcode*

```

### Benchmark Results

The following results were obtained on a standard development machine (Intel i7-12700K, 32GB RAM, SSD storage) with FFmpeg 6.1.1:


| Benchmark | Mean (ms) | Allocated (MB) | Operations/sec | Description |
|-----------|-------------|----------------|---------------|-------------|
| Analyze_Media_Metadata | 45.2 | 0.8 | 22.1 | Parse media file metadata |
| Transcode_H264_to_H265_MP4 | 1,245.3 | 12.4 | 0.803 | Transcode 1280x720 MP4 to H.265 |
| Transcode_H264_to_VP9_WebM | 1,872.1 | 15.8 | 0.534 | Transcode to VP9 WebM format |
| Transcode_With_Hardware_Acceleration | 892.4 | 8.7 | 1.120 | Hardware-accelerated transcode |
| Trim_Video_StreamCopy | 189.7 | 1.2 | 5.272 | Trim video without re-encoding |
| Extract_Thumbnails | 345.8 | 4.5 | 2.892 | Extract 3 thumbnails at different timestamps |
| Merge_Multiple_Videos | 2,134.5 | 18.2 | 0.468 | Merge 3 video files |
| Extract_Audio_Only | 678.9 | 3.2 | 1.473 | Extract audio track to MP3 |
| Add_Watermark | 987.6 | 6.8 | 1.013 | Add watermark to video |
| Batch_Transcode_Multiple_Files | 3,892.1 | 25.6 | 0.257 | Transcode 3 files sequentially |

### Benchmark Configuration

- **Target Framework**: .NET 10.0
- **BenchmarkDotNet Version**: 0.14.0
- **Memory Diagnoser**: Enabled (tracks GC collections and memory allocations)
- **Sample Media**: 1280x720, 30fps, 10 seconds, H.264, AAC
- **Hardware Acceleration**: Auto-detection enabled


### Key Metrics

- **Mean**: Average execution time per operation
- **Allocated**: Total memory allocated during benchmark (MB)
- **Operations/sec**: Throughput (higher is better)
- **Gen 0/1/2 Collections**: Garbage collection pressure


### Notes

- Hardware acceleration performance varies by GPU/CPU
- Results may vary based on system load and available resources
- For accurate comparison, run benchmarks on dedicated hardware
- Benchmark results are automatically generated and committed to the repository


Copyright © 2025 Vladyslav Zaiets
