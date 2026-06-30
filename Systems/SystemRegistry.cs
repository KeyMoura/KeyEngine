using System.Collections.ObjectModel;

namespace KeyEngine.Systems;

using KeyEngine.Services;

/// <summary>
/// Stores and manages engine system instances.
/// </summary>
/// <remarks>
/// Each system type is instantiated at most once for the lifetime of the
/// engine. Subsequent requests return the existing instance.
/// </remarks>
internal sealed class SystemRegistry
{
    private IServiceResolver? _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemRegistry"/> class.
    /// </summary>
    /// <param name="services">
    /// The service resolver.
    /// </param>
    public SystemRegistry(
        IServiceResolver? services = null)
    {
        _services = services;
    }

    /// <summary>
    /// Sets the service resolver.
    /// </summary>
    /// <param name="services">
    /// The service resolver.
    /// </param>
    internal void SetServices(
    IServiceResolver services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _services = services;
    }

    /// <summary>
    /// Gets an existing system instance or creates one if it does not exist.
    /// </summary>
    /// <param name="systemType">
    /// The type of system to retrieve.
    /// </param>
    /// <returns>
    /// The system instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="systemType"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the requested system cannot be instantiated.
    /// </exception>
    public object GetOrCreate(Type systemType)
    {
        ArgumentNullException.ThrowIfNull(systemType);

        if (_services is null)
        {
            throw new InvalidOperationException(
                "The service provider has not been initialized.");
        }

        return _services.GetService(systemType);
    }
}
