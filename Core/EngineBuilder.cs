using KeyEngine.Plugins;
using KeyEngine.Scheduler;
using KeyEngine.Services;
using System.Reflection;

namespace KeyEngine.Core;

/// <summary>
/// Builds and configures a <see cref="Engine"/> instance.
/// </summary>
public sealed class EngineBuilder
{
    private readonly List<Assembly> _assemblies = new();
    private readonly SchedulerOptions _schedulerOptions = new();
    private readonly ApplicationInfo _engineInfo = new();
    private readonly List<Type> _plugins = new();

    private string _pluginDirectory = "plugins";

    /// <summary>
    /// Registers an assembly to be scanned by the engine.
    /// </summary>
    /// <param name="assembly">
    /// The assembly to register.
    /// </param>
    /// <returns>
    /// The current <see cref="EngineBuilder"/> instance.
    /// </returns>
    public EngineBuilder RegisterAssembly(Assembly assembly)
    {
        _assemblies.Add(assembly);

        return this;
    }

    public EngineBuilder SetPluginDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _pluginDirectory = path;

        return this;
    }

    /// <summary>
    /// Registers a plugin with the engine.
    /// </summary>
    /// <typeparam name="TPlugin">
    /// The plugin type.
    /// </typeparam>
    /// <returns>
    /// The current <see cref="EngineBuilder"/> instance.
    /// </returns>
    public EngineBuilder AddPlugin<TPlugin>()
        where TPlugin : class, IPlugin
    {
        _plugins.Add(typeof(TPlugin));

        return this;
    }

    /// <summary>
    /// Configures the engine scheduler.
    /// </summary>
    /// <param name="configure">
    /// The configuration delegate.
    /// </param>
    /// <returns>
    /// The current <see cref="EngineBuilder"/> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public EngineBuilder ConfigureScheduler(
        Action<SchedulerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(_schedulerOptions);

        return this;
    }

    /// <summary>
    /// Configures the engine metadata.
    /// </summary>
    /// <param name="configure">
    /// The configuration delegate.
    /// </param>
    /// <returns>
    /// The current <see cref="EngineBuilder"/> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public EngineBuilder ConfigureInfo(
        Action<ApplicationInfo> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(_engineInfo);

        return this;
    }

    /// <summary>
    /// Builds a configured <see cref="Engine"/>.
    /// </summary>
    /// <returns>
    /// A configured engine instance.
    /// </returns>
    public Engine Build()
    {
        EngineOptions options = new()
        {
            Assemblies = _assemblies,
            PluginDirectory = _pluginDirectory,
            Scheduler = _schedulerOptions,
            Info = _engineInfo,
        };

        return new Engine(options);
    }
}