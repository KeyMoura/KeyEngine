namespace KeyEngine.Events;

/// <summary>
/// Defines the execution order of event listeners.
/// </summary>
public enum EventPriority
{
    /// <summary>
    /// Executes before all other listeners.
    /// </summary>
    Lowest,

    /// <summary>
    /// Executes before normal listeners.
    /// </summary>
    Low,

    /// <summary>
    /// Executes with normal priority.
    /// </summary>
    Normal,

    /// <summary>
    /// Executes after normal listeners.
    /// </summary>
    High,

    /// <summary>
    /// Executes immediately before monitor listeners.
    /// </summary>
    Highest,

    /// <summary>
    /// Executes after every other listener.
    /// Intended for logging and diagnostics.
    /// </summary>
    Monitor
}