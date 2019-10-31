# Getting Started with FFmpeg .NET Wrapper

This guide will help you install and start using FFmpeg .NET Wrapper in your .NET 10 application.

## Prerequisites

### System Requirements

- **.NET 10 Runtime** or **SDK 10.0+**
  - Download: https://dotnet.microsoft.com/download/dotnet/10.0
  - Verify: `dotnet --version`

- **FFmpeg** (latest stable)
  - Must be installed and available in system PATH
  - Verify: `ffmpeg -version` from command line

### Installation by Platform

#### macOS (Homebrew)

```bash
brew install ffmpeg

# Verify installation
ffmpeg -version
```

#### Ubuntu/Debian Linux

```bash
sudo apt-get update
sudo apt-get install ffmpeg

# Verify installation
ffmpeg -version
```

#### Windows

**Option 1: Chocolatey**
```powershell
choco install ffmpeg

# Verify installation
ffmpeg -version
```

**Option 2: Manual Download**
1. Visit https://ffmpeg.org/download.html
2. Download Windows build
3. Extract to `C:\ffmpeg`
4. Add `C:\ffmpeg\bin` to system PATH
5. Restart terminal and verify: `ffmpeg -version`

#### Docker

The provided `Dockerfile` includes FFmpeg. See [deployment.md](deployment.md).

## Installation Steps

### Step 1: Create a New Project

```bash
dotnet new console -n MyFFmpegApp
cd MyFFmpegApp
```

### Step 2: Install FFmpeg Wrapper

Via NuGet Package Manager:
```bash
dotnet add package FFmpegDotnetWrapper
```

Via Package Manager Console:
```powershell
Install-Package FFmpegDotnetWrapper
```

Via Manual Source:
```bash
git clone https://github.com/vladyslav-zaiets/ffmpeg-dotnet-wrapper.git
cd ffmpeg-dotnet-wrapper
dotnet pack
# Copy the .nupkg to your NuGet cache or project
```

### Step 3: Verify Installation

Create `Program.cs`:

```csharp
using FFmpegDotnetWrapper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddLogging(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Information));

services.AddFFmpegWrapper(options =>
{
    options.DefaultTimeout = TimeSpan.FromSeconds(600);
    options.EnableDetailedLogging = true;
});

var serviceProvider = services.BuildServiceProvider();
var ffmpeg = serviceProvider.GetRequiredService<IFFmpegService>();

var available = await ffmpeg.IsFFmpegAvailableAsync();
Console.WriteLine(available ? "✓ FFmpeg is ready!" : "✗ FFmpeg not found");

var version = await ffmpeg.GetFFmpegVersionAsync();
Console.WriteLine($"FFmpeg: {version}");
```

Run it:
```bash
dotnet run
```

Expected output:
```
✓ FFmpeg is ready!
FFmpeg: ffmpeg version 7.0...
```

## Basic Workflow

### 1. Setup Dependency Injection

```csharp
using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Register services
var services = new ServiceCollection();

services.AddLogging(builder =>
    builder.AddConsole()
           .SetMinimumLevel(LogLevel.Information));

services.AddFFmpegWrapper(options =>
{
    options.DefaultTimeout = TimeSpan.FromSeconds(600);
    options.EnableDetailedLogging = true;
    options.MaxConcurrentOperations = 4;
});

var serviceProvider = services.BuildServiceProvider();
```

### 2. Get the Service

```csharp
var ffmpeg = serviceProvider.GetRequiredService<IFFmpegService>();
```

### 3: Perform Operations

```csharp
// Example: Transcode to WebM
var result = await ffmpeg.TranscodeAsync(
    inputPath: "input.mp4",
    outputPath: "output.webm",
    settings: new TranscodeSettings
    {
        VideoCodec = VideoCodec.VP9,
        AudioCodec = AudioCodec.Opus,
        Container = ContainerFormat.WebM,
        MaxWidth = 1280,
        MaxHeight = 720,
        Quality = QualityPreset.Medium
    });

if (result.Success)
    Console.WriteLine($"✓ Completed in {result.ElapsedTime.TotalSeconds}s");
else
    Console.WriteLine($"✗ Error: {result.ErrorMessage}");
```

## Common Use Cases

### Use Case 1: Simple Transcode

Convert MP4 to WebM for web delivery:

```csharp
var settings = new TranscodeSettings
{
    VideoCodec = VideoCodec.VP9,
    AudioCodec = AudioCodec.Opus,
    Container = ContainerFormat.WebM,
    VideoBitrate = 1500,
    AudioBitrate = 96,
    MaxWidth = 1280,
    MaxHeight = 720,
    FrameRate = 30
};

await ffmpeg.TranscodeAsync("video.mp4", "video.webm", settings);
```

