namespace KeyEngine.Diagnostics;

/// <summary>
/// Represents the current state of a plugin.
/// </summary>
public enum PluginState
{
    /// <summary>
    /// The plugin has been discovered but not loaded.
    /// </summary>
    Discovered,

    /// <summary>
    /// The plugin has been loaded.
    /// </summary>
    Loaded,

    /// <summary>
    /// The plugin is running.
    /// </summary>
    Running,

    /// <summary>
    /// The plugin has been stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// The plugin failed to load.
    /// </summary>
    Failed,

    /// <summary>
    /// The plugin is disabled.
    /// </summary>
    Disabled
}