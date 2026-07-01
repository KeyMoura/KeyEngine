using KeyEngine.Events;

namespace KeyEngine.Parameters.Events;

/// <summary>
/// Raised after a runtime parameter is created or its value changes.
/// </summary>
public sealed class ParameterChangedEvent
    : IEvent
{
    /// <summary>
    /// Gets the parameter key.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the previous value, or <see langword="null"/> for a new parameter.
    /// </summary>
    public object? OldValue { get; init; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public object? NewValue { get; init; }
}
