# Frequently Asked Questions

Common questions about FFmpeg .NET Wrapper.

## Installation & Setup

### Q: Do I need to install FFmpeg separately?

**A:** Yes. FFmpeg .NET Wrapper is a .NET wrapper around the FFmpeg binary. You must install FFmpeg separately:

```bash
# macOS
brew install ffmpeg

# Linux
sudo apt-get install ffmpeg

# Windows
choco install ffmpeg
```

Verify with: `ffmpeg -version`

### Q: Which .NET versions are supported?

**A:** Only **.NET 10** is supported. This is the latest LTS version with the best performance and security features.

We do not support .NET 6, 7, 8, or older versions.

### Q: Can I use this on .NET Framework?

**A:** No. FFmpeg .NET Wrapper requires .NET Core/.NET 5+. .NET Framework is legacy and no longer supported by Microsoft.

### Q: How do I verify the installation worked?

**A:** Create a test program:

```csharp
var services = new ServiceCollection();
services.AddFFmpegWrapper();
var sp = services.BuildServiceProvider();
var ffmpeg = sp.GetRequiredService<IFFmpegService>();
var available = await ffmpeg.IsFFmpegAvailableAsync();
Console.WriteLine(available ? "✓ Success" : "✗ FFmpeg not found");
```

Run it and check the output.

---

## Usage & API

### Q: How do I track progress of a transcode operation?

**A:** Use the `IProgress<OperationStatistics>` parameter:

```csharp
var progress = new Progress<OperationStatistics>(stat =>
{
    Console.WriteLine($"Progress: {stat.Percentage:F1}%");
    Console.WriteLine($"Elapsed: {stat.ElapsedTime.TotalSeconds:F0}s");
    Console.WriteLine($"ETA: {stat.EstimatedTimeRemaining?.TotalSeconds:F0}s");
});

await ffmpeg.TranscodeAsync(input, output, settings, progress);
```

The progress is updated in real-time as FFmpeg processes the file.

### Q: Can I cancel an operation in progress?

**A:** Yes, using `CancellationToken`:

```csharp
var cts = new CancellationTokenSource();

// Start operation in one task
var transcodeTask = ffmpeg.TranscodeAsync(
    input, output, settings,
    cancellationToken: cts.Token);

// Cancel from another task
cts.Cancel();

try
{
    await transcodeTask;
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
}
```

### Q: How do I increase the timeout for large files?

**A:** Configure timeout at startup:

```csharp
services.AddFFmpegWrapper(options =>
{
    options.DefaultTimeout = TimeSpan.FromMinutes(30);  // 30 minutes
});
```

Or via `appsettings.json`:

```json
{
  "FFmpegOptions": {
    "DefaultTimeout": "00:30:00"
  }
}
```

### Q: What codecs does the wrapper support?

**A:** The wrapper supports these video codecs:
- `H264` (x264) – Fast, good quality
- `H265` (x265) – Slower, better quality
- `VP8` (libvpx-vp8) – WebM format
- `VP9` (libvpx-vp9) – Better VP8
- `AV1` (libaom-av1) – Best compression, very slow

And these audio codecs:
- `AAC` – Streaming standard
- `MP3` – Legacy, wide support
- `Opus` – Modern, efficient
- `FLAC` – Lossless
- `VORBIS` – OGG Vorbis

Your FFmpeg build may not include all codecs. Check with `ffmpeg -encoders`.

### Q: How do I choose the right codec?

**A:** Consider these factors:

| Use Case | Video Codec | Audio Codec | Reasoning |
|----------|-------------|-------------|-----------|
| **Web streaming** | VP9 | Opus | Best compression for streaming |
| **Compatibility** | H264 | AAC | Works on all devices |
| **Speed** | H264 | AAC | Fastest encoding |
| **Quality** | AV1 | FLAC | Best for archival (slow) |
| **Mobile** | H265 | AAC | Good balance |

### Q: Can I use custom FFmpeg arguments?

**A:** The wrapper abstracts common operations. For advanced features, you can:

1. Use transcode settings to customize most operations
2. Access the underlying FFmpeg command (if enabled in logging)
3. Build your own wrapper around `ProcessUtilities`

```csharp
// Enable detailed logging to see FFmpeg command
services.AddFFmpegWrapper(options =>
{
    options.EnableDetailedLogging = true;
});

// View the command in logs
// FFmpeg command: ffmpeg -i input.mp4 -c:v libvpx-vp9 ...
```

### Q: How do I merge videos without re-encoding?

