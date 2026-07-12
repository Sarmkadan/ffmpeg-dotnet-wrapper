using System;
using System.Collections.Generic;

namespace FFmpegDotnetWrapper.Cli
{
    /// <summary>
    /// Provides extension methods for <see cref="CliCommand"/>.
    /// </summary>
    public static class CliCommandExtensions
    {
        /// <summary>
        /// Checks if the command has the specified option.
        /// </summary>
        /// <param name="command">The CLI command.</param>
        /// <param name="optionName">The name of the option.</param>
        /// <returns><c>true</c> if the option exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="optionName"/> is null or empty.</exception>
        public static bool HasOption(this CliCommand command, string optionName)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(optionName);

            return command.Options.ContainsKey(optionName);
        }

        /// <summary>
        /// Gets the value of the specified option.
        /// </summary>
        /// <param name="command">The CLI command.</param>
        /// <param name="optionName">The name of the option.</param>
        /// <returns>The value of the option, or <c>null</c> if not set or not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="optionName"/> is null or empty.</exception>
        public static string? GetOptionValue(this CliCommand command, string optionName)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(optionName);

            return command.Options.TryGetValue(optionName, out var value) ? value : null;
        }

        /// <summary>
        /// Tries to get the value of the specified option.
        /// </summary>
        /// <param name="command">The CLI command.</param>
        /// <param name="optionName">The name of the option.</param>
        /// <param name="value">The value of the option, if found.</param>
        /// <returns><c>true</c> if the option exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="optionName"/> is null or empty.</exception>
        public static bool TryGetOptionValue(this CliCommand command, string optionName, out string? value)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(optionName);

            return command.Options.TryGetValue(optionName, out value);
        }
    }
}
