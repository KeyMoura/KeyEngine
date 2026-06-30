using KeyEngine.Events.Models;
using KeyEngine.Systems;

namespace KeyEngine.Events;

/// <summary>
/// Publishes events to registered listeners.
/// </summary>
public sealed class EventBus
{
    private readonly SystemRegistry _systemRegistry;

    internal EventBus(SystemRegistry systemRegistry)
    {
        ArgumentNullException.ThrowIfNull(systemRegistry);

        _systemRegistry = systemRegistry;
    }

    private readonly Dictionary<Type, List<EventListenerMetadata>> _listeners = new();

    /// <summary>
    /// Registers an event listener.
    /// </summary>
    /// <param name="listener">
    /// The listener metadata.
    /// </param>
    internal void Register(EventListenerMetadata listener)
    {
        ArgumentNullException.ThrowIfNull(listener);

        if (!_listeners.TryGetValue(listener.EventType, out List<EventListenerMetadata>? listeners))
        {
            listeners = new List<EventListenerMetadata>();
            _listeners.Add(listener.EventType, listeners);
        }

        listeners.Add(listener);

        // Sort once during registration.
        listeners.Sort(static (a, b) =>
        {
            int priority = a.Priority.CompareTo(b.Priority);

            if (priority != 0)
            {
                return priority;
            }

            return a.Order.CompareTo(b.Order);
        });
    }

    /// <summary>
    /// Gets all listeners registered for an event type.
    /// </summary>
    /// <param name="eventType">
    /// The event type.
    /// </param>
    /// <returns>
    /// A read-only collection of listeners.
    /// </returns>
    internal IReadOnlyList<EventListenerMetadata> GetListeners(Type eventType)
    {
        if (_listeners.TryGetValue(eventType, out List<EventListenerMetadata>? listeners))
        {
            return listeners;
        }

        return Array.Empty<EventListenerMetadata>();
    }

    /// <summary>
    /// Publishes an event.
    /// </summary>
    /// <typeparam name="TEvent">
    /// The event type.
    /// </typeparam>
    /// <param name="event">
    /// The event instance.
    /// </param>
    /// <returns>
    /// The same event instance.
    /// </returns>
    public TEvent Publish<TEvent>(TEvent @event)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        foreach (EventListenerMetadata listener in GetListeners(typeof(TEvent)))
        {
            if (@event is CancellableEvent { IsCancelled: true } &&
                listener.IgnoreCancelled)
            {
                continue;
            }

            // Temporary instance creation.
            object instance =
                _systemRegistry.GetOrCreate(listener.Method.DeclaringType);

            listener.Method.Invoker.Invoke(instance, @event);
        }

        return @event;
    }
}
