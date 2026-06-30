using System.Reflection;

namespace KeyEngine.Plugins;

/// <summary>
/// Represents a loaded plugin.
/// </summary>
internal sealed class LoadedPlugin
{
    /// <summary>
    /// Gets the plugin assembly.
    /// </summary>
    public required Assembly Assembly { get; init; }

    /// <summary>
    /// Gets the plugin manifest.
    /// </summary>
    public required PluginManifest Manifest { get; init; }

    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    public required IPlugin Instance { get; init; }

    /// <summary>
    /// Gets the plugin context.
    /// </summary>
    public required PluginContext Context { get; init; }
}
