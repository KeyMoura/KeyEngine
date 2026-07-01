using KeyEngine.Abstractions;
using KeyEngine.Metadata;
using KeyEngine.Services;

namespace KeyEngine.Plugins;

/// <summary>
/// Default implementation of <see cref="IPluginBuilder"/>.
/// </summary>
internal sealed class PluginBuilder : IPluginBuilder
{
    private readonly List<Type> _systems = new();
    private readonly PluginInfo _info = new();
    private readonly ServiceCollection _services = new();
    private readonly ScanResult _scanResult = new();
    private readonly PluginContext _context;

    /// <summary>
    /// Gets the registered systems.
    /// </summary>
    internal IReadOnlyList<Type> Systems => _systems;

    internal PluginInfo Info => _info;

    internal ServiceCollection Services => _services;

    internal ScanResult ScanResult => _scanResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginBuilder"/> class.
    /// </summary>
    /// <param name="context">
    /// The plugin context.
    /// </param>
    public PluginBuilder(
        PluginContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    /// <inheritdoc/>
    public void AddSystem<TSystem>()
        where TSystem : class, IEngineSystem
    {
        _systems.Add(typeof(TSystem));

        _services.AddSingleton<TSystem>();
    }

    /// <summary>
    /// Registers a plugin configuration object.
    /// </summary>
    /// <typeparam name="TConfiguration">
    /// The configuration type.
    /// </typeparam>
    public void AddConfiguration<TConfiguration>()
        where TConfiguration : class, new()
    {
        TConfiguration configuration =
            _context.Configuration.Get<TConfiguration>();

        _services.AddSingleton(configuration);
    }

    /// <inheritdoc/>
    public void AddSingleton<TService>()
        where TService : class
    {
        _services.AddSingleton<TService>();
    }

    /// <inheritdoc/>
    public void AddSingleton<TService>(TService instance)
        where TService : class
    {
        _services.AddSingleton(instance);
    }

    /// <inheritdoc/>
    public void AddSingleton<TService, TImplementation>()
        where TImplementation : class, TService
    {
        _services.AddSingleton<TService, TImplementation>();
    }

    /// <inheritdoc/>
    public void AddTransient<TService>()
        where TService : class
    {
        _services.AddTransient<TService>();
    }

    /// <inheritdoc/>
    public void AddTransient<TService, TImplementation>()
        where TImplementation : class, TService
    {
        _services.AddTransient<TService, TImplementation>();
    }

    /// <summary>
    /// Configures the plugin metadata.
    /// </summary>
    /// <param name="configure">
    /// The configuration delegate.
    /// </param>
    public void ConfigureInfo(
        Action<PluginInfo> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(_info);
    }
}
