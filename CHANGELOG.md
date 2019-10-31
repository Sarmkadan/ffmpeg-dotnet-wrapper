# Changelog

All notable changes to FFmpeg .NET Wrapper are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added

- **Background Job Service**: Asynchronous operation queue with persistence
  - `BackgroundJobService` for enqueuing transcode/trim/merge/watermark operations
  - `IJobRepository` abstraction for job storage
  - Job state tracking: Pending, Processing, Completed, Failed
  - Webhook notifications on job completion

- **Webhook Integration**: Notify external systems on operation completion
  - `WebhookService` for registering and triggering webhooks
  - Configurable webhook endpoints
  - Retry logic with exponential backoff

- **REST API Controller**: `FFmpegController` with endpoints for all operations
  - `POST /api/ffmpeg/transcode` – Transcode video files
  - `POST /api/ffmpeg/trim` – Trim video segments
  - `POST /api/ffmpeg/merge` – Merge multiple videos
  - `POST /api/ffmpeg/watermark` – Add watermarks

- **Batch Processing**: `BatchOperationService` for concurrent file processing
  - Process multiple files in parallel with configurable concurrency
  - Aggregate statistics: completion rate, success rate, duration
  - Progress reporting via `IProgress<OperationStatistics>`

- **Media Analysis**: `AnalyzeMediaAsync()` to extract file metadata
  - Duration, resolution, codecs, frame rate, bitrate
  - Codec detection (video/audio)
  - Used for validation and progress estimation

### Changed

- **Improved Error Handling**: `FFmpegException` with exit code and raw output
  - Detailed error messages for debugging
  - FFmpeg stderr captured in result
  - Timeout handling with proper cancellation

- **Logging Architecture**: Structured logging throughout the pipeline
  - `ILogger<T>` injected into all services
  - Information, Debug, Warning, Error levels
  - Progress updates logged at Debug level

- **Configuration**: Flexible `FFmpegOptions` for startup customization
  - `DefaultTimeout`: Configurable per-operation timeout
  - `MaxConcurrentOperations`: Control parallelism
  - `FFmpegPath`: Custom FFmpeg executable path
  - `WorkingDirectory`: Temporary file location
  - `EnableDetailedLogging`: Debug output control

### Fixed

- Process cleanup on timeout or cancellation
- Proper handling of special characters in file paths
- Audio/video sync issues with keyframe alignment
- Memory leak in progress tracking with large files

## [1.1.0] - 2026-04-15

### Added

- **Watermark Support**: Add image overlays to videos
  - `WatermarkSettings` with position, scale, opacity
  - Multiple position options: TopLeft, TopRight, BottomLeft, BottomRight, Center
  - Aspect ratio preservation

- **Video Merge**: Concatenate multiple video files
  - `MergeAsync()` method supporting multiple inputs
  - Configurable audio/video preservation
  - Optional crossfade transitions

- **Video Trim**: Extract segments from videos
  - `TrimAsync()` for segment extraction
  - Keyframe-aligned trimming option
  - Selective audio/video stream preservation

- **CLI Command Parser**: `CliCommandParser` for command-line interface
  - Commands: transcode, trim, merge, watermark
  - Argument parsing and validation
  - Output formatting with progress

- **Repository Pattern**: Data abstraction layer
  - `IMediaRepository` for media file storage
  - `IOperationRepository` for operation history
  - In-memory implementations included

### Changed

- Refactored `TranscodeSettings` for clarity
  - Separated video/audio bitrate
  - Added quality presets: Low, Medium, High, Lossless
  - Added auto-scaling with max dimensions

- Improved `ProcessUtilities` robustness
  - Better FFmpeg output parsing
  - Frame/time/bitrate extraction for progress
  - Timeout enforcement via `CancellationToken`

### Fixed

- Incorrect frame rate calculation from FFmpeg output
- Memory issues with large concurrent operations
- Path normalization on Windows with UNC paths

## [1.0.0] - 2026-03-01

### Added

- **Core Transcoding**: `IFFmpegService` with transcode operation
  - Video codec support: H.264, H.265, VP9, AV1
  - Audio codec support: AAC, MP3, Opus, FLAC, Vorbis
  - Container formats: MP4, WebM, MKV, OGG, AVI
  - Bitrate and quality configuration
  - Frame rate and resolution control
  - Aspect ratio preservation

- **Dependency Injection**: `ServiceCollectionExtensions`
  - Clean DI registration: `AddFFmpegWrapper()`
  - Configuration via `FFmpegOptions`
  - Logging integration

- **Process Management**: `ProcessUtilities`
  - Safe FFmpeg subprocess spawning
  - Array-based arguments (injection-proof)
  - Output stream capture
  - Exit code validation
  - Timeout enforcement

- **Utilities**: Helper functions for common tasks
  - `FileUtilities`: Path validation, normalization
  - `ValidationUtilities`: Input validation
  - `ProgressTracker`: FFmpeg output parsing
  - `ExtensionMethods`: LINQ extensions

- **Configuration**: `FFmpegOptions` class
  - FFmpeg executable path
  - Operation timeout
  - Working directory
  - Logging control

- **Documentation**: Comprehensive guides
  - README.md with examples
  - Architecture.md with design patterns
  - API reference
  - Getting started guide
  - FAQ

- **Examples**: 7 complete example programs
  - Basic transcoding
  - Batch processing
  - Video trimming
  - Video merging
  - Watermarking
  - Media analysis
  - REST API server

- **CI/CD**: GitHub Actions workflow
  - Build on Linux, macOS, Windows
  - Test with .NET 10
  - Docker image building
  - NuGet package publishing

- **Docker Support**: Dockerfile and docker-compose.yml
  - Multi-stage build for optimization
  - Health checks
  - Volume mounts for input/output
  - Configurable via environment variables

### Security

- Command injection prevention via array arguments
- Path traversal prevention with validation
- Input validation for all user-provided data
- No shell execution (process array API)

---

## Version Support

| Version | Release Date | .NET Versions | Status |
|---------|--------------|---------------|--------|
| 1.2.0 | 2026-05-04 | 10.0 | Current |
| 1.1.0 | 2026-04-15 | 10.0 | Supported |
| 1.0.0 | 2026-03-01 | 10.0 | Supported |

---

## Upgrade Guide

### From 1.1.0 to 1.2.0

No breaking changes. New features:
- Background jobs: Enable with `options.EnableBackgroundJobs = true`
- Webhooks: Enable with `options.EnableWebhooks = true`
- REST API: Use new `FFmpegController` endpoints

### From 1.0.0 to 1.1.0

No breaking changes. New methods:
- `TrimAsync()` – Extract video segments
- `MergeAsync()` – Concatenate videos
- `WatermarkAsync()` – Add image overlays

---

## Contributing

See [README.md](README.md#contributing) for contribution guidelines.

---

## License

MIT License – See [LICENSE](LICENSE) file.

Copyright © 2026 Vladyslav Zaiets
