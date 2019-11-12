// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Events
{
    /// <summary>
    /// Base class for all events in the FFmpeg wrapper system.
    /// Provides timestamps and correlation IDs for tracking events across distributed systems.
    /// </summary>
    public abstract class FFmpegEvent
    {
        /// <summary>
        /// Unique identifier for this event instance.
        /// Useful for deduplication and idempotent event processing.
        /// </summary>
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Timestamp when the event occurred (server time).
        /// All times are in UTC for consistency across time zones.
        /// </summary>
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Correlation ID linking related events in a workflow.
        /// Enables tracking of composite operations like "transcode → watermark → upload".
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Optional source identifier indicating which operation triggered this event.
        /// Examples: "TranscodeService", "WatermarkService", "BatchProcessor".
        /// </summary>
        public string? Source { get; set; }
    }

    /// <summary>
    /// Event raised when a video processing operation starts.
    /// </summary>
    public class OperationStartedEvent : FFmpegEvent
    {
        public string InputFile { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Event raised when a video processing operation completes successfully.
    /// </summary>
    public class OperationCompletedEvent : FFmpegEvent
    {
        public string InputFile { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public long OutputFileSize { get; set; }
    }

    /// <summary>
    /// Event raised when a video processing operation fails.
    /// </summary>
    public class OperationFailedEvent : FFmpegEvent
    {
        public string InputFile { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? StackTrace { get; set; }
    }

    /// <summary>
    /// Event raised to report progress during long-running operations.
    /// </summary>
    public class ProgressReportedEvent : FFmpegEvent
    {
        public string OperationType { get; set; } = string.Empty;
        public double ProgressPercentage { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public string? StatusMessage { get; set; }
    }

    /// <summary>
    /// Interface for event handlers that listen to specific event types.
    /// Implementations handle events asynchronously and can perform side effects.
    /// </summary>
    public interface IEventHandler<in TEvent> where TEvent : FFmpegEvent
    {
        Task HandleAsync(TEvent @event);
    }

    /// <summary>
    /// Event publisher using pub-sub pattern for decoupled event handling.
    /// Subscribers register handlers for specific event types.
    /// Publisher notifies all registered handlers when events occur.
    /// </summary>
    public interface IEventPublisher
    {
        void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : FFmpegEvent;
        void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : FFmpegEvent;
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : FFmpegEvent;
    }

    public class EventPublisher : IEventPublisher
    {
        private readonly ILogger<EventPublisher> _logger;
        private readonly Dictionary<Type, List<object>> _subscribers = new();
        private readonly object _lockObject = new();

        public EventPublisher(ILogger<EventPublisher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers an event handler for a specific event type.
        /// The same handler can be registered multiple times (will be called multiple times).
        /// Returns a subscription ID that can be used to unsubscribe later.
        /// </summary>
        public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : FFmpegEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lockObject)
            {
                var eventType = typeof(TEvent);
                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new List<object>();
                }

                _subscribers[eventType].Add(handler);
                _logger.LogDebug(
                    "Event handler subscribed: {EventType} -> {HandlerType}",
                    eventType.Name,
                    handler.GetType().Name);
            }
        }

        /// <summary>
        /// Unregisters an event handler from a specific event type.
        /// Removes only the first occurrence if registered multiple times.
        /// </summary>
        public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : FFmpegEvent
        {
            if (handler == null)
                return;

            lock (_lockObject)
            {
                var eventType = typeof(TEvent);
                if (_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers.Remove(handler);
                    _logger.LogDebug(
                        "Event handler unsubscribed: {EventType} -> {HandlerType}",
                        eventType.Name,
                        handler.GetType().Name);
                }
            }
        }

        /// <summary>
        /// Publishes an event to all registered subscribers.
        /// Handles both sync and async operations, ensuring all handlers are called.
        /// Catches and logs exceptions from individual handlers to prevent cascade failures.
        /// </summary>
        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : FFmpegEvent
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var eventType = typeof(TEvent);
            List<object>? handlers;

            lock (_lockObject)
            {
                if (!_subscribers.TryGetValue(eventType, out handlers))
                {
                    _logger.LogDebug("No subscribers for event type: {EventType}", eventType.Name);
                    return;
                }

                // Create a copy to avoid modification during iteration
                handlers = new List<object>(handlers);
            }

            _logger.LogDebug(
                "Publishing event: {EventType} (Subscribers: {Count})",
                eventType.Name,
                handlers.Count);

            var tasks = new List<Task>();

            foreach (var handler in handlers)
            {
                try
                {
                    // Get the generic method on the handler
                    var handlerType = handler.GetType();
                    var handleMethod = handlerType.GetMethod(nameof(IEventHandler<FFmpegEvent>.HandleAsync));

                    if (handleMethod != null)
                    {
                        var task = (Task)handleMethod.Invoke(handler, new object[] { @event })!;
                        tasks.Add(task);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error invoking event handler for {EventType}",
                        eventType.Name);
                }
            }

            // Wait for all handlers to complete
            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiting for event handlers to complete");
            }
        }

        /// <summary>
        /// Gets the number of registered subscribers for a specific event type.
        /// Useful for testing and debugging event system state.
        /// </summary>
        public int GetSubscriberCount<TEvent>() where TEvent : FFmpegEvent
        {
            lock (_lockObject)
            {
                var eventType = typeof(TEvent);
                return _subscribers.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
            }
        }

        /// <summary>
        /// Clears all event subscriptions.
        /// Used during shutdown or testing to reset event system state.
        /// </summary>
        public void ClearSubscriptions()
        {
            lock (_lockObject)
            {
                var count = _subscribers.Values.Sum(h => h.Count);
                _subscribers.Clear();
                _logger.LogInformation("Cleared {Count} event subscriptions", count);
            }
        }
    }
}
