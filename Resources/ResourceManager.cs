namespace KeyEngine.Resources;

/// <summary>
/// Registers resource loaders and caches loaded resources.
/// </summary>
/// <remarks>
/// Cached resources are shared and owned by the manager for its lifetime.
/// Disposable cached resources are disposed with the manager. Registered
/// loaders are borrowed and are never disposed by the manager. Instances are
/// not guaranteed to be thread-safe; loaders must not be registered
/// concurrently with resource loading.
/// </remarks>
public sealed class ResourceManager
    : IDisposable
{
    private readonly Dictionary<(Type Type, string Scheme), object> _loaders = new();

    private readonly Dictionary<(Type Type, ResourceLocation Location), object>
        _handles = new();

    private readonly HashSet<IDisposable> _disposableResources =
        new(ReferenceEqualityComparer.Instance);

    private bool _isDisposed;

    /// <summary>
    /// Registers the loader for a resource type and location scheme.
    /// </summary>
    /// <typeparam name="T">
    /// The resource type.
    /// </typeparam>
    /// <param name="scheme">
    /// The location scheme handled by the loader, such as <c>file</c>,
    /// <c>embedded</c>, or <c>memory</c>.
    /// </param>
    /// <param name="loader">
    /// The borrowed resource loader. The manager does not dispose the loader.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the scheme is blank or malformed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a loader is already registered for the resource type and
    /// scheme.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the resource manager has been disposed.
    /// </exception>
    public void Register<T>(
        string scheme,
        IResourceLoader<T> loader)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(loader);

        ThrowIfDisposed();

        string normalizedScheme =
            ResourceLocation.NormalizeScheme(scheme);

        if (!_loaders.TryAdd(
                (typeof(T), normalizedScheme),
                loader))
        {
            throw new InvalidOperationException(
                $"A resource loader is already registered for '{typeof(T).FullName}' and scheme '{normalizedScheme}'.");
        }
    }

    /// <summary>
    /// Loads and caches a resource from the specified location.
    /// </summary>
    /// <typeparam name="T">
    /// The resource type.
    /// </typeparam>
    /// <param name="location">
    /// The resource location.
    /// </param>
    /// <returns>
    /// A handle to the shared cached resource. The resource remains owned by
    /// the manager.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the resource location is invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no loader is registered for the exact resource type and
    /// location scheme, or the loader returns <see langword="null"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the resource manager has been disposed.
    /// </exception>
    public ResourceHandle<T> Load<T>(
        ResourceLocation location)
        where T : class
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(location.Scheme) ||
            string.IsNullOrWhiteSpace(location.Identifier))
        {
            throw new ArgumentException(
                "The resource location must specify a scheme and identifier.",
                nameof(location));
        }

        (Type Type, ResourceLocation Location) key =
            (typeof(T), location);

        if (_handles.TryGetValue(
                key,
                out object? cached))
        {
            return (ResourceHandle<T>)cached;
        }

        if (!_loaders.TryGetValue(
                (typeof(T), location.Scheme),
                out object? registeredLoader))
        {
            throw new InvalidOperationException(
                $"No resource loader is registered for '{typeof(T).FullName}' and scheme '{location.Scheme}'.");
        }

        IResourceLoader<T> loader =
            (IResourceLoader<T>)registeredLoader;

        T resource = loader.Load(location)
            ?? throw new InvalidOperationException(
                $"The resource loader for '{typeof(T).FullName}' returned null.");

        ResourceHandle<T> handle = new(
            location,
            resource);

        _handles.Add(
            key,
            handle);

        if (resource is IDisposable disposable)
        {
            _disposableResources.Add(disposable);
        }

        return handle;
    }

    /// <summary>
    /// Disposes cached resources owned by the manager. Registered loaders are
    /// not disposed.
    /// </summary>
    /// <exception cref="AggregateException">
    /// Thrown after cleanup when one or more cached resources fail to dispose.
    /// </exception>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        List<Exception>? exceptions = null;

        foreach (IDisposable resource in _disposableResources)
        {
            try
            {
                resource.Dispose();
            }
            catch (Exception exception)
            {
                exceptions ??= [];
                exceptions.Add(exception);
            }
        }

        _disposableResources.Clear();
        _handles.Clear();
        _loaders.Clear();

        if (exceptions is not null)
        {
            throw new AggregateException(
                "One or more cached resources failed to dispose.",
                exceptions);
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            _isDisposed,
            this);
    }
}
