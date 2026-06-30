namespace KeyEngine.Services;

/// <summary>
/// Registers services with the engine.
/// </summary>
internal interface IServiceCollection
{
    /// <summary>
    /// Registers a singleton service.
    /// </summary>
    void AddSingleton<TService, TImplementation>()
        where TImplementation : class, TService;

    /// <summary>
    /// Registers a transient service.
    /// </summary>
    void AddTransient<TService, TImplementation>()
        where TImplementation : class, TService;

    /// <summary>
    /// Registers a singleton by concrete type.
    /// </summary>
    /// <param name="implementationType">
    /// The implementation type.
    /// </param>
    void AddSingleton(Type implementationType);

    /// <summary>
    /// Registers a transient by concrete type.
    /// </summary>
    /// <param name="implementationType">
    /// The implementation type.
    /// </param>
    void AddTransient(Type implementationType);

    /// <summary>
    /// Registers an existing singleton instance.
    /// </summary>
    /// <typeparam name="TService">
    /// The service type.
    /// </typeparam>
    /// <param name="instance">
    /// The service instance.
    /// </param>
    void AddSingleton<TService>(
        TService instance)
        where TService : class;

    /// <summary>
    /// Registers an existing singleton instance.
    /// </summary>
    /// <param name="serviceType">
    /// The service type.
    /// </param>
    /// <param name="instance">
    /// The service instance.
    /// </param>
    void AddSingleton(
        Type serviceType,
        object instance);

    /// <summary>
    /// Registers a singleton service.
    /// </summary>
    /// <typeparam name="TService">
    /// The service type.
    /// </typeparam>
    void AddSingleton<TService>()
        where TService : class;

    /// <summary>
    /// Registers a transient service.
    /// </summary>
    /// <typeparam name="TService">
    /// The service type.
    /// </typeparam>
    void AddTransient<TService>()
        where TService : class;
}
