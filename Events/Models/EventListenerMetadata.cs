using KeyEngine.Metadata;

namespace KeyEngine.Events.Models;

/// <summary>
/// Represents a discovered event listener.
/// </summary>
public sealed class EventListenerMetadata
{
    /// <summary>
    /// Gets the underlying method metadata.
    /// </summary>
    public required MethodMetadata Method { get; init; }

    /// <summary>
    /// Gets the handled event type.
    /// </summary>
    public required Type EventType { get; init; }

    /// <summary>
    /// Gets the listener priority.
    /// </summary>
    public required EventPriority Priority { get; init; }

    /// <summary>
    /// Gets whether cancelled events should be ignored.
    /// </summary>
    public required bool IgnoreCancelled { get; init; }

    /// <summary>
    /// Gets the execution order.
    /// </summary>
    public required int Order { get; init; }
}