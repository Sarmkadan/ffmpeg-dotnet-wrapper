# FFmpeg .NET Wrapper Examples

Comprehensive examples demonstrating all major features of the FFmpeg .NET Wrapper library.

## Overview

Each example is a self-contained program that demonstrates a specific use case.

## Running Examples

### Prerequisites

- .NET 10 SDK
- FFmpeg installed and in PATH
- Input files (video files for processing)

### Basic Usage

```bash
# Build all examples
dotnet build

# Run specific example
dotnet run --project examples/01-basic-transcode.csproj -- input.mp4 output.webm

# Or with make
make examples
```

---

## Example Programs

### 1. Basic Transcode (01-basic-transcode.cs)

**Description**: Simple video transcoding from one format to another.

**Features**:
- H.264 → VP9 conversion
- Progress tracking with real-time updates
- Error handling and validation

**Usage**:
```bash
dotnet run --project examples/01-basic-transcode.csproj input.mp4 output.webm
```

**Output**:
```
FFmpeg 7.0.1 Copyright...
Starting transcode: input.mp4 -> output.webm
Progress: 45.2% | Elapsed: 12s | ETA: 15s
✓ Transcode completed successfully
Duration: 27.42s
Output file: output.webm (125.4 MB)
```

---

### 2. Batch Processing (02-batch-processing.cs)

**Description**: Process multiple video files concurrently with progress reporting.

**Features**:
- Parallel processing (4 files at once)
- Aggregate statistics
- Overall progress tracking
- File enumeration from directory

**Usage**:
```bash
# Process all MP4 files in input/ to output/
dotnet run --project examples/02-batch-processing.csproj input/ output/

# Custom directories
dotnet run --project examples/02-batch-processing.csproj /path/to/input /path/to/output
```

**Output**:
```
Found 12 files to process

╔════════════════════════════════════════╗
║     Batch Processing Progress          ║
╚════════════════════════════════════════╝
Completed:      3/12 files
Successful:     3 files
Failed:         0 files
Success Rate:   100.00%
Progress:       25.0%
Elapsed Time:   45s
Estimated ETA:  135s
[██████░░░░░░░░░░░░░░░░░░░░]
```

---

### 3. Video Trimming (03-video-trimming.cs)

**Description**: Extract a segment from a video file.

**Features**:
- Keyframe-aligned trimming
- Duration validation
- Media analysis before trimming
- Segment extraction

**Usage**:
```bash
# Extract 60-second segment starting at 10 seconds
dotnet run --project examples/03-video-trimming.csproj video.mp4 10 60

# Alternative syntax
dotnet run --project examples/03-video-trimming.csproj \
  --input video.mp4 \
  --start 10 \
  --duration 60
```

**Output**:
```
Analyzing input file: video.mp4
File duration: 00:05:23.5600000
Resolution: 1920x1080
Video codec: h264

Trimming segment from 10s to 70s (duration: 60s)
✓ Trimming completed successfully
Output: video_trimmed.mp4 (145.2 MB)
Duration: 8.34s
```

---

### 4. Video Merging (04-video-merging.cs)

**Description**: Concatenate multiple video files into a single file.

**Features**:
- Multi-file concatenation
- Duration calculation
- Metadata analysis
- Progress reporting

**Usage**:
```bash
# Merge three videos
dotnet run --project examples/04-video-merging.csproj \
  merged.mp4 intro.mp4 main.mp4 outro.mp4

# Or in any order
dotnet run --project examples/04-video-merging.csproj \
  output.mp4 video1.mp4 video2.mp4 video3.mp4 video4.mp4
```

**Output**:
```
Analyzing 3 input files...
  intro.mp4: 5s, 1920x1080, h264
  main.mp4: 120s, 1920x1080, h264
  outro.mp4: 3s, 1920x1080, h264
Total duration: 128s

Merging 3 videos into merged.mp4
Progress: 100.0%

✓ Merge completed successfully
Output: merged.mp4
Output size: 1250.5 MB
Duration: 45.23s
```

---

### 5. Watermarking (05-watermarking.cs)

**Description**: Add a watermark/logo overlay to a video.

**Features**:
- 9 position options (TopLeft, TopRight, Center, etc.)
- Configurable scale and opacity
- Aspect ratio preservation
- Video and watermark analysis

**Usage**:
```bash
# Add watermark at top-right
dotnet run --project examples/05-watermarking.csproj video.mp4 logo.png TopRight

# Bottom-left position
dotnet run --project examples/05-watermarking.csproj video.mp4 logo.png BottomLeft

# Center position (default)
dotnet run --project examples/05-watermarking.csproj video.mp4 logo.png Center
```

**Supported Positions**:
- TopLeft, TopRight, TopCenter
- BottomLeft, BottomRight, BottomCenter
- MiddleLeft, MiddleRight
- Center

**Output**:
```
Video resolution: 1920x1080
Duration: 00:02:30.5600000
Watermark size: 512x256

Adding watermark at TopRight position
  Scale: 15.0% of video width
  Opacity: 80.0%
  Offset: X=15px, Y=15px

✓ Watermarking completed successfully
Output: video_watermarked.mp4
Size: 245.8 MB
Duration: 18.42s
```

---

### 6. Media Analysis (06-media-analysis.cs)

**Description**: Extract and display detailed metadata from video files.

**Features**:
- File size and duration
- Video properties (codec, resolution, frame rate)
- Audio properties (codec, sample rate, channels)
- Bitrate and compression ratio
- Multi-file analysis

