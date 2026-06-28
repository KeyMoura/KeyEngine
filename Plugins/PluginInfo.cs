using KeyEngine.Core;

namespace KeyEngine.Plugins;

/// <summary>
/// Represents metadata about a plugin.
/// </summary>
public sealed class PluginInfo : ApplicationInfo
{
    /// <summary>
    /// Gets or sets the plugin identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;
}