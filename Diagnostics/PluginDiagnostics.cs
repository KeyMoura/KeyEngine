namespace KeyEngine.Diagnostics;

/// <summary>
/// Represents diagnostic information about a plugin.
/// </summary>
public sealed class PluginDiagnostics
{
    /// <summary>
    /// Gets the plugin identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the plugin name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the plugin version.
    /// </summary>
    public required Version Version { get; init; }

    /// <summary>
    /// Gets the plugin state.
    /// </summary>
    public PluginState State { get; init; }

    /// <summary>
    /// Gets the number of registered systems.
    /// </summary>
    public int SystemCount { get; init; }

    /// <summary>
    /// Gets the number of registered commands.
    /// </summary>
    public int CommandCount { get; init; }

    /// <summary>
    /// Gets the number of registered event listeners.
    /// </summary>
    public int EventListenerCount { get; init; }

    /// <summary>
    /// Gets the number of registered services.
    /// </summary>
    public int ServiceCount { get; init; }
}