### Use Case 2: Extract a Clip

Get a 30-second segment starting at 2 minutes:

```csharp
var settings = new TrimSettings
{
    StartTime = TimeSpan.FromMinutes(2),
    Duration = TimeSpan.FromSeconds(30)
};

await ffmpeg.TrimAsync("full.mp4", "clip.mp4", settings);
```

### Use Case 3: Batch Processing

Process multiple files with progress:

```csharp
var batchService = serviceProvider.GetRequiredService<BatchOperationService>();

var progress = new Progress<OperationStatistics>(stat =>
{
    Console.WriteLine($"Progress: {stat.CompletedOperations}/{stat.TotalOperations}");
});

var files = Directory.GetFiles("input/", "*.mp4");
await batchService.ProcessFilesAsync(files, "output/", settings, progress);
```

## Advanced Configuration

### Custom FFmpeg Path

Some systems might have FFmpeg installed in a non-standard location:

```csharp
services.AddFFmpegWrapper(options =>
{
    options.FFmpegPath = "/usr/local/custom/ffmpeg";
    // or on Windows
    // options.FFmpegPath = @"C:\Tools\ffmpeg\ffmpeg.exe";
});
```

### Increase Timeout for Large Files

Large video files need more processing time:

```csharp
services.AddFFmpegWrapper(options =>
{
    options.DefaultTimeout = TimeSpan.FromMinutes(30);
});
```

### Enable Detailed Logging

Debug problems with detailed output:

```csharp
services.AddLogging(builder =>
    builder.AddConsole()
           .SetMinimumLevel(LogLevel.Debug)
           .AddFilter("FFmpegDotnetWrapper", LogLevel.Debug));

services.AddFFmpegWrapper(options =>
{
    options.EnableDetailedLogging = true;
});
```

### Configure via appsettings.json

```json
{
  "FFmpegOptions": {
    "DefaultTimeout": "00:10:00",
    "EnableDetailedLogging": false,
    "MaxConcurrentOperations": 4,
    "FFmpegPath": "/usr/bin/ffmpeg"
  }
}
```

Then in code:

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var options = config.GetSection("FFmpegOptions").Get<FFmpegOptions>();
services.AddFFmpegWrapper(options);
```

## Testing Your Setup

### Verify FFmpeg Works

```bash
# List available encoders
ffmpeg -encoders | grep -E "h264|vp9|opus"

# Check input file
ffmpeg -i input.mp4 2>&1 | head -20
```

### Test the Wrapper

```csharp
// Comprehensive test
var ffmpeg = serviceProvider.GetRequiredService<IFFmpegService>();

// 1. Check availability
var available = await ffmpeg.IsFFmpegAvailableAsync();
Console.WriteLine($"Available: {available}");

// 2. Get version
var version = await ffmpeg.GetFFmpegVersionAsync();
Console.WriteLine($"Version: {version}");

// 3. Analyze a file
var media = new MediaFile { Path = "test.mp4" };
var props = await ffmpeg.AnalyzeMediaAsync(media);
Console.WriteLine($"Duration: {props.Duration}");
Console.WriteLine($"Resolution: {props.Width}x{props.Height}");
```

## Troubleshooting

### Issue: "FFmpeg is not installed"

```
FFmpegException: FFmpeg executable not found
```

**Solution**: Ensure FFmpeg is installed and in PATH:
```bash
which ffmpeg          # On macOS/Linux
where ffmpeg          # On Windows
```

If not in PATH, specify explicitly:
```csharp
options.FFmpegPath = "/absolute/path/to/ffmpeg";
```

### Issue: "Permission denied"

```
UnauthorizedAccessException: Access to the file is denied
```

**Solution**: Check file and directory permissions:
```bash
chmod +x /path/to/ffmpeg
chmod 755 /input/directory
chmod 755 /output/directory
```

### Issue: Codec not found

```
Unknown encoder 'vp9'
```

**Solution**: Your FFmpeg build doesn't include that codec. Check what's available:
```bash
ffmpeg -encoders
```

Use a different codec or recompile FFmpeg with support.

### Issue: Out of memory

```
Cannot allocate memory
```

**Solution**: Reduce concurrent operations:
```csharp
options.MaxConcurrentOperations = 1;
```

Or optimize input file size.

## Next Steps

- Read [API Reference](api-reference.md) for complete method documentation
- Check [examples/](../examples/) for sample code
- Review [deployment.md](deployment.md) for production setup
- See [faq.md](faq.md) for common questions
