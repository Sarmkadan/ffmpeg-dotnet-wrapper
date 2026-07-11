using System;
using FFmpegDotnetWrapper.Models;
using Xunit;

namespace FFmpegDotnetWrapper.Tests
{
    public static class SubtitleSettingsTestsExtensions
    {
        /// <summary>
        /// Creates a SubtitleSettings instance with default values.
        /// </summary>
        /// <param name="_">The assertion context (unused).</param>
        /// <returns>A new SubtitleSettings instance.</returns>
        public static SubtitleSettings WithDefaultSettings(this SubtitleSettingsTests _) => new();

        /// <summary>
        /// Asserts that setting an invalid subtitle path throws the expected exception type.
        /// </summary>
        /// <param name="assert">The assertion context (unused).</param>
        /// <param name="settings">The SubtitleSettings instance.</param>
        /// <param name="testPath">The path to test.</param>
        /// <param name="expectedExceptionType">The expected exception type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="testPath"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="expectedExceptionType"/> is null.</exception>
        public static void ShouldThrowWhenPathInvalid(this SubtitleSettingsTests assert, SubtitleSettings settings, string testPath, Type expectedExceptionType)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(testPath);
            ArgumentNullException.ThrowIfNull(expectedExceptionType);

            Assert.Throws(expectedExceptionType, () => settings.SubtitlePath = testPath);
        }

        /// <summary>
        /// Asserts that a subtitle path with a valid .srt extension is accepted.
        /// </summary>
        /// <param name="assert">The assertion context (unused).</param>
        /// <param name="settings">The SubtitleSettings instance.</param>
        /// <param name="srtPath">The path to the .srt subtitle file.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="srtPath"/> is null.</exception>
        public static void ShouldAcceptSrtFile(this SubtitleSettingsTests assert, SubtitleSettings settings, string srtPath)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(srtPath);

            settings.SubtitlePath = srtPath;
            Assert.Equal(srtPath, settings.SubtitlePath);
        }

        /// <summary>
        /// Asserts that a subtitle path with a valid .ass extension is accepted.
        /// </summary>
        /// <param name="assert">The assertion context (unused).</param>
        /// <param name="settings">The SubtitleSettings instance.</param>
        /// <param name="assPath">The path to the .ass subtitle file.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assPath"/> is null.</exception>
        public static void ShouldAcceptAssFile(this SubtitleSettingsTests assert, SubtitleSettings settings, string assPath)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(assPath);

            settings.SubtitlePath = assPath;
            Assert.Equal(assPath, settings.SubtitlePath);
        }

        /// <summary>
        /// Validates that the SubtitleSettings instance is in a valid state.
        /// </summary>
        /// <param name="assert">The assertion context (unused).</param>
        /// <param name="settings">The SubtitleSettings instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
        public static void ShouldBeValid(this SubtitleSettingsTests assert, SubtitleSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var exception = Record.Exception(settings.Validate);
            Assert.Null(exception);
        }

        /// <summary>
        /// Asserts that cloning produces an independent copy.
        /// </summary>
        /// <param name="assert">The assertion context (unused).</param>
        /// <param name="original">The original SubtitleSettings instance.</param>
        /// <param name="expectedFontSize">The expected font size value.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="original"/> is null.</exception>
        public static void ShouldProduceIndependentCopy(this SubtitleSettingsTests assert, SubtitleSettings original, int expectedFontSize)
        {
            ArgumentNullException.ThrowIfNull(original);

            var clone = original.Clone();

            Assert.Equal(original.SubtitlePath, clone.SubtitlePath);
            Assert.Equal(original.HardEmbed, clone.HardEmbed);
            Assert.Equal(original.FontSize, clone.FontSize);
            Assert.Equal(original.Language, clone.Language);

            clone.FontSize = expectedFontSize;
            Assert.NotEqual(original.FontSize, clone.FontSize);
        }
    }
}