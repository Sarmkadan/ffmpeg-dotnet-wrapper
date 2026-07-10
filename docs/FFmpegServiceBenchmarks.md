# FFmpegServiceBenchmarks

`FFmpegServiceBenchmarks` is a benchmarking suite designed to evaluate the performance and functionality of the `FFmpegService` class within the `ffmpeg-dotnet-wrapper` project. This class provides a series of benchmark methods that test common FFmpeg operations, including transcoding, metadata analysis, stream manipulation, and hardware acceleration. The benchmarks are intended for performance measurement, regression testing, and validation of the wrapper's capabilities under various scenarios.

---

## API

### `public void GlobalSetup()`
**Purpose**: Initializes shared resources required for all benchmark tests, such as temporary directories, input files, or FFmpeg configurations. This method is executed once before any benchmark runs.
**Parameters**: None.
**Return Value**: `void`.
**Throws**:
- `IOException` if required input files cannot be created or accessed.
- `FFmpegException` if FFmpeg initialization fails.

---

### `public void GlobalCleanup()`
**Purpose**: Releases resources allocated during `GlobalSetup` and cleans up temporary files or directories. This method is executed once after all benchmarks complete.
**Parameters**: None.
**Return Value**: `void`.
**Throws**:
- `IOException` if temporary files cannot be deleted.

---

### `public async Task Transcode_H264_to_H265_MP4()`
**Purpose**: Benchmarks transcoding a video from H.264 to H.265 (HEVC) in an MP4 container. Measures the time taken and validates the output file.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**:
- `FFmpegException` if transcoding fails (e.g., invalid codec, missing hardware support).
- `FileNotFoundException` if the input file is missing.

---

### `public async Task Transcode_H264_to_VP9_WebM()`
**Purpose**: Benchmarks transcoding a video from H.264 to VP9 in a WebM container. Useful for evaluating performance with royalty-free codecs.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**:
- `FFmpegException` if transcoding fails (e.g., unsupported codec parameters).
- `FileNotFoundException` if the input file is missing.

---

### `public async Task Transcode_With_Hardware_Acceleration()`
**Purpose**: Benchmarks transcoding with hardware acceleration enabled (e.g., NVENC, QSV, or VA-API). Validates whether hardware-accelerated pipelines reduce processing time.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**:
- `FFmpegException` if hardware acceleration is unavailable or fails.
- `NotSupportedException` if the system lacks compatible hardware/drivers.

---

### `public async Task Trim_Video_StreamCopy()`
**Purpose**: Benchmarks trimming a video while copying streams (no re-encoding). Measures the speed of stream extraction and container remuxing.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**:
- `FFmpegException` if stream copying fails (e.g., invalid timestamps).
- `FileNotFoundException` if the input file is missing.

---

### `public async Task Analyze_Media_Metadata()`
**Purpose**: Benchmarks reading and parsing media metadata (e.g., duration, codecs, resolution). Evaluates the performance of metadata extraction.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**:
- `FFmpegException` if metadata parsing fails.
- `FileNotFoundException` if the input file is missing.

---

### `public async Task Extract_Thumbnails()`
**Purpose**: Benchmarks extracting multiple thumbnails from a video at specified intervals. Measures the speed of frame extraction and image encoding.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**:
- `FFmpegException` if thumbnail extraction fails (e.g., invalid timestamps).
- `DirectoryNotFoundException` if the output directory is missing.

---

### `public async Task Merge_Multiple_Videos()`
**Purpose**: Benchmarks merging multiple video files into a single output. Tests concatenation performance and container compatibility.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**:
- `FFmpegException` if merging fails (e.g., incompatible codecs).
- `FileNotFoundException` if input files are missing.

---

### `public async Task Extract_Audio_Only()`
**Purpose**: Benchmarks extracting audio from a video file and saving it as a standalone audio file (e.g., MP3, AAC). Measures demuxing and audio encoding performance.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**:
- `FFmpegException` if audio extraction fails.
- `FileNotFoundException` if the input file is missing.

---

### `public async Task Add_Watermark()`
**Purpose**: Benchmarks adding a watermark (e.g., PNG overlay) to a video. Evaluates the performance of video filtering and overlay operations.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**:
- `FFmpegException` if watermarking fails (e.g., invalid filter graph).
- `FileNotFoundException` if the input video or watermark image is missing.

---

### `public async Task Batch_Transcode_Multiple_Files()`
**Purpose**: Benchmarks transcoding multiple files sequentially or in parallel. Tests the scalability of the `FFmpegService` when processing batch operations.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**:
- `AggregateException` if one or more transcoding operations fail.
- `DirectoryNotFoundException` if input/output directories are missing.

---

## Usage

### Example 1: Running a Single Benchmark
