namespace KeyEngine.Events;

/// <summary>
/// Represents an event that can be cancelled.
/// </summary>
public abstract class CancellableEvent
    : IEvent
{
    /// <summary>
    /// Gets or sets a value indicating whether the event has been cancelled.
    /// </summary>
    public bool IsCancelled
    {
        get;
        set;
    }
}