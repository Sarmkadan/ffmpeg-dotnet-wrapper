# CLAUDE.md

Strongly-typed .NET 10 wrapper around the FFmpeg CLI (transcode, trim, merge, watermark, thumbnails, GIF, streaming) with a fluent settings API, shipped as the `FFmpegDotnetWrapper` NuGet package.

## Build

- SDK: .NET 10 (`global.json` pins `10.0.100`, rollForward latestMinor). FFmpeg must be on PATH for integration scenarios (`make verify-ffmpeg`).
- `dotnet restore && dotnet build -c Release` (or `make build`)
- Solution: `ffmpeg-dotnet-wrapper.slnx` (library + tests + benchmarks)
- Library project is `FFmpegDotnetWrapper.csproj` at repo root; it excludes `examples/`, `tests/`, `benchmarks/`, `src/Api/Controllers/`, `src/Middleware/`, `src/Program.cs` and a few host-only files via `<Compile Remove>`. Check the csproj before assuming a `src/` file is compiled.
- NuGet: `dotnet pack -c Release -o ./nupkg` (`make pack`)
- Docker: `make docker-build`, `make docker-compose-up` (API on :5000)

## Test

- `dotnet test -c Release` (`make test` builds first, then `--no-build`)
- Single test: `dotnet test --filter "FullyQualifiedName~TrimSettingsTests"`
- Stack: xunit 2.9, FluentAssertions 7, Moq. Test project: `tests/ffmpeg-dotnet-wrapper.Tests/`
- No real FFmpeg in unit tests: use `FakeFFmpegProcessRunner` (`src/Services/`) behind `IFFmpegProcessRunner`.
- CI: `.github/workflows/build.yml` runs restore/build/test in Release on push and PR to `main`.

## Lint / Format

- `dotnet format --verify-no-changes` (`make format`), `dotnet format` to fix (`make format-fix`)
- `make lint` = build with `/p:TreatWarningsAsErrors=true` (default is false; CS1591 missing-XML-doc is suppressed in `Directory.Build.props`)
- Style is in `.editorconfig`: 4 spaces, LF, braces always, Allman braces (`csharp_new_line_before_open_brace = all`), `_camelCase` private fields, PascalCase public members.

## Layout

- `src/Services/` - `FFmpegService` (main `IFFmpegService` impl), `FFmpegProcessRunner`, `TranscodeService`, `GifExportService`, `AdaptiveBitrateService`, `BatchOperationService`
- `src/Abstractions/` - `IFFmpegProcessRunner`, `ProcessResult`
- `src/Models/` - settings and result types (`TranscodeSettings`, `TrimSettings`, `MergeSettings`, `WatermarkSettings`, `MediaFile`, `ConversionResult`, ...)
- `src/Utilities/` - `ProcessUtilities`, `FileUtilities`, `ValidationUtilities`
- `src/Exceptions/` - `FFmpegException`, `ValidationException`, `FileOperationException`, ...
- `src/Configuration/` - `FFmpegOptions`, DI `ServiceCollectionExtensions`
- `src/Api/`, `src/Middleware/`, `src/Program.cs` - optional REST host (not part of the NuGet library)
- `src/Cli/`, `src/BackgroundJobs/`, `src/Events/`, `src/Policies/`, `src/Caching/`, `src/Repository/`, `src/Monitoring/` - CLI, job queue, events, retry/rate-limit policies, etc.
- `examples/` - standalone usage samples; `benchmarks/` - BenchmarkDotNet project; `docs/` - one markdown file per public type plus `architecture.md`, `faq.md`, `troubleshooting.md`
- Layering (see `docs/architecture.md`): Application (API/CLI/jobs) -> Services -> Models -> Utilities -> external `ffmpeg` process.

## Conventions

- Root namespace `FFmpegDotnetWrapper`; sub-namespaces mirror folders (`FFmpegDotnetWrapper.Models`, `.Services`, `.Exceptions`). Tests live in `FFmpegDotnetWrapper.Tests`.
- Nullable and implicit usings enabled; `LangVersion latest`; file-scoped namespaces.
- Public API gets XML doc comments (`GenerateDocumentationFile` is on).
- Companion-file pattern per type: `Foo.cs`, `FooExtensions.cs`, `FooJsonExtensions.cs`, `FooValidation.cs`, `FooEnumerableExtensions.cs`. Follow it when extending a model.
- Tests: one file per type, `FooTests.cs`, `FooExtensionsTests.cs`, `FooValidationTests.cs`; method names `Method_ShouldExpectedBehavior[_WhenCondition]`; `[Fact]`/`[Theory]`, FluentAssertions `.Should()`.
- Async methods end in `Async` and take `CancellationToken`.
- Settings classes expose `Clone()` and `Validate()`; validation throws `ValidationException`.
- Add a `docs/<TypeName>.md` when adding a public type. Commit messages use conventional prefixes (`docs:`, `chore:`, `feat:`, `fix:`).
