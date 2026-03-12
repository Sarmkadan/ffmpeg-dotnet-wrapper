# Migration Guide for v2.0
## Breaking Changes
* None
## New Features Overview
* Real-time streaming pipeline with adaptive bitrate switching
## Step-by-Step Migration from v1.x to v2.0
1. Update FFmpegDotnetWrapper.csproj to target net10.0
2. Update NuGet packages to latest versions
## Code Examples Showing Old vs New API
* Old API: `FFmpegService.TranscodeAsync(inputFile, outputFile)`
* New API: `FFmpegService.TranscodeAsync(inputFile, outputFile, new TranscodeSettings { AdaptiveBitrate = true })`
## Configuration Changes
* Add `AdaptiveBitrate` setting to TranscodeSettings