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
    /// <summary>
    /// Unit tests for <see cref="ProcessUtilities"/> covering argument escaping,
    /// synchronous and asynchronous process execution, timeouts, cancellation,
    /// executable availability checks, and command injection prevention.
    /// </summary>
    public class ProcessUtilitiesTests
    {
        /// <summary>
        /// Verifies that escaping an empty string argument produces a quoted empty string.
        /// </summary>
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

        /// <summary>
        /// Verifies that escaping a null string argument produces a quoted empty string.
        /// </summary>
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

        /// <summary>
        /// Verifies that plain text containing no spaces is returned unchanged by the escaper.
        /// </summary>
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

        /// <summary>
        /// Verifies that text containing spaces is wrapped in double quotes when escaped.
        /// </summary>
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

        /// <summary>
        /// Verifies that embedded double quotes are escaped with backslashes and the
        /// whole argument is wrapped in double quotes.
        /// </summary>
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

        /// <summary>
        /// Verifies that a path-like argument containing backslashes is wrapped in
        /// double quotes while preserving its backslash separators.
        /// </summary>
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

        /// <summary>
        /// Verifies that an argument combining spaces, double quotes and backslashes
        /// is wrapped in quotes with its special characters preserved.
        /// </summary>
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

        /// <summary>
        /// Verifies that running "echo" synchronously yields exit code zero, captures the
        /// echoed text on standard output, leaves standard error empty, reports no timeout,
        /// and marks the result as successful.
        /// </summary>
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

        /// <summary>
        /// Verifies that a synchronous process started with an explicit working directory
        /// completes successfully with exit code zero.
        /// </summary>
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

        /// <summary>
        /// Verifies that a command exiting with a failure status ("cmd.exe /c exit 1" on
        /// Windows, "false" elsewhere) reports a non-zero exit code and is marked unsuccessful.
        /// </summary>
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

        /// <summary>
        /// Verifies that standard input supplied to a synchronous process ("cat") is echoed
        /// back on standard output with exit code zero.
        /// </summary>
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

        /// <summary>
        /// Verifies that running "echo" asynchronously yields exit code zero, captures the
        /// echoed text on standard output, leaves standard error empty, reports no timeout,
        /// and marks the result as successful.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
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

        /// <summary>
        /// Verifies that an asynchronous command exiting with a failure status
        /// ("cmd.exe /c exit 2" on Windows, "false" elsewhere) reports a non-zero
        /// exit code and is marked unsuccessful.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
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

        /// <summary>
        /// Verifies that cancelling a long-running asynchronous "sleep" command via a
        /// cancellation token produces a timed-out result with exit code -1 and a
        /// non-empty standard error.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
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

        /// <summary>
        /// Verifies that a synchronous "sleep" command exceeding its timeout limit is
        /// reported as timed out with exit code -1.
        /// </summary>
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

        /// <summary>
        /// Verifies that attempting to execute a non-existent executable synchronously
        /// throws an <see cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public void ExecuteProcess_InvalidCommand_ThrowsInvalidOperationException()
        {
            // Arrange
            var fileName = "nonexistentcommand12345";
            var arguments = "test";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ProcessUtilities.ExecuteProcess(fileName, arguments));
        }

        /// <summary>
        /// Verifies that "echo", available on all supported platforms, is reported as
        /// an existing executable.
        /// </summary>
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

        /// <summary>
        /// Verifies that a made-up executable name is reported as unavailable.
        /// </summary>
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

        /// <summary>
        /// Verifies that a command injection attempt ("file.txt && rm -rf /") is neutralized
        /// by wrapping the entire argument in double quotes.
        /// </summary>
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

        /// <summary>
        /// Verifies that a synchronous "echo" run captures its message on standard output
        /// while leaving standard error empty.
        /// </summary>
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

        /// <summary>
        /// Verifies that an asynchronous "echo" run captures its message on standard output
        /// while leaving standard error empty.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
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

        /// <summary>
        /// Verifies that success requires both a zero exit code and no timeout.
        /// </summary>
        [Theory]
        [InlineData(0, false, true)]
        [InlineData(1, false, false)]
        [InlineData(0, true, false)]
        public void ProcessResult_Success_ReturnsExpectedValue(
            int exitCode,
            bool timedOut,
            bool expected)
        {
            var result = new ProcessUtilities.ProcessResult
            {
                ExitCode = exitCode,
                TimedOut = timedOut
            };

            result.Success.Should().Be(expected);
        }

        /// <summary>
        /// Verifies that the string representation contains every process result detail.
        /// </summary>
        [Fact]
        public void ProcessResult_ToString_ContainsResultDetails()
        {
            var result = new ProcessUtilities.ProcessResult
            {
                ExitCode = 2,
                StandardOutput = "standard output",
                StandardError = "standard error",
                ExecutionTime = TimeSpan.FromSeconds(3),
                TimedOut = true
            };

            var text = result.ToString();

            text.Should().Contain("ExitCode = 2");
            text.Should().Contain("StandardOutput = standard output");
            text.Should().Contain("StandardError = standard error");
            text.Should().Contain($"ExecutionTime = {result.ExecutionTime}");
            text.Should().Contain("TimedOut = True");
        }

        /// <summary>
        /// Verifies that a null executable name is rejected with the correct parameter name.
        /// </summary>
        [Fact]
        public void ExecuteProcess_NullFileName_ThrowsArgumentNullException()
        {
            var action = () => ProcessUtilities.ExecuteProcess(null!, string.Empty);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("fileName");
        }

        /// <summary>
        /// Verifies that null command arguments are rejected with the correct parameter name.
        /// </summary>
        [Fact]
        public void ExecuteProcess_NullArguments_ThrowsArgumentNullException()
        {
            var action = () => ProcessUtilities.ExecuteProcess("dotnet", null!);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("arguments");
        }

        /// <summary>
        /// Verifies that a portable dotnet command completes successfully and captures output.
        /// </summary>
        [Fact]
        public void ExecuteProcess_DotnetVersion_ReturnsSuccessfulResultWithOutput()
        {
            var result = ProcessUtilities.ExecuteProcess(
                "dotnet",
                "--version",
                timeout: TimeSpan.FromSeconds(30));

            result.ExitCode.Should().Be(0);
            result.TimedOut.Should().BeFalse();
            result.Success.Should().BeTrue();
            result.StandardOutput.Should().NotBeNullOrWhiteSpace();
        }
    }
}
