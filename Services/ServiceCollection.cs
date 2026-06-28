namespace KeyEngine.Services;

/// <summary>
/// Default implementation of <see cref="IServiceCollection"/>.
/// </summary>
internal sealed class ServiceCollection : IServiceCollection
{
    private readonly List<ServiceDescriptor> _services = new();

    /// <summary>
    /// Adds an existing service descriptor.
    /// </summary>
    /// <param name="descriptor">
    /// The service descriptor.
    /// </param>
    internal void Add(
    ServiceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        _services.Add(descriptor);
    }

    /// <summary>
    /// Gets the registered services.
    /// </summary>
    internal IReadOnlyList<ServiceDescriptor> Services => _services;

    /// <inheritdoc/>
    public void AddSingleton<TService, TImplementation>()
        where TImplementation : class, TService
    {
        _services.Add(new ServiceDescriptor
        {
            ServiceType = typeof(TService),
            ImplementationType = typeof(TImplementation),
            Lifetime = ServiceLifetime.Singleton
        });
    }

    /// <inheritdoc/>
    public void AddTransient<TService, TImplementation>()
        where TImplementation : class, TService
    {
        _services.Add(new ServiceDescriptor
        {
            ServiceType = typeof(TService),
            ImplementationType = typeof(TImplementation),
            Lifetime = ServiceLifetime.Transient
        });
    }

    /// <inheritdoc/>
    public void AddSingleton(Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(implementationType);

        _services.Add(new ServiceDescriptor
        {
            ServiceType = implementationType,
            ImplementationType = implementationType,
            Lifetime = ServiceLifetime.Singleton
        });
    }

    /// <inheritdoc/>
    public void AddTransient(Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(implementationType);

        _services.Add(new ServiceDescriptor
        {
            ServiceType = implementationType,
            ImplementationType = implementationType,
            Lifetime = ServiceLifetime.Transient
        });
    }

    /// <summary>
    /// Registers an existing singleton instance.
    /// </summary>
    /// <param name="instance">
    /// The service instance.
    /// </param>
    /// <typeparam name="TService">
    /// The service type.
    /// </typeparam>
    public void AddSingleton<TService>(
        TService instance)
        where TService : class
    {
        {
            ArgumentNullException.ThrowIfNull(instance);

            AddSingleton(
                typeof(TService),
                instance);
        }
    }

    /// <inheritdoc/>
    public void AddSingleton(
        Type serviceType,
        object instance)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(instance);

        _services.Add(new ServiceDescriptor
        {
            ServiceType = serviceType,
            ImplementationType = instance.GetType(),
            Lifetime = ServiceLifetime.Singleton,
            Instance = instance
        });
    }

    /// <inheritdoc/>
    public void AddSingleton<TService>()
        where TService : class
    {
        AddSingleton<TService, TService>();
    }

    /// <inheritdoc/>
    public void AddTransient<TService>()
        where TService : class
    {
        AddTransient<TService, TService>();
    }

    /// <summary>
    /// Registers multiple singleton services.
    /// </summary>
    /// <param name="implementationTypes">
    /// The implementation types.
    /// </param>
    internal void AddSingletonRange(IEnumerable<Type> implementationTypes)
    {
        ArgumentNullException.ThrowIfNull(implementationTypes);

        foreach (Type type in implementationTypes)
        {
            AddSingleton(type);
        }
    }

    /// <summary>
    /// Builds a service resolver from the registered services.
    /// </summary>
    internal IServiceResolver Build()
    {
        return new ServiceProvider(_services);
    }
}