namespace KeyEngine.Resources;

/// <summary>
/// Represents a loaded resource and its location.
/// </summary>
/// <typeparam name="T">
/// The resource type.
/// </typeparam>
public readonly struct ResourceHandle<T>
    where T : class
{
    /// <summary>
    /// Gets the location from which the resource was loaded.
    /// </summary>
    public ResourceLocation Location { get; }

    /// <summary>
    /// Gets the loaded resource.
    /// </summary>
    public T Resource { get; }

    internal ResourceHandle(
        ResourceLocation location,
        T resource)
    {
        Location = location;
        Resource = resource;
    }
}
