using KeyEngine.Core;

namespace KeyEngine.Plugins;

/// <summary>
/// Represents a plugin manifest.
/// </summary>
public sealed class PluginManifest : ApplicationInfo
{
    /// <summary>
    /// Gets or sets the manifest version.
    /// </summary>
    public int ManifestVersion { get; init; } = 1;

    /// <summary>
    /// Gets or sets the unique plugin identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the plugin entry point.
    /// </summary>
    public required string Main { get; init; }

    /// <summary>
    /// Gets the IDs of plugins that must load before this plugin.
    /// </summary>
    public IList<string> Dependencies { get; init; }
        = [];
}
