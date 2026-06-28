namespace KeyEngine.Plugins;

/// <summary>
/// Manages loaded plugins.
/// </summary>
public sealed class PluginManager
{
    private readonly PluginLoader _loader = new();

    private readonly List<LoadedPlugin> _plugins = new();

    private readonly Dictionary<LoadedPlugin, PluginBuilder> _builders = new();

    /// <summary>
    /// Gets the loaded plugins.
    /// </summary>
    public IReadOnlyList<LoadedPlugin> Plugins => _plugins;

    /// <summary>
    /// Loads plugins from a directory.
    /// </summary>
    /// <param name="directory">
    /// The plugins directory.
    /// </param>
    public void Load(
        string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        _plugins.Clear();

        foreach (LoadedPlugin plugin in _loader.LoadPlugins(directory))
        {
            PluginBuilder builder = new(plugin.Context);

            plugin.Instance.Configure(
                plugin.Context,
                builder);

            _builders.Add(
                plugin,
                builder);

            foreach (Type system in builder.Systems)
            {
                // TODO: Move this into LoadedPlugin later.
            }

            _plugins.Add(plugin);
        }
    }

    /// <summary>
    /// Gets the builder for a loaded plugin.
    /// </summary>
    /// <param name="plugin">
    /// The loaded plugin.
    /// </param>
    /// <returns>
    /// The plugin builder.
    /// </returns>
    internal PluginBuilder GetBuilder(
        LoadedPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        return _builders[plugin];
    }
}