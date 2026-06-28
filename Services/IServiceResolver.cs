namespace KeyEngine.Services;

/// <summary>
/// Resolves registered services.
/// </summary>
public interface IServiceResolver
{
    /// <summary>
    /// Resolves a service.
    /// </summary>
    /// <typeparam name="TService">
    /// The service type.
    /// </typeparam>
    /// <returns>
    /// The resolved service.
    /// </returns>
    TService GetService<TService>()
        where TService : class;

    /// <summary>
    /// Resolves a service.
    /// </summary>
    /// <param name="serviceType">
    /// The requested service type.
    /// </param>
    /// <returns>
    /// The resolved service.
    /// </returns>
    object GetService(Type serviceType);
}