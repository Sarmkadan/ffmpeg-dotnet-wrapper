using FFmpegDotnetWrapper.Api.DTOs;
using Xunit;
using System;

namespace FFmpegDotnetWrapper.Tests
{
    public class ApiRequestTests
    {
        [Fact]
        public void TranscodeRequest_HasDefaultValues()
        {
            // Arrange & Act
            var request = new TranscodeRequest();

            // Assert
            Assert.NotNull(request.RequestId);
            Assert.NotEqual(Guid.Empty, Guid.Parse(request.RequestId));
            Assert.InRange(request.CreatedAt, DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
            Assert.Null(request.CorrelationId);
            Assert.Null(request.TenantId);
            Assert.Equal(string.Empty, request.InputPath);
            Assert.Equal(string.Empty, request.OutputPath);
            Assert.Equal("mp4", request.OutputFormat);
            Assert.Null(request.Codec);
            Assert.Null(request.Bitrate);
            Assert.Null(request.Quality);
        }

        [Fact]
        public void TranscodeRequest_CanSetAndGetProperties()
        {
            // Arrange
            var request = new TranscodeRequest
            {
                RequestId = "test-request-id",
                CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                CorrelationId = "test-correlation-id",
                TenantId = "test-tenant-id",
                InputPath = "/test/input.mp4",
                OutputPath = "/test/output.mp4",
                OutputFormat = "mkv",
                Codec = "h265",
                Bitrate = 2500,
                Quality = 23
            };

            // Act & Assert
            Assert.Equal("test-request-id", request.RequestId);
            Assert.Equal(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc), request.CreatedAt);
            Assert.Equal("test-correlation-id", request.CorrelationId);
            Assert.Equal("test-tenant-id", request.TenantId);
            Assert.Equal("/test/input.mp4", request.InputPath);
            Assert.Equal("/test/output.mp4", request.OutputPath);
            Assert.Equal("mkv", request.OutputFormat);
            Assert.Equal("h265", request.Codec);
            Assert.Equal(2500, request.Bitrate);
            Assert.Equal(23, request.Quality);
        }

        [Fact]
        public void TranscodeRequest_CanSetNullablePropertiesToNull()
        {
            // Arrange
            var request = new TranscodeRequest
            {
                RequestId = "test-request-id",
                CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                CorrelationId = null,
                TenantId = null,
                InputPath = "/test/input.mp4",
                OutputPath = "/test/output.mp4",
                OutputFormat = "mkv",
                Codec = null,
                Bitrate = null,
                Quality = null
            };

            // Act & Assert
            Assert.Equal("test-request-id", request.RequestId);
            Assert.Equal(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc), request.CreatedAt);
            Assert.Null(request.CorrelationId);
            Assert.Null(request.TenantId);
            Assert.Equal("/test/input.mp4", request.InputPath);
            Assert.Equal("/test/output.mp4", request.OutputPath);
            Assert.Equal("mkv", request.OutputFormat);
            Assert.Null(request.Codec);
            Assert.Null(request.Bitrate);
            Assert.Null(request.Quality);
        }

        [Fact]
        public void TranscodeRequest_Validation_RequiresInputPath()
        {
            // Arrange
            var request = new TranscodeRequest
            {
                RequestId = "test-request-id",
                CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                InputPath = string.Empty, // Empty string should trigger validation
                OutputPath = "/test/output.mp4"
            };

            // Act & Assert
            Assert.Throws<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            {
                // Trigger validation by trying to validate the object
                var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
                System.ComponentModel.DataAnnotations.Validator.ValidateObject(request, context, validateAllProperties: true);
            });
        }

        [Fact]
        public void TranscodeRequest_Validation_RequiresOutputPath()
        {
            // Arrange
            var request = new TranscodeRequest
            {
                RequestId = "test-request-id",
                CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                InputPath = "/test/input.mp4",
                OutputPath = string.Empty // Empty string should trigger validation
            };

            // Act & Assert
            Assert.Throws<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            {
                // Trigger validation by trying to validate the object
                var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
                System.ComponentModel.DataAnnotations.Validator.ValidateObject(request, context, validateAllProperties: true);
            });
        }

        [Fact]
        public void TranscodeRequest_Validation_BitrateRange()
        {
            // Arrange
            var request = new TranscodeRequest
            {
                RequestId = "test-request-id",
                CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                InputPath = "/test/input.mp4",
                OutputPath = "/test/output.mp4",
                Bitrate = 0 // Below minimum of 1
            };

            // Act & Assert
            Assert.Throws<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            {
                // Trigger validation by trying to validate the object
                var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
                System.ComponentModel.DataAnnotations.Validator.ValidateObject(request, context, validateAllProperties: true);
            });
        }

        [Fact]
        public void TranscodeRequest_Validation_QualityRange()
        {
            // Arrange
            var request = new TranscodeRequest
            {
                RequestId = "test-request-id",
                CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                InputPath = "/test/input.mp4",
                OutputPath = "/test/output.mp4",
                Quality = 52 // Above maximum of 51
            };

            // Act & Assert
            Assert.Throws<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            {
                // Trigger validation by trying to validate the object
                var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
                System.ComponentModel.DataAnnotations.Validator.ValidateObject(request, context, validateAllProperties: true);
            });
        }
    }
}