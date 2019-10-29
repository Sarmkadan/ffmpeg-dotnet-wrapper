# Changelog

All notable changes to FFmpeg .NET Wrapper are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.1] - 2026-03-05
### Security
- Added input validation and length limits
- Added request timeout configuration
- Added security policy and vulnerability reporting

## [2.0.0] - 2026-07-18

### Added
- Add real-time streaming pipeline with adaptive bitrate switching
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x

### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency

### Fixed
- Various edge cases found through testing

## [1.0.0] - 2025-09-22

### Added

- **NuGet Packaging**: `FFmpegDotnetWrapper` available on NuGet
  - Package ID, version, license, and readme metadata configured
  - Source link support for debugging into the library
  - XML documentation generated for IntelliSense

- **Docker Support**: `Dockerfile` and `docker-compose.yml`
  - Multi-stage build (SDK → runtime) for minimal image size
  - FFmpeg pre-installed in image
  - Volume mounts for input/output directories
  - Health checks and environment variable configuration

- **CI/CD**: GitHub Actions workflows
  - `build.yml`: Build and test on Linux, macOS, Windows
  - `codeql.yml`: Automated security scanning on push and schedule
  - `nuget-publish.yml`: Automated NuGet publishing on release tags
  - Dependabot configuration for weekly dependency updates

- **Streaming Progress**: `StreamingProgressService` for real-time progress over HTTP
  - Server-sent events for long-running operations
  - Progress reporting compatible with `IProgress<OperationStatistics>`

- **Security**: Input hardening across all code paths
  - Process arguments passed as arrays (no shell expansion)
  - Path traversal checks in `ValidationUtilities`
  - Rate limiting middleware to prevent resource exhaustion

### Changed

- Promoted all APIs to stable; removed `Experimental` annotations
- `FFmpegOptions.DefaultTimeout` default raised from 300 s to 600 s
- Documentation overhauled: architecture diagram, FAQ, deployment guide

### Fixed

- Temporary file cleanup on cancellation and process timeout
- `ProgressTracker` reported stale frame count at operation end

---

## [0.9.0] - 2025-07-28

### Added

- **REST API Controller**: `FFmpegController` with endpoints for all operations
  - `POST /api/ffmpeg/transcode`
  - `POST /api/ffmpeg/trim`
  - `POST /api/ffmpeg/merge`
  - `POST /api/ffmpeg/watermark`
  - `ApiRequest` and `ApiResponse` DTOs

- **Background Job Service**: Asynchronous operation queue
  - `BackgroundJobService` for enqueuing transcode/trim/merge/watermark jobs
  - `JobQueue` with in-memory persistence
  - Job state tracking: Pending, Processing, Completed, Failed

- **Webhook Integration**: Notify external systems on operation completion
  - `WebhookService` with configurable endpoints
  - Retry logic with exponential backoff

- **Middleware pipeline**: `ErrorHandlingMiddleware`, `RateLimitingMiddleware`,
  `RequestLoggingMiddleware`, `ValidationMiddleware`

- **`ApplicationStartup`**: Centralised startup configuration for API mode

### Fixed

- Middleware order causing validation errors to bypass error handler
- Job queue blocking thread pool under high concurrency

---

## [0.8.0] - 2025-06-16

### Added

- **Repository Pattern**: Persistence abstraction layer
  - `IMediaRepository`, `MediaRepository` for media file metadata
  - `IOperationRepository`, `OperationRepository` for operation history
  - In-memory implementations suitable for testing and small deployments

- **Event Publishing**: `EventPublisher` for internal operation lifecycle events
  - Raised on operation start, completion, and failure

- **Caching**: `CacheService` for ffprobe metadata results
  - Avoids repeated analysis of the same file

### Changed

- DI registration updated: `AddFFmpegWrapper()` now registers repositories and cache
- `IFFmpegService` made fully mockable via interface segregation

---

## [0.7.0] - 2025-05-19

### Added

- **Batch Processing**: `BatchOperationService` for concurrent file processing
  - Process multiple files with configurable parallelism (`MaxConcurrentOperations`)
  - Aggregate statistics: total, completed, failed, success rate, elapsed time
  - Progress reporting via `IProgress<OperationStatistics>`

- **`OperationStatistics`** model with counts, rates, and timing

### Fixed

- `SemaphoreSlim` not released on exception in batch loop
- Incorrect elapsed time reported for very short operations (<100 ms)

---

## [0.6.0] - 2025-04-28

### Added

- **Media Analysis**: `AnalyzeMediaAsync()` to extract file metadata via ffprobe
  - Duration, resolution (width × height), codecs, frame rate, bitrate
  - `MediaFile` model with analysed properties
  - Used internally for progress estimation and validation

