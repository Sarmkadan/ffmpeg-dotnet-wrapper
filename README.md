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

Copyright © 2025 Vladyslav Zaiets

Copyright © 2025 Vladyslav Zaiets
