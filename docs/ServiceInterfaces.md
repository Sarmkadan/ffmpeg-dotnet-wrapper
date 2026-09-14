# Service Interfaces

This document provides an overview of the service interfaces in the FFmpegDotnetWrapper library.

## IAdaptiveBitrateService

The `IAdaptiveBitrateService` interface orchestrates adaptive bitrate (ABR) streaming pipelines by managing the initialization, encoding, and retrieval of segmented media renditions. It provides asynchronous enumeration over generated streaming segments and supports cancellation of in-progress pipelines.

For detailed documentation, see [AdaptiveBitrateService](./AdaptiveBitrateService.md).

## IWatermarkService

The `IWatermarkService` interface provides convenience methods for applying watermarks to videos using predefined positions or custom settings. The service handles validation, logging, and delegates the actual watermarking operation to `IFFmpegService`.

For detailed documentation, see [WatermarkService](./WatermarkService.md).