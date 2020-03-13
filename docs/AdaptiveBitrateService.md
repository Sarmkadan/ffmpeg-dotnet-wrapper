# AdaptiveBitrateService

Orchestrates adaptive bitrate (ABR) streaming pipelines by managing the initialization, encoding, and retrieval of segmented media renditions. It provides asynchronous enumeration over generated streaming segments and supports cancellation of in-progress pipelines.

## API

### `public AdaptiveBitrateService`

Constructs a new instance of the service. No configuration or dependencies are exposed through the constructor signature.

### `public async IAsyncEnumerable<StreamingSegment> RunPipelineAsync`

Executes the full ABR pipeline from initialization through encoding, yielding each `StreamingSegment` as it becomes available. The method combines initialization and rendition encoding into a single consumable async sequence.

**Returns:** An asynchronous stream of `StreamingSegment` instances representing the output of the pipeline.

**Exceptions:** Throws when pipeline initialization or encoding fails. The specific exception types depend on the underlying implementation and input validity.

### `public Task<string> InitialisePipelineAsync`

Prepares the pipeline for execution. This may involve validating input parameters, allocating resources, or establishing the processing context required before encoding can begin.

**Returns:** A task that resolves to a string token or identifier representing the initialized pipeline state.

**Exceptions:** Throws if initialization prerequisites are not met, such as invalid configuration or resource unavailability.

### `public async IAsyncEnumerable<StreamingSegment> EncodeRenditionAsync`

Encodes a specific rendition and yields the resulting streaming segments asynchronously. This method assumes the pipeline has already been initialized.

**Returns:** An asynchronous stream of `StreamingSegment` instances for the requested rendition.

**Exceptions:** Throws if the pipeline has not been initialized, if encoding parameters are invalid, or if an error occurs during the encoding process.

### `public Task<StreamingPipelineResult?> GetPipelineResultAsync`

Retrieves the final result of a completed pipeline execution. The result may be `null` if the pipeline has not yet finished or was cancelled.

**Returns:** A task that resolves to a `StreamingPipelineResult?` containing output metadata, or `null` if no result is available.

**Exceptions:** Throws if an error occurs while assembling or accessing the pipeline result.

### `public Task<bool> CancelPipelineAsync`

Requests cancellation of the currently running pipeline. The method returns a boolean indicating whether the cancellation request was successfully registered.

**Returns:** A task that resolves to `true` if the cancellation was accepted, `false` if no pipeline was active or cancellation could not be honoured.

**Exceptions:** Throws if an unexpected error occurs during the cancellation process.

## Usage

### Example 1: Full Pipeline Execution with Cancellation Support

```csharp
var service = new AdaptiveBitrateService();
using var cts = new CancellationTokenSource();

try
{
    await foreach (var segment in service.RunPipelineAsync().WithCancellation(cts.Token))
    {
        Console.WriteLine($"Segment: {segment.Name}, Duration: {segment.Duration}");
        await UploadSegmentAsync(segment);

        if (ShouldAbort())
        {
            await service.CancelPipelineAsync();
            break;
        }
    }

    var result = await service.GetPipelineResultAsync();
    if (result is not null)
    {
        Console.WriteLine($"Pipeline complete. Manifest: {result.ManifestPath}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Pipeline was cancelled.");
}
```

### Example 2: Stepwise Initialization and Encoding

```csharp
var service = new AdaptiveBitrateService();

string pipelineId = await service.InitialisePipelineAsync();
Console.WriteLine($"Pipeline initialized: {pipelineId}");

await foreach (var segment in service.EncodeRenditionAsync())
{
    await StoreSegmentAsync(pipelineId, segment);
}

var result = await service.GetPipelineResultAsync();
if (result is null)
{
    Console.WriteLine("No result available — pipeline may have been cancelled or failed.");
}
else
{
    Console.WriteLine($"Encoding finished. Output: {result.OutputDirectory}");
}
```

## Notes

- **Thread safety:** The service is designed for single-consumer scenarios. Concurrent calls to `RunPipelineAsync`, `EncodeRenditionAsync`, or `CancelPipelineAsync` on the same instance may lead to undefined behaviour or race conditions. Synchronise access externally if multiple threads must interact with a shared instance.
- **Cancellation timing:** `CancelPipelineAsync` requests cancellation but does not guarantee immediate termination. Segments already in flight may still be yielded by the async enumerator before the cancellation takes effect.
- **Null result:** `GetPipelineResultAsync` returns `null` when the pipeline has not reached a terminal state (completed or cancelled). Always null-check the return value before accessing its members.
- **Initialization dependency:** `EncodeRenditionAsync` requires a successful prior call to `InitialisePipelineAsync`. Calling it without initialization will throw.
- **Resource cleanup:** The async enumerables returned by `RunPipelineAsync` and `EncodeRenditionAsync` may hold unmanaged resources until the enumeration completes or is disposed. Ensure the enumeration is consumed fully or disposed of appropriately to avoid resource leaks.
