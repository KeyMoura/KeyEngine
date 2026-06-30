using System.Reflection;

namespace KeyEngine.Plugins;

/// <summary>
/// Loads plugins from disk.
/// </summary>
internal sealed class PluginLoader
{

    private readonly PluginManifestLoader _manifestLoader = new();

    private readonly PluginContextFactory _contextFactory = new();

    /// <summary>
    /// Loads all plugins from a directory.
    /// </summary>
    /// <param name="directory">
    /// The plugins directory.
    /// </param>
    /// <returns>
    /// The loaded plugins.
    /// </returns>
    public IEnumerable<LoadedPlugin> LoadPlugins(
        string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        if (!Directory.Exists(directory))
        {
            yield break;
        }

        foreach (string dll in Directory.EnumerateFiles(
                     directory,
                     "*.dll"))
        {
            Assembly assembly =
                Assembly.LoadFrom(dll);

            if (!_manifestLoader.TryLoad(
                    assembly,
                    out PluginManifest? manifest))
            {
                continue;
            }

            Type pluginType =
                GetPluginType(
                    assembly,
                    manifest);

            IPlugin plugin =
                CreatePlugin(pluginType);

            PluginContext context =
                _contextFactory.Create(
                    directory,
                    manifest);

            yield return new LoadedPlugin
            {
                Assembly = assembly,
                Manifest = manifest,
                Instance = plugin,
                Context = context
            };
        }
    }

    /// <summary>
    /// Gets the plugin entry type.
    /// </summary>
    /// <param name="assembly">
    /// The plugin assembly.
    /// </param>
    /// <param name="manifest">
    /// The plugin manifest.
    /// </param>
    /// <returns>
    /// The plugin type.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the entry point cannot be found.
    /// </exception>
    private Type GetPluginType(
        Assembly assembly,
        PluginManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(manifest);

        return assembly.GetType(manifest.Main)
            ?? throw new InvalidOperationException(
                $"Plugin entry point '{manifest.Main}' was not found.");
    }

    /// <summary>
    /// Creates an instance of a plugin.
    /// </summary>
    /// <param name="pluginType">
    /// The plugin type.
    /// </param>
    /// <returns>
    /// The created plugin.
    /// </returns>
    private IPlugin CreatePlugin(Type pluginType)
    {
        ArgumentNullException.ThrowIfNull(pluginType);

        return (IPlugin)Activator.CreateInstance(pluginType)!;
    }
}