**Usage**:
```bash
# Analyze single file
dotnet run --project examples/06-media-analysis.csproj video.mp4

# Analyze multiple files
dotnet run --project examples/06-media-analysis.csproj \
  video1.mp4 video2.mkv video3.webm

# Compare files
dotnet run --project examples/06-media-analysis.csproj input/*.mp4
```

**Output**:
```
FFmpeg: ffmpeg version 7.0.1...

╔════════════════════════════════════════╗
║  video.mp4                              ║
╚════════════════════════════════════════╝
File:               video.mp4
Size:               245.30 MB

Timing:
  Duration:         00:05:30.1200000
  Duration (sec):   330.12s

Video:
  Codec:            h264
  Resolution:       1920x1080
  Aspect Ratio:     1.78:1
  Frame Rate:       30 fps

Audio:
  Codec:            aac
  Sample Rate:      48000 Hz
  Channels:         2

Bitrate:
  Total:            6000 kbps
  Total:            6.00 Mbps

Statistics:
  File bitrate:     5.95 Mbps
  Compression:      99.2%
```

---

### 7. REST API Server (07-rest-api-server.cs)

**Description**: Run as a REST API service for remote video processing.

**Features**:
- HTTP endpoints for all operations
- JSON request/response format
- OpenAPI/Swagger support
- Health checks
- Error handling

**Usage**:
```bash
# Start server
dotnet run --project examples/07-rest-api-server.csproj

# In another terminal, make requests:
curl http://localhost:5000/health
curl http://localhost:5000/api/info
curl -X POST http://localhost:5000/api/transcode ...
```

**Endpoints**:

#### GET /health
Health check endpoint.
```bash
curl http://localhost:5000/health
# Response: {"status":"healthy"}
```

#### GET /api/info
Get FFmpeg information.
```bash
curl http://localhost:5000/api/info
# Response: {"available":true,"version":"ffmpeg version 7.0.1..."}
```

#### POST /api/analyze
Analyze a media file.
```bash
curl -X POST http://localhost:5000/api/analyze \
  -H "Content-Type: application/json" \
  -d '{"filePath":"video.mp4"}'
```

#### POST /api/transcode
Transcode a video.
```bash
curl -X POST http://localhost:5000/api/transcode \
  -H "Content-Type: application/json" \
  -d '{
    "inputPath":"input.mp4",
    "outputPath":"output.webm",
    "videoCodec":"VP9",
    "audioCodec":"Opus",
    "container":"WebM",
    "videoBitrate":1500,
    "quality":"High"
  }'
```

#### POST /api/trim
Trim a video segment.
```bash
curl -X POST http://localhost:5000/api/trim \
  -H "Content-Type: application/json" \
  -d '{
    "inputPath":"video.mp4",
    "outputPath":"clip.mp4",
    "startSeconds":10,
    "durationSeconds":60,
    "keyframe":true
  }'
```

---

### 8. Advanced Transcoding (08-advanced-transcoding.cs)

**Description**: Transcode with preset profiles optimized for different use cases.

**Features**:
- Multiple presets (web, streaming, mobile, archive)
- Compression ratio calculation
- Encoding speed estimation
- Input analysis before transcoding

**Presets**:
- **web**: VP9 WebM (1280x720) – Optimized for web streaming
- **streaming**: H.264 MP4 (1920x1080) – Live streaming ready
- **mobile**: H.264 MP4 (854x480) – Mobile device playback
- **archive**: H.265 MKV (lossless) – High-quality archival

**Usage**:
```bash
# Web preset (default)
dotnet run --project examples/08-advanced-transcoding.csproj input.mp4 output_dir/

# Streaming preset
dotnet run --project examples/08-advanced-transcoding.csproj input.mp4 output_dir/ streaming

# Mobile preset
dotnet run --project examples/08-advanced-transcoding.csproj input.mp4 output_dir/ mobile

# Archive preset
dotnet run --project examples/08-advanced-transcoding.csproj input.mp4 output_dir/ archive
```

**Output**:
```
Analyzing input file: input.mp4
Duration: 00:05:30.1200000
Resolution: 1920x1080
Bitrate: 6000 kbps

Transcoding with preset: web
Settings:
  Video codec:  VP9
  Audio codec:  Opus
  Bitrate:      1500k
  Quality:      High
  Resolution:   max 1280x720

✓ Transcode completed successfully
Output: output_dir__web.webm
Input size:   245.30 MB
Output size:  125.45 MB
Compression:  48.8%
Encoding time: 4m 32s
Encoding speed: 1.21x
```

---

## Building Your Own Example

Template for creating a new example:

```csharp
// =============================================================================
// Author: Your Name
// =============================================================================

using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Examples;

public class MyExample
{
    public static async Task Main(string[] args)
    {
        // Setup DI
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddFFmpegWrapper(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(600);
        });

        var sp = services.BuildServiceProvider();
        var ffmpeg = sp.GetRequiredService<IFFmpegService>();
        var logger = sp.GetRequiredService<ILogger<MyExample>>();

        try
        {
            // Your code here
            logger.LogInformation("Operation complete");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error");
        }
    }
}
```

---

## Performance Tips

1. **Use appropriate codec** – H.264 for speed, VP9 for quality
2. **Match output resolution** – Don't upscale, downscale as needed
3. **Adjust bitrate** – Higher bitrate = better quality but larger file
4. **Parallel processing** – Use batch operations for multiple files
5. **SSD storage** – Store temporary files on fast storage

## Troubleshooting

See [../../docs/faq.md](../../docs/faq.md) for common issues and solutions.

## Contributing

Examples are welcome! Create a new file following the naming convention and include documentation.
