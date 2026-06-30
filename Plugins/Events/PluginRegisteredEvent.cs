using KeyEngine.Events;

namespace KeyEngine.Plugins.Events;

/// <summary>
/// Raised after a plugin has completed service and system registration.
/// </summary>
public sealed class PluginRegisteredEvent
    : IEvent
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
}
