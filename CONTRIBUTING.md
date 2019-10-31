# Contributing to FFmpeg .NET Wrapper

Thank you for your interest in contributing to FFmpeg .NET Wrapper! We welcome bug reports, feature requests, and pull requests from the community.

## Code of Conduct

This project adheres to the Contributor Covenant [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## Reporting Issues

Found a bug or have a feature request? Please open an issue on [GitHub Issues](https://github.com/sarmkadan/ffmpeg-dotnet-wrapper/issues).

**Before opening an issue:**
- Check if the issue already exists
- Include a clear description and steps to reproduce
- For bugs: mention your .NET version, OS, FFmpeg version, and any error messages
- For features: explain the use case and how it would benefit users

**Security issues**: Please do NOT open public issues for security vulnerabilities. See [SECURITY.md](SECURITY.md) for reporting instructions.

## Development Setup

### Prerequisites

- **.NET 10.0 SDK** – [Download](https://dotnet.microsoft.com/download)
- **FFmpeg** – [Installation Guide](https://ffmpeg.org/download.html)
  - **macOS**: `brew install ffmpeg`
  - **Linux (Ubuntu/Debian)**: `sudo apt-get install ffmpeg`
  - **Windows**: Download installer or use `choco install ffmpeg`
- **Git** – [Download](https://git-scm.com/)

### Cloning & Building

```bash
# Fork the repository on GitHub, then clone your fork
git clone https://github.com/YOUR_USERNAME/ffmpeg-dotnet-wrapper.git
cd ffmpeg-dotnet-wrapper

# Add upstream remote for syncing
git remote add upstream https://github.com/sarmkadan/ffmpeg-dotnet-wrapper.git

# Build the project
dotnet build

# Run tests
dotnet test

# Verify FFmpeg is available
ffmpeg -version
```

## Making Changes

### Branch Naming

Create feature branches from `main` with descriptive names:

```bash
git checkout -b feature/add-subtitle-support
git checkout -b fix/memory-leak-in-batch-operations
git checkout -b docs/improve-api-reference
```

### Code Style

Follow these conventions:

- **Naming**: Use PascalCase for public members, camelCase for private members
- **Formatting**: Follow C# conventions (match the existing code style)
- **Documentation**: Add XML documentation comments to all public APIs:
  ```csharp
  /// <summary>
  /// Transcodes a video file with the specified settings.
  /// </summary>
  /// <param name="inputPath">Path to the input video file</param>
  /// <param name="outputPath">Path for the output file</param>
  /// <param name="settings">Transcoding configuration</param>
  /// <returns>Result of the transcode operation</returns>
  public async Task<ConversionResult> TranscodeAsync(
      string inputPath,
      string outputPath,
      TranscodeSettings settings)
  ```
- **Author Headers**: Preserve existing author attribution headers in files you modify
- **No External Dependencies**: Do not introduce dependencies beyond `Microsoft.Extensions.*`

### Testing

- Write unit tests for all new features
- Ensure existing tests still pass: `dotnet test`
- Tests should be clear and focus on behavior, not implementation details
- Use descriptive test names: `ShouldThrowExceptionWhenInputFileDoesNotExist` instead of `Test1`

### Committing

Write clear, descriptive commit messages:

```bash
# Good
git commit -m "Add support for subtitle tracks in transcode operation"

# Good
git commit -m "Fix: handle empty merge settings gracefully"

# Avoid vague messages
git commit -m "Fixed stuff"
git commit -m "WIP"
```

## Submitting a Pull Request

1. **Sync with upstream** (if time has passed):
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Push to your fork**:
   ```bash
   git push origin feature/my-feature
   ```

3. **Open a pull request** on GitHub with:
   - A clear title describing the change
   - Description explaining what the PR does and why
   - Reference any related issues: `Fixes #123`
   - Confirmation that tests pass locally

4. **CI checks** must pass (GitHub Actions will run automatically)

5. **Code review**: Address feedback from maintainers

6. **Merge**: Once approved, your PR will be merged!

## PR Guidelines

- Keep PRs focused on a single change or feature
- If your PR is large, consider breaking it into smaller PRs
- Rebase before pushing to keep history clean
- All tests must pass before merging
- Aim for descriptive commit messages (will be part of git history)

## License

By contributing, you agree that your contributions will be licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Questions?

Open an issue with the `question` label or check existing discussions on GitHub.

Thank you for helping make FFmpeg .NET Wrapper better! 🚀
