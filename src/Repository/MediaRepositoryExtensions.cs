using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Models;
using System.Diagnostics.CodeAnalysis;

namespace FFmpegDotnetWrapper.Repository
{
    /// <summary>
    /// Extension methods that add convenient, higher-level operations on top of <see cref="MediaRepository"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class MediaRepositoryExtensions
    {
        /// <summary>
        /// Retrieves all media files as a materialized read-only list.
        /// </summary>
        /// <param name="repository">The repository instance.</param>
        /// <returns>A read-only list containing all media files.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
        public static async Task<IReadOnlyList<MediaFile>> GetAllMediaFilesAsync(this MediaRepository repository)
        {
            ArgumentNullException.ThrowIfNull(repository);
            var all = await repository.GetAllAsync().ConfigureAwait(false);
            return all.ToList();
        }

        /// <summary>
        /// Returns the total number of media records stored in the repository.
        /// </summary>
        /// <param name="repository">The repository instance.</param>
        /// <returns>The count of media records.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
        public static async Task<int> GetMediaCountAsync(this MediaRepository repository)
        {
            ArgumentNullException.ThrowIfNull(repository);
            return await repository.GetCountAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Adds the supplied <paramref name="media"/> if a file with the same path does not already exist;
        /// otherwise updates the existing record and returns the updated entity.
        /// </summary>
        /// <param name="repository">The repository instance.</param>
        /// <param name="media">The media file to add or update.</param>
        /// <returns>The added or updated media file.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> or <paramref name="media"/> is <see langword="null"/>.</exception>
        public static async Task<MediaFile> AddOrUpdateByFilePathAsync(this MediaRepository repository, MediaFile media)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(media);

            var existing = await repository.GetByFilePathAsync(media.FilePath).ConfigureAwait(false);
            return existing is null
                ? await repository.AddAsync(media).ConfigureAwait(false)
                : await repository.UpdateAsync(media).ConfigureAwait(false);
        }

        /// <summary>
        /// Searches for media files by name, returning an empty list when no matches are found.
        /// </summary>
        /// <param name="repository">The repository instance.</param>
        /// <param name="name">The name to search for.</param>
        /// <returns>A read-only list of matching media files; empty if no matches found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        public static async Task<IReadOnlyList<MediaFile>> SearchByNameOrEmptyAsync(this MediaRepository repository, string name)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(name);

            var results = await repository.SearchByNameAsync(name).ConfigureAwait(false);
            return results?.ToList() ?? new List<MediaFile>();
        }
    }
}
