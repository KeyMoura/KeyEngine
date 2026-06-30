namespace KeyEngine.Resources;

/// <summary>
/// Loads resources of a specific type.
/// </summary>
/// <typeparam name="T">
/// The resource type.
/// </typeparam>
public interface IResourceLoader<T>
    where T : class
{
    /// <summary>
    /// Loads a resource from the specified location.
    /// </summary>
    /// <param name="location">
    /// The resource location.
    /// </param>
    /// <returns>
    /// The loaded resource.
    /// </returns>
    T Load(ResourceLocation location);
}
