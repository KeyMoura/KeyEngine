using KeyEngine.Events;

namespace KeyEngine.Annotations;

/// <summary>
/// Marks a method as an event listener.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class EventListenerAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the listener priority.
    /// </summary>
    public EventPriority Priority { get; init; }
        = EventPriority.Normal;

    /// <summary>
    /// Gets or sets whether cancelled events should be ignored.
    /// </summary>
    public bool IgnoreCancelled { get; init; }

    /// <summary>
    /// Gets or sets the execution order within the same priority.
    /// </summary>
    public int Order { get; init; }
}