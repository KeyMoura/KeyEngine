namespace KeyEngine.Plugins;

/// <summary>
/// Manages loaded plugins.
/// </summary>
internal sealed class PluginManager
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
        _builders.Clear();

        LoadedPlugin[] discoveredPlugins =
            _loader.LoadPlugins(directory).ToArray();

        foreach (LoadedPlugin plugin in OrderByDependencies(discoveredPlugins))
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

    internal static IReadOnlyList<LoadedPlugin> OrderByDependencies(
        IReadOnlyList<LoadedPlugin> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        Dictionary<string, LoadedPlugin> pluginsById =
            new(StringComparer.Ordinal);

        foreach (LoadedPlugin plugin in plugins)
        {
            if (!pluginsById.TryAdd(
                    plugin.Manifest.Id,
                    plugin))
            {
                throw new global::KeyEngine.Validation.EngineValidationException(
                    $"Duplicate plugin ID '{plugin.Manifest.Id}' was discovered.");
            }
        }

        List<LoadedPlugin> ordered = [];
        Dictionary<LoadedPlugin, VisitState> states = [];
        List<LoadedPlugin> path = [];

        foreach (LoadedPlugin plugin in plugins)
        {
            Visit(
                plugin,
                pluginsById,
                states,
                path,
                ordered);
        }

        return ordered;
    }

    private static void Visit(
        LoadedPlugin plugin,
        IReadOnlyDictionary<string, LoadedPlugin> pluginsById,
        Dictionary<LoadedPlugin, VisitState> states,
        List<LoadedPlugin> path,
        List<LoadedPlugin> ordered)
    {
        if (states.TryGetValue(
                plugin,
                out VisitState state))
        {
            if (state == VisitState.Visited)
            {
                return;
            }

            int cycleStart = path.FindIndex(candidate =>
                ReferenceEquals(candidate, plugin));

            IEnumerable<string> cycle = path
                .Skip(cycleStart)
                .Select(candidate => candidate.Manifest.Id)
                .Append(plugin.Manifest.Id);

            throw new global::KeyEngine.Validation.EngineValidationException(
                $"Plugin dependency cycle detected: {string.Join(" -> ", cycle)}.");
        }

        states.Add(
            plugin,
            VisitState.Visiting);
        path.Add(plugin);

        foreach (string dependencyId in plugin.Manifest.Dependencies)
        {
            if (!pluginsById.TryGetValue(
                    dependencyId,
                    out LoadedPlugin? dependency))
            {
                throw new global::KeyEngine.Validation.EngineValidationException(
                    $"Plugin '{plugin.Manifest.Id}' depends on missing plugin '{dependencyId}'.");
            }

            Visit(
                dependency,
                pluginsById,
                states,
                path,
                ordered);
        }

        path.RemoveAt(path.Count - 1);
        states[plugin] = VisitState.Visited;
        ordered.Add(plugin);
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

    private enum VisitState
    {
        Visiting,
        Visited
    }
}
