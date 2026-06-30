namespace KeyEngine.Metadata;

/// <summary>
/// Identifies the purpose of a discovered engine method.
/// </summary>
internal enum MethodKind
{
    /// <summary>
    /// Executes once during engine initialization.
    /// </summary>
    Startup,

    /// <summary>
    /// Executes once every engine frame.
    /// </summary>
    Update,

    /// <summary>
    /// Executes at a fixed interval.
    /// </summary>
    FixedUpdate,

    /// <summary>
    /// Executes during engine shutdown.
    /// </summary>
    Shutdown,

    /// <summary>
    /// Executes when an event is published.
    /// </summary>
    EventListener,

    /// <summary>
    /// Executes when a command is invoked.
    /// </summary>
    Command
}
