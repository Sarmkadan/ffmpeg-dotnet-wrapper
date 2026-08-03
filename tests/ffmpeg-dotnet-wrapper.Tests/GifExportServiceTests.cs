namespace ffmpeg_dotnet_wrapper_tests
{
    using Xunit;
    using FFmpegDotnetWrapper.Services;
    using FFmpegDotnetWrapper.Models;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class GifExportServiceTests
    {
        [Fact]
        public async Task ExportGifAsync_NullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new GifExportService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                service.ExportGifAsync("source.mp4", TimeSpan.Zero, TimeSpan.FromSeconds(1), null!));
        }

        [Fact]
        public async Task ExportGifAsync_SourceFileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var service = new GifExportService();
            var settings = new GifExportSettings();

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                service.ExportGifAsync("non_existent_file.mp4", TimeSpan.Zero, TimeSpan.FromSeconds(1), settings));
        }

        [Fact]
        public async Task ExportGifAsync_DefaultSettings_SourceFileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var service = new GifExportService();

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                service.ExportGifAsync("non_existent_file.mp4", TimeSpan.Zero, TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public void GifExportSettings_DefaultValues_AreCorrect()
        {
            // Arrange
            var settings = new GifExportSettings();

            // Assert
            Assert.Equal(10, settings.Fps);
            Assert.Equal(640, settings.Width);
            Assert.Equal(DitherMode.Sierra2_4a, settings.DitherMode);
            Assert.Equal(-1, settings.Loop);
        }
    }
}
