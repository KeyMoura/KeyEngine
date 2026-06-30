using KeyEngine.Events;

namespace KeyEngine.Plugins.Events;

/// <summary>
/// Raised after a plugin has been discovered and placed in load order.
/// </summary>
public sealed class PluginLoadedEvent
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
