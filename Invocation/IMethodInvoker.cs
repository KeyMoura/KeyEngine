namespace KeyEngine.Invocation;

/// <summary>
/// Represents an object capable of invoking a reflected method.
/// </summary>
public interface IMethodInvoker
{
    /// <summary>
    /// Invokes the method.
    /// </summary>
    /// <param name="instance">
    /// The object instance that owns the method.
    /// </param>
    /// <param name="arguments">
    /// The arguments supplied to the method.
    /// </param>
    object? Invoke(
        object instance,
        params object?[] arguments);
}