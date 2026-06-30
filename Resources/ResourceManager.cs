namespace KeyEngine.Resources;

/// <summary>
/// Registers resource loaders and caches loaded resources.
/// </summary>
public sealed class ResourceManager
{
    private readonly Dictionary<Type, object> _loaders = new();

    private readonly Dictionary<(Type Type, ResourceLocation Location), object>
        _handles = new();

    /// <summary>
    /// Registers the loader for a resource type.
    /// </summary>
    /// <typeparam name="T">
    /// The resource type.
    /// </typeparam>
    /// <param name="loader">
    /// The resource loader.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a loader is already registered for the resource type.
    /// </exception>
    public void Register<T>(
        IResourceLoader<T> loader)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(loader);

        if (!_loaders.TryAdd(
                typeof(T),
                loader))
        {
            throw new InvalidOperationException(
                $"A resource loader is already registered for '{typeof(T).FullName}'.");
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
    /// A handle to the loaded resource.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no loader is registered for the resource type or the loader
    /// returns <see langword="null"/>.
    /// </exception>
    public ResourceHandle<T> Load<T>(
        ResourceLocation location)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(location.Scheme) ||
            string.IsNullOrWhiteSpace(location.Value))
        {
            throw new ArgumentException(
                "The resource location must specify a scheme and value.",
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
                typeof(T),
                out object? registeredLoader))
        {
            throw new InvalidOperationException(
                $"No resource loader is registered for '{typeof(T).FullName}'.");
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

        return handle;
    }
}
