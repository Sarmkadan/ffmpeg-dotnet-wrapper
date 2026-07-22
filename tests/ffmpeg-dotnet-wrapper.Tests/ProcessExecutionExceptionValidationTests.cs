using System;
using System.Collections.Generic;
using FFmpegDotnetWrapper.Exceptions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests
{
    public class ProcessExecutionExceptionValidationTests
    {
        private ProcessExecutionException CreateException(
            string message = "Test message",
            int? exitCode = null,
            string errorOutput = null)
        {
            var ex = new ProcessExecutionException(message);
            ex.ExitCode = exitCode;
            ex.ErrorOutput = errorOutput;
            return ex;
        }

        [Fact]
        public void Validate_HappyPath_NoProblems()
        {
            var ex = CreateException(); // message set, ExitCode null, ErrorOutput null
            var problems = ex.Validate();

            Assert.Empty(problems);
        }

        [Fact]
        public void Validate_ExitCodeSetWithErrorOutput_NoProblems()
        {
            var ex = CreateException(exitCode: 0, errorOutput: "Some error");
            var problems = ex.Validate();

            Assert.Empty(problems);
        }

        [Fact]
        public void Validate_ExitCodeNegative_ReturnsProblem()
        {
            var ex = CreateException(exitCode: -1, errorOutput: "Error");
            var problems = ex.Validate();

            Assert.Contains("ExitCode must be a non-negative integer when set.", problems);
        }

        [Fact]
        public void Validate_ExitCodeSetWithoutErrorOutput_ReturnsProblem()
        {
            var ex = CreateException(exitCode: 1, errorOutput: null);
            var problems = ex.Validate();

            Assert.Contains("ErrorOutput must be provided when ExitCode is set.", problems);
        }

        [Fact]
        public void Validate_MessageNullOrWhiteSpace_ReturnsProblem()
        {
            var ex = CreateException(message: "   ");
            var problems = ex.Validate();

            Assert.Contains("Message cannot be null, empty, or whitespace.", problems);
        }

        [Fact]
        public void Validate_NullException_ThrowsArgumentNullException()
        {
            ProcessExecutionException ex = null;
            Assert.Throws<ArgumentNullException>(() => ex.Validate());
        }

        [Fact]
        public void IsValid_ReturnsTrueForValidException()
        {
            var ex = CreateException();
            Assert.True(ex.IsValid());
        }

        [Fact]
        public void IsValid_ReturnsFalseForInvalidException()
        {
            var ex = CreateException(exitCode: -5, errorOutput: null);
            Assert.False(ex.IsValid());
        }

        [Fact]
        public void EnsureValid_NoExceptionWhenValid()
        {
            var ex = CreateException();
            var exception = Record.Exception(() => ex.EnsureValid());
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureValid_ThrowsArgumentExceptionWhenInvalid()
        {
            var ex = CreateException(exitCode: -1, errorOutput: null);
            var exception = Assert.Throws<ArgumentException>(() => ex.EnsureValid());

            Assert.Contains("ProcessExecutionException is invalid", exception.Message);
            Assert.Contains("ExitCode must be a non-negative integer when set.", exception.Message);
            Assert.Contains("ErrorOutput must be provided when ExitCode is set.", exception.Message);
        }

        [Fact]
        public void EnsureValid_NullException_ThrowsArgumentNullException()
        {
            ProcessExecutionException ex = null;
            Assert.Throws<ArgumentNullException>(() => ex.EnsureValid());
        }
    }
}
