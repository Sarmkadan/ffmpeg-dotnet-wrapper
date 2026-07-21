// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for ProcessUtilities.cs
// =============================================================================

using System;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Utilities;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests
{
    public class ProcessUtilitiesTests
    {
        [Fact]
        public void EscapeArgument_EmptyString_ReturnsQuotedEmptyString()
        {
            // Arrange
            var emptyArg = string.Empty;

            // Act
            var result = ProcessUtilities.EscapeArgument(emptyArg);

            // Assert
            result.Should().Be("\"\"");
        }

        [Fact]
        public void EscapeArgument_NullString_ReturnsQuotedEmptyString()
        {
            // Arrange
            string nullArg = null;

            // Act
            var result = ProcessUtilities.EscapeArgument(nullArg);

            // Assert
            result.Should().Be("\"\"");
        }

        [Fact]
        public void EscapeArgument_PlainTextWithoutSpaces_ReturnsUnchanged()
        {
            // Arrange
            var plainArg = "plaintext";

            // Act
            var result = ProcessUtilities.EscapeArgument(plainArg);

            // Assert
            result.Should().Be("plaintext");
        }

        [Fact]
        public void EscapeArgument_TextWithSpaces_ReturnsQuoted()
        {
            // Arrange
            var spacedArg = "text with spaces";

            // Act
            var result = ProcessUtilities.EscapeArgument(spacedArg);

            // Assert
            result.Should().Be("\"text with spaces\"");
        }

        [Fact]
        public void EscapeArgument_TextWithQuotes_ReturnsEscapedAndQuoted()
        {
            // Arrange
            var quotedArg = "text with \"quotes\"";

            // Act
            var result = ProcessUtilities.EscapeArgument(quotedArg);

            // Assert
            result.Should().Be("\"text with \\\"quotes\\\"\"");
        }

        [Fact]
        public void EscapeArgument_TextWithBackslashes_ReturnsQuoted()
        {
            // Arrange
            var backslashArg = "path\\to\\file";

            // Act
            var result = ProcessUtilities.EscapeArgument(backslashArg);

            // Assert - The result should contain escaped backslashes inside quotes
            result.Should().StartWith("\"").And.EndWith("\"");
            result.Should().Contain("\\").And.Contain("to");
        }

        [Fact]
        public void EscapeArgument_TextWithMultipleSpecialChars_ReturnsProperlyEscaped()
        {
            // Arrange
            var complexArg = "file with spaces and \"quotes\" and \\backslashes";

            // Act
            var result = ProcessUtilities.EscapeArgument(complexArg);

            // Assert - Should be wrapped in quotes and contain escaped quotes and backslashes
            result.Should().StartWith("\"").And.EndWith("\"");
            result.Should().Contain("spaces");
            result.Should().Contain("quotes");
            result.Should().Contain("backslashes");
        }

        [Fact]
        public void ExecuteProcess_SuccessfulCommand_ReturnsProcessResultWithExitCodeZero()
        {
            // Arrange - Use echo command which is cross-platform
            var fileName = "echo";
            var arguments = "test output";

            // Act
            var result = ProcessUtilities.ExecuteProcess(fileName, arguments);

            // Assert
            result.Should().NotBeNull();
            result.ExitCode.Should().Be(0);
            result.StandardOutput.Should().Contain("test output");
            result.StandardError.Should().BeEmpty();
            result.TimedOut.Should().BeFalse();
            result.Success.Should().BeTrue();
        }

        [Fact]
        public void ExecuteProcess_CommandWithWorkingDirectory_ReturnsProcessResult()
        {
            // Arrange
            var fileName = "echo";
            var arguments = "working directory test";
            var workingDirectory = Environment.CurrentDirectory;

            // Act
            var result = ProcessUtilities.ExecuteProcess(fileName, arguments, workingDirectory);

            // Assert
            result.Should().NotBeNull();
            result.ExitCode.Should().Be(0);
            result.Success.Should().BeTrue();
        }

        [Fact]
        public void ExecuteProcess_NonZeroExitCode_ReturnsCorrectExitCode()
        {
            // Arrange - Use a command that will fail
            // On Unix-like systems, using invalid command; on Windows, using cmd /c exit 1
            var fileName = OperatingSystem.IsWindows() ? "cmd.exe" : "false";
            var arguments = OperatingSystem.IsWindows() ? "/c exit 1" : "";

            // Act
            var result = ProcessUtilities.ExecuteProcess(fileName, arguments);

            // Assert
            result.Should().NotBeNull();
            result.ExitCode.Should().NotBe(0);
            result.Success.Should().BeFalse();
        }

        [Fact]
        public void ExecuteProcess_CommandWithInput_ReturnsProcessResultWithInput()
        {
            // Arrange
            var fileName = "cat"; // cat reads from stdin
            var arguments = "";
            var input = "test input data";

            // Act
            var result = ProcessUtilities.ExecuteProcess(fileName, arguments, input: input);

            // Assert
            result.Should().NotBeNull();
            result.ExitCode.Should().Be(0);
            result.StandardOutput.Should().Contain("test input data");
        }

        [Fact]
        public async Task ExecuteProcessAsync_SuccessfulCommand_ReturnsProcessResultWithExitCodeZero()
        {
            // Arrange
            var fileName = "echo";
            var arguments = "async test output";

            // Act
            var result = await ProcessUtilities.ExecuteProcessAsync(fileName, arguments);

            // Assert
            result.Should().NotBeNull();
            result.ExitCode.Should().Be(0);
            result.StandardOutput.Should().Contain("async test output");
            result.StandardError.Should().BeEmpty();
            result.TimedOut.Should().BeFalse();
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task ExecuteProcessAsync_NonZeroExitCode_ReturnsCorrectExitCode()
        {
            // Arrange
            var fileName = OperatingSystem.IsWindows() ? "cmd.exe" : "false";
            var arguments = OperatingSystem.IsWindows() ? "/c exit 2" : "";

            // Act
            var result = await ProcessUtilities.ExecuteProcessAsync(fileName, arguments);

            // Assert
            result.Should().NotBeNull();
            result.ExitCode.Should().NotBe(0);
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task ExecuteProcessAsync_WithCancellationToken_CancelsExecution()
        {
            // Arrange
            var fileName = "sleep";
            var arguments = "10"; // Sleep for 10 seconds
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            // Act
            var result = await ProcessUtilities.ExecuteProcessAsync(fileName, arguments, cancellationToken: cts.Token);

            // Assert
            result.Should().NotBeNull();
            result.TimedOut.Should().BeTrue();
            result.ExitCode.Should().Be(-1);
            result.StandardError.Should().NotBeEmpty();
        }

        [Fact]
        public void ExecuteProcess_Timeout_ReturnsTimedOutProcessResult()
        {
            // Arrange
            var fileName = "sleep";
            var arguments = "5"; // Sleep for 5 seconds
            var timeout = TimeSpan.FromMilliseconds(100);

            // Act
            var result = ProcessUtilities.ExecuteProcess(fileName, arguments, timeout: timeout);

            // Assert
            result.Should().NotBeNull();
            result.TimedOut.Should().BeTrue();
            result.ExitCode.Should().Be(-1);
        }

        [Fact]
        public void ExecuteProcess_InvalidCommand_ThrowsInvalidOperationException()
        {
            // Arrange
            var fileName = "nonexistentcommand12345";
            var arguments = "test";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ProcessUtilities.ExecuteProcess(fileName, arguments));
        }

        [Fact]
        public void IsExecutableAvailable_ExistingExecutable_ReturnsTrue()
        {
            // Arrange - echo should exist on all platforms
            var executableName = "echo";

            // Act
            var result = ProcessUtilities.IsExecutableAvailable(executableName);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsExecutableAvailable_NonExistingExecutable_ReturnsFalse()
        {
            // Arrange
            var executableName = "nonexistentcommand_xyz123";

            // Act
            var result = ProcessUtilities.IsExecutableAvailable(executableName);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void EscapeArgument_CommandLineInjectionAttempt_ReturnsSafeArgument()
        {
            // Arrange - Test command injection prevention
            var injectionArg = "file.txt && rm -rf /";

            // Act
            var result = ProcessUtilities.EscapeArgument(injectionArg);

            // Assert - Should be wrapped in quotes to prevent injection
            result.Should().Be("\"file.txt && rm -rf /\"");
        }

        [Fact]
        public void ExecuteProcess_CapturesBothStdoutAndStderr()
        {
            // Arrange - Use a command that writes to both streams
            var fileName = "echo";
            var arguments = "stdout message";

            // Act
            var result = ProcessUtilities.ExecuteProcess(fileName, arguments);

            // Assert
            result.Should().NotBeNull();
            result.StandardOutput.Should().Contain("stdout message");
            result.StandardError.Should().BeEmpty();
        }

        [Fact]
        public async Task ExecuteProcessAsync_CapturesBothStdoutAndStderr()
        {
            // Arrange
            var fileName = "echo";
            var arguments = "async stdout message";

            // Act
            var result = await ProcessUtilities.ExecuteProcessAsync(fileName, arguments);

            // Assert
            result.Should().NotBeNull();
            result.StandardOutput.Should().Contain("async stdout message");
            result.StandardError.Should().BeEmpty();
        }
    }
}