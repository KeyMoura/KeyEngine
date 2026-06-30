namespace KeyEngine.Plugins;

/// <summary>
/// Manages loaded plugins.
/// </summary>
internal sealed class PluginManager
{
    private readonly PluginLoader _loader = new();

    private readonly global::KeyEngine.Events.EventBus _events;

    private readonly List<LoadedPlugin> _plugins = new();

    private readonly Dictionary<LoadedPlugin, PluginBuilder> _builders = new();

    /// <summary>
    /// Gets the loaded plugins.
    /// </summary>
    public IReadOnlyList<LoadedPlugin> Plugins => _plugins;

    internal PluginManager(global::KeyEngine.Events.EventBus events)
    {
        ArgumentNullException.ThrowIfNull(events);

        _events = events;
    }

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

        LoadedPlugin[] discoveredPlugins =
            _loader.LoadPlugins(directory).ToArray();

        LoadPlugins(discoveredPlugins);
    }

    internal void LoadPlugins(IReadOnlyList<LoadedPlugin> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        _plugins.Clear();
        _builders.Clear();

        foreach (LoadedPlugin plugin in OrderPlugins(plugins))
        {
            _events.Publish(new Events.PluginLoadedEvent
            {
                Id = plugin.Manifest.Id,
                Name = plugin.Manifest.Name,
                Version = plugin.Manifest.Version
            });

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

            _events.Publish(new Events.PluginRegisteredEvent
            {
                Id = plugin.Manifest.Id,
                Name = plugin.Manifest.Name,
                Version = plugin.Manifest.Version
            });
        }
    }

    internal static IReadOnlyList<LoadedPlugin> OrderPlugins(
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

        Dictionary<LoadedPlugin, List<LoadedPlugin>> prerequisites = [];

        foreach (LoadedPlugin plugin in plugins)
        {
            prerequisites.Add(
                plugin,
                []);
        }

        foreach (LoadedPlugin plugin in plugins)
        {
            foreach (string dependencyId in plugin.Manifest.Dependencies)
            {
                if (!pluginsById.TryGetValue(
                        dependencyId,
                        out LoadedPlugin? dependency))
                {
                    throw new global::KeyEngine.Validation.EngineValidationException(
                        $"Plugin '{plugin.Manifest.Id}' depends on missing plugin '{dependencyId}'.");
                }

                AddPrerequisite(
                    prerequisites,
                    plugin,
                    dependency);
            }

            foreach (string targetId in plugin.Manifest.LoadAfter)
            {
                if (pluginsById.TryGetValue(
                        targetId,
                        out LoadedPlugin? target))
                {
                    AddPrerequisite(
                        prerequisites,
                        plugin,
                        target);
                }
            }

            foreach (string targetId in plugin.Manifest.LoadBefore)
            {
                if (pluginsById.TryGetValue(
                        targetId,
                        out LoadedPlugin? target))
                {
                    AddPrerequisite(
                        prerequisites,
                        target,
                        plugin);
                }
            }
        }

        List<LoadedPlugin> ordered = [];
        Dictionary<LoadedPlugin, VisitState> states = [];
        List<LoadedPlugin> path = [];

        foreach (LoadedPlugin plugin in plugins)
        {
            Visit(
                plugin,
                prerequisites,
                states,
                path,
                ordered);
        }

        return ordered;
    }

    private static void AddPrerequisite(
        Dictionary<LoadedPlugin, List<LoadedPlugin>> prerequisites,
        LoadedPlugin plugin,
        LoadedPlugin prerequisite)
    {
        List<LoadedPlugin> pluginPrerequisites = prerequisites[plugin];

        if (!pluginPrerequisites.Contains(prerequisite))
        {
            pluginPrerequisites.Add(prerequisite);
        }
    }

    private static void Visit(
        LoadedPlugin plugin,
        IReadOnlyDictionary<LoadedPlugin, List<LoadedPlugin>> prerequisites,
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
                $"Plugin load-order cycle detected: {string.Join(" -> ", cycle)}.");
        }

        states.Add(
            plugin,
            VisitState.Visiting);
        path.Add(plugin);

        foreach (LoadedPlugin prerequisite in prerequisites[plugin])
        {
            Visit(
                prerequisite,
                prerequisites,
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