**A:** Use the merge operation. FFmpeg will concatenate files without re-encoding if formats match:

```csharp
var settings = new MergeSettings
{
    PreserveAudio = true,
    PreserveVideo = true,
    Crossfade = false
};

await ffmpeg.MergeAsync(
    new[] { "video1.mp4", "video2.mp4" },
    "merged.mp4",
    settings);
```

**Note**: All input files must have compatible codecs and formats.

---

## Performance & Optimization

### Q: How many concurrent operations can I run?

**A:** Depends on your system resources. The default is 4. Adjust with:

```csharp
services.AddFFmpegWrapper(options =>
{
    options.MaxConcurrentOperations = 8;  // Your CPU core count
});
```

**Guidelines**:
- Use CPU core count for CPU-bound operations
- Use 2x-4x core count for I/O-bound (network storage)
- Monitor memory usage (each FFmpeg process uses ~200-500 MB)

### Q: Why is transcoding so slow?

**A:** Transcoding is CPU-intensive. Factors affecting speed:

1. **Codec choice** – VP9 is 3-5x slower than H.264
2. **Quality setting** – Higher quality = slower
3. **Input file size** – Larger files take longer
4. **System CPU** – Faster CPU = faster encoding
5. **Other processes** – Competing processes slow it down

**To improve speed**:
```csharp
var settings = new TranscodeSettings
{
    VideoCodec = VideoCodec.H264,      // Faster than VP9
    Quality = QualityPreset.Low,       // Lower quality = faster
    MaxWidth = 1280,                   // Smaller resolution = faster
    MaxHeight = 720,
    FrameRate = 30                     // Lower fps = faster
};
```

### Q: How much disk space do I need?

**A:** Safe minimum: **1.5x largest input file size**

For example:
- 1 GB input file → need 1.5 GB free
- 5 GB input file → need 7.5 GB free

Transcoding creates temporary files in `WorkingDirectory` (default: system temp).

### Q: Can I use an SSD for temporary files?

**A:** Yes! Configure it:

```csharp
services.AddFFmpegWrapper(options =>
{
    options.WorkingDirectory = "/mnt/ssd/ffmpeg-temp";
});
```

This significantly improves performance.

### Q: How do I process thousands of files efficiently?

**A:** Use batch processing with a queue:

```csharp
var batchService = serviceProvider.GetRequiredService<BatchOperationService>();

var progress = new Progress<OperationStatistics>(stat =>
{
    Console.WriteLine($"Progress: {stat.CompletedOperations}/{stat.TotalOperations}");
});

var files = Directory.GetFiles("input/", "*.mp4");
await batchService.ProcessFilesAsync(
    files,
    "output/",
    settings,
    progress);
```

Or use background jobs for async processing:

```csharp
foreach (var file in files)
{
    var jobId = await jobService.EnqueueTranscodeAsync(file, outputDir, settings);
    // Jobs processed in background
}
```

---

## Troubleshooting

### Q: I get "FFmpeg is not installed or not available"

**A:** FFmpeg is either not installed or not in your PATH.

**Verify installation**:
```bash
ffmpeg -version
```

**If not found**:
- **macOS**: `brew install ffmpeg`
- **Linux**: `sudo apt-get install ffmpeg`
- **Windows**: Download from https://ffmpeg.org/download.html

**If installed but not in PATH**:

Specify the full path:
```csharp
services.AddFFmpegWrapper(options =>
{
    options.FFmpegPath = "/usr/local/bin/ffmpeg";
    // or on Windows
    // options.FFmpegPath = @"C:\ffmpeg\bin\ffmpeg.exe";
});
```

### Q: Operation times out

**A:** The timeout is set too low for the file size.

**Increase timeout**:
```csharp
services.AddFFmpegWrapper(options =>
{
    options.DefaultTimeout = TimeSpan.FromMinutes(30);
});
```

**Rule of thumb**: 1 minute per 1 GB of video file.

### Q: "Unknown encoder 'vp9'"

**A:** Your FFmpeg build doesn't include VP9 codec.

**Check available codecs**:
```bash
ffmpeg -encoders | grep -i vp9
```

**Solutions**:
1. Use a different codec (H.264)
2. Recompile FFmpeg with VP9 support
3. Install FFmpeg from source with all codecs

### Q: Output file is corrupted or won't play

**A:** Usually caused by:

1. **Interrupted process** – Check logs for errors
2. **Incompatible codec combination** – Use standard settings
3. **Incomplete write** – Ensure output directory has write permissions
4. **File still open** – Close file before checking

