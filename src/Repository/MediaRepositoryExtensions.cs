using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Repository
{
    /// <summary>
    /// Extension methods that add convenient, higher‑level operations on top of <see cref="MediaRepository"/>.
    /// </summary>
    public static class MediaRepositoryExtensions
    {
        /// <summary>
        /// Retrieves all media files as a materialised read‑only list.
        /// </summary>
        public static async Task<IReadOnlyList<MediaFile>> GetAllMediaFilesAsync(this MediaRepository repository)
        {
            var all = await repository.GetAllAsync().ConfigureAwait(false);
            return all.ToList();
        }

        /// <summary>
        /// Returns the total number of media records stored in the repository.
        /// </summary>
        public static async Task<int> GetMediaCountAsync(this MediaRepository repository)
        {
            return await repository.GetCountAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Adds the supplied <paramref name="media"/> if a file with the same path does not already exist;
        /// otherwise updates the existing record and returns the updated entity.
        /// </summary>
        public static async Task<MediaFile> AddOrUpdateByFilePathAsync(this MediaRepository repository, MediaFile media)
        {
            if (media == null) throw new ArgumentNullException(nameof(media));

            var existing = await repository.GetByFilePathAsync(media.FilePath).ConfigureAwait(false);
            return existing is null
                ? await repository.AddAsync(media).ConfigureAwait(false)
                : await repository.UpdateAsync(media).ConfigureAwait(false);
        }

        /// <summary>
        /// Searches for media files by name, returning an empty list when no matches are found.
        /// </summary>
        public static async Task<IReadOnlyList<MediaFile>> SearchByNameOrEmptyAsync(this MediaRepository repository, string name)
        {
            var results = await repository.SearchByNameAsync(name).ConfigureAwait(false);
            return results?.ToList() ?? new List<MediaFile>();
        }
    }
}