### Changed

- `ProcessUtilities` refactored to handle both ffmpeg and ffprobe invocations
- `FFmpegConstants` updated with ffprobe argument templates

---

## [0.5.0] - 2025-04-07

### Added

- **CLI Command Parser**: `CliCommandParser` for command-line interface
  - Commands: `transcode`, `trim`, `merge`, `watermark`
  - Argument parsing and `--help` output
  - `OutputFormatter` for human-readable console output

- **Progress Tracking**: `ProgressTracker` parses FFmpeg stderr lines
  - Extracts frame, time, bitrate, and speed fields
  - Emits `FFmpegProgressUpdate` objects consumed by `IProgress<T>`

### Changed

- `src/Program.cs` updated to dispatch between CLI and API startup modes

---

## [0.4.0] - 2025-03-14

### Added

- **Watermark Support**: `WatermarkAsync()` adds image overlays to videos
  - `WatermarkSettings`: position, scale (0–1), opacity (0–1), pixel offsets
  - Positions: `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`, `Center`
  - Aspect ratio preservation for overlay images

### Fixed

- `overlay` filter expression used wrong variable for Y offset on `BottomLeft`/`BottomRight`

---

## [0.3.0] - 2025-02-24

### Added

- **Video Merge**: `MergeAsync()` concatenates multiple video files
  - `MergeSettings`: audio/video stream preservation, optional crossfade flag
  - Generates concat demuxer list file; cleaned up after operation

- **Video Trim**: `TrimAsync()` extracts segments
  - `TrimSettings`: start time, duration (null = to end), keyframe alignment
  - Selective audio/video stream inclusion

### Fixed

- Merge operation left temporary concat list on disk when FFmpeg exited non-zero
- Trim with `Keyframe = false` produced audio drift on some codecs

---

## [0.2.0] - 2025-02-03

### Added

- **Error Handling**: `FFmpegException` with `ExitCode` and `RawOutput` properties
- **Validation**: `ValidationUtilities` — path existence, null/empty guards, bitrate bounds
- **File Utilities**: `FileUtilities` — path normalisation, extension checks, temp file helpers
- **Formatting**: `FormattingUtilities` — duration, bitrate, and codec string helpers
- **Extension Methods**: `ExtensionMethods` — LINQ and string convenience helpers

### Changed

- `ConversionResult` now includes `ErrorMessage`, `RawOutput`, and `ExitCode`
- Failed operations no longer throw by default; check `result.Success` instead

---

## [0.1.0] - 2025-01-15

### Added

- **Core Transcoding**: `IFFmpegService` / `FFmpegService` with `TranscodeAsync()`
  - Video codecs: H.264, H.265, VP9, AV1
  - Audio codecs: AAC, MP3, Opus, FLAC, Vorbis
  - Container formats: MP4, WebM, MKV, OGG, AVI
  - Bitrate, quality preset, frame rate, and resolution control

- **Process Management**: `ProcessUtilities`
  - FFmpeg subprocess spawning with array-based arguments
  - stdout/stderr capture; exit code validation
  - Timeout enforcement via `CancellationToken`

- **Configuration**: `FFmpegOptions` and `ServiceCollectionExtensions`
  - `AddFFmpegWrapper()` DI extension for clean registration
  - Options: `FFmpegPath`, `DefaultTimeout`, `WorkingDirectory`, `EnableDetailedLogging`

- **Models**: `TranscodeSettings`, `ConversionResult`, `FFmpegOperation`

- **Constants**: `FFmpegConstants`, `OperationConstants`

- **Logging**: `ILogger<T>` injected into all services; Information/Debug/Warning/Error levels

---

## Version Support

| Version | Release Date | .NET Versions | Status    |
|---------|--------------|---------------|-----------|
| 1.0.0   | 2025-09-22   | 10.0          | Current   |
| 0.9.0   | 2025-07-28   | 10.0          | Supported |
| 0.8.0   | 2025-06-16   | 10.0          | Supported |

---

## Upgrade Guide

### From 0.9.0 to 1.0.0

No breaking changes. New optional features:
- Streaming progress: use `StreamingProgressService` in API projects
- NuGet install: `dotnet add package FFmpegDotnetWrapper`

### From 0.8.0 to 0.9.0

No breaking changes. New methods and services:
- `BackgroundJobService.EnqueueTranscodeAsync()` — asynchronous job queue
- `WebhookService.NotifyAsync()` — completion webhooks
- REST API via `FFmpegController`

---

## Contributing

See [README.md](README.md#contributing) for contribution guidelines.

---

## License

MIT License – See [LICENSE](LICENSE) file.

Copyright © 2025 Vladyslav Zaiets