**To debug**:
```csharp
services.AddLogging(builder =>
    builder.SetMinimumLevel(LogLevel.Debug)
           .AddConsole());

services.AddFFmpegWrapper(options =>
{
    options.EnableDetailedLogging = true;
});

// Check the detailed logs
```

### Q: Audio is out of sync with video

**A:** Usually happens with certain codec/container combinations.

**Workaround**:
```csharp
var settings = new TrimSettings
{
    StartTime = TimeSpan.FromSeconds(10),
    Duration = TimeSpan.FromSeconds(30),
    Keyframe = true  // Align to nearest keyframe
};
```

Or re-encode instead of copying streams:
```csharp
var settings = new TranscodeSettings
{
    VideoCodec = VideoCodec.H264,
    AudioCodec = AudioCodec.AAC  // Re-encode both
};
```

### Q: High memory usage

**A:** FFmpeg memory usage is normal (200-500 MB per process).

**To reduce**:
1. Decrease `MaxConcurrentOperations`
2. Reduce output resolution
3. Use simpler codec (H.264 instead of VP9)
4. Restart process periodically

### Q: No space left on device

**A:** Temporary files or output files filled the disk.

**Solutions**:
1. Delete old temporary files: `rm -rf /tmp/ffmpeg-*`
2. Free up disk space
3. Use alternate disk for temp: `options.WorkingDirectory = "/mnt/other/"`
4. Implement cleanup: Delete output after processing

---

## Advanced Topics

### Q: How do I integrate with my database?

**A:** Implement `IMediaRepository`:

```csharp
public class DatabaseMediaRepository : IMediaRepository
{
    private readonly IDbContext _db;
    
    public async Task SaveMediaAsync(MediaFile media)
    {
        _db.MediaFiles.Add(media);
        await _db.SaveChangesAsync();
    }
}

// Register
services.AddScoped<IMediaRepository, DatabaseMediaRepository>();
```

### Q: Can I use this in ASP.NET Core?

**A:** Yes! Register in `Program.cs`:

```csharp
builder.Services.AddFFmpegWrapper(options =>
{
    options.DefaultTimeout = TimeSpan.FromSeconds(600);
});

var app = builder.Build();

app.MapPost("/api/transcode", async (
    TranscodeRequest request,
    IFFmpegService ffmpeg) =>
{
    var result = await ffmpeg.TranscodeAsync(
        request.InputPath,
        request.OutputPath,
        request.Settings);
    return result;
});
```

### Q: How do I handle webhooks on job completion?

**A:** Enable webhooks and implement handler:

```csharp
services.AddFFmpegWrapper(options =>
{
    options.EnableWebhooks = true;
});

var jobService = serviceProvider.GetRequiredService<WebhookService>();

await jobService.RegisterWebhookAsync(
    url: "https://example.com/webhook",
    events: WebhookEvent.OperationCompleted);
```

Webhook will POST to your URL on completion.

### Q: Can I customize the FFmpeg command?

**A:** The wrapper generates FFmpeg commands internally. For full control, use raw FFmpeg via `ProcessUtilities` or call `ffmpeg` directly.

---

## Contributing

### Q: How do I contribute to the project?

**A:** 

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make changes (follow code guidelines)
4. Test thoroughly
5. Push: `git push origin feature/my-feature`
6. Open a pull request

Guidelines:
- Follow C# naming conventions
- Add XML documentation
- Write tests for new features
- Keep files under 200 lines

### Q: I found a bug, how do I report it?

**A:** 

1. Check if it's already reported: https://github.com/vladyslav-zaiets/ffmpeg-dotnet-wrapper/issues
2. Create new issue with:
   - Clear title and description
   - Steps to reproduce
   - Expected vs. actual behavior
   - System info (OS, .NET version, FFmpeg version)
   - Logs or error messages

---

## License & Support

### Q: What license is this project under?

**A:** MIT License. You can use this in commercial projects. See [LICENSE](../LICENSE) file.

### Q: Is there commercial support available?

**A:** For support, questions, or custom development:
- Email: `support@sarmkadan.com`
- Website: https://sarmkadan.com
- GitHub Issues: https://github.com/vladyslav-zaiets/ffmpeg-dotnet-wrapper/issues

### Q: Can I use this in production?

**A:** Yes! The library is production-ready. However:
- Thoroughly test in your environment
- Configure proper error handling
- Monitor logs and metrics
- Plan for failures and recovery
- Keep FFmpeg updated

See [deployment.md](deployment.md) for production setup.
