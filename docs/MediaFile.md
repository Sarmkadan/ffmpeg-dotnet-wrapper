# MediaFile

Represents a multimedia file processed by the ffmpeg-dotnet-wrapper library, containing metadata and properties extracted from the underlying media file.

## API

### `Id`
A unique identifier for the media file. This property is read-only and set during initialization.

### `Name`
The name of the media file, typically derived from the source file name. This property is read-only and set during initialization.

### `Duration`
The duration of the media file as a `TimeSpan`, or `null` if the duration cannot be determined. This property is read-only and set during initialization.

### `Width`
The width of the video stream in pixels, or `null` if the file contains no video stream. This property is read-only and set during initialization.

### `Height`
The height of the video stream in pixels, or `null` if the file contains no video stream. This property is read-only and set during initialization.

### `FrameRate`
The frame rate of the video stream in frames per second, or `null` if the file contains no video stream. This property is read-only and set during initialization.

### `Bitrate`
The bitrate of the media file in bits per second, or `null` if the bitrate cannot be determined. This property is read-only and set during initialization.

### `VideoCodec`
The name of the video codec used in the media file, or `null` if the file contains no video stream. This property is read-only and set during initialization.

### `AudioCodec`
The name of the audio codec used in the media file, or `null` if the file contains no audio stream. This property is read-only and set during initialization.

### `AudioSampleRate`
The sample rate of the audio stream in Hertz, or `null` if the file contains no audio stream. This property is read-only and set during initialization.

### `AudioChannels`
The number of audio channels in the media file, or `null` if the file contains no audio stream. This property is read-only and set during initialization.

### `CreatedAt`
The timestamp indicating when the media file was first processed or created in the system. This property is read-only and set during initialization.

### `ModifiedAt`
The timestamp indicating when the media file was last modified or reprocessed, or `null` if the file has never been modified. This property is read-only and set during initialization.

### `Description`
A user-provided description of the media file, or `null` if no description is set. This property is read-only and set during initialization.

### `Metadata`
A dictionary of additional metadata extracted from the media file, where keys are metadata tags and values are the corresponding metadata values. This property is read-only and set during initialization.

### `ValidateAsVideo()`
Validates that the media file contains a valid video stream. Throws an exception if the file contains no video stream or if the video stream is invalid.

### `ValidateAsAudio()`
Validates that the media file contains a valid audio stream. Throws an exception if the file contains no audio stream or if the audio stream is invalid.

### `GetFileSizeInMegabytes()`
Calculates and returns the size of the media file in megabytes. The value is computed at the time of the call and may not reflect the current file size on disk.

## Usage
