# Contributing to FFmpeg .NET Wrapper

Thanks for helping out! Bug reports, feature requests and pull requests are all welcome.

## Prerequisites

- **.NET SDK 10.0.100** or newer in the 10.0 line. `global.json` pins `10.0.100` with `rollForward: latestMinor`. The library targets `net10.0`.
- **FFmpeg** on your `PATH`. Unit tests use a fake process runner, but integration scenarios call the real `ffmpeg` binary. Check it with `make verify-ffmpeg` or `ffmpeg -version`.
- **Make** (optional). The Makefile wraps the `dotnet` commands below.

## Build

```bash
dotnet restore
dotnet build -c Release    # or: make build
```

The solution is `ffmpeg-dotnet-wrapper.slnx`. It contains the library, the tests and the benchmarks.

## Run tests

```bash
dotnet test -c Release     # or: make test
```

Run a single test class with a filter:

```bash
dotnet test --filter "FullyQualifiedName~TrimSettingsTests"
```

Tests use xUnit, FluentAssertions and Moq. They live in `tests/ffmpeg-dotnet-wrapper.Tests/`.

## Coding conventions

- Follow `.editorconfig`: 4 spaces, LF line endings, braces always, Allman braces. Run `dotnet format --verify-no-changes` (or `make format`) before pushing.
- `Nullable` is enabled. Fix nullability warnings rather than suppressing them.
- The build must produce **no warnings**. `make lint` builds with `TreatWarningsAsErrors=true`.
- Add XML documentation comments to every public type and member.
- Async methods end in `Async` and take a `CancellationToken`.

## Pull request process

1. Branch from an up-to-date `main`, for example `git checkout -b feat/trim-validation`.
2. Keep each PR small and focused on one change. Split unrelated work into separate PRs.
3. Use conventional commit prefixes (`feat:`, `fix:`, `docs:`, `chore:`).
4. Add or update tests for behaviour changes. Add a `docs/<TypeName>.md` when you add a public type.
5. Make sure CI passes. The `CI` workflow (`.github/workflows/build.yml`) runs restore, build and test in Release on every push and PR to `main`.
6. In the PR description, explain what changed and why, and list how you tested it.

By contributing, you agree that your contributions will be licensed under the MIT License.
