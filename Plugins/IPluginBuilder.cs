using KeyEngine.Abstractions;

namespace KeyEngine.Plugins;

/// <summary>
/// Provides registration services to plugins.
/// </summary>
public interface IPluginBuilder
{
    /// <summary>
    /// Registers an engine system.
    /// </summary>
    /// <typeparam name="TSystem">
    /// The system type.
    /// </typeparam>
    void AddSystem<TSystem>()
        where TSystem : class, IEngineSystem;

    /// <summary>
    /// Registers a plugin configuration object.
    /// </summary>
    /// <typeparam name="TConfiguration">
    /// The configuration type.
    /// </typeparam>
    void AddConfiguration<TConfiguration>()
        where TConfiguration : class, new();

    /// <summary>
    /// Registers a singleton service.
    /// </summary>
    /// <typeparam name="TService">
    /// The service type.
    /// </typeparam>
    void AddSingleton<TService>()
        where TService : class;

    /// <summary>
    /// Registers an existing singleton service instance.
    /// </summary>
    /// <typeparam name="TService">
    /// The service type.
    /// </typeparam>
    /// <param name="instance">
    /// The singleton instance.
    /// </param>
    void AddSingleton<TService>(TService instance)
        where TService : class;

    /// <summary>
    /// Registers a singleton service.
    /// </summary>
    /// <typeparam name="TService">
    /// The service type.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type.
    /// </typeparam>
    void AddSingleton<TService, TImplementation>()
        where TImplementation : class, TService;

    /// <summary>
    /// Registers a transient service.
    /// </summary>
    /// <typeparam name="TService">
    /// The service type.
    /// </typeparam>
    void AddTransient<TService>()
        where TService : class;

    /// <summary>
    /// Registers a transient service.
    /// </summary>
    /// <typeparam name="TService">
    /// The service type.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type.
    /// </typeparam>
    void AddTransient<TService, TImplementation>()
        where TImplementation : class, TService;
}
