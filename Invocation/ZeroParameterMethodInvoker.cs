using System.Reflection;

namespace KeyEngine.Invocation;

/// <summary>
/// Invokes an instance method that declares no parameters.
/// </summary>
internal sealed class ZeroParameterMethodInvoker : IMethodInvoker
{

    private readonly MethodInfo _method;
    /// <summary>
    /// Initializes a new instance of the <see cref="ZeroParameterMethodInvoker"/> class.
    /// </summary>
    /// <param name="method">
    /// The method to invoke.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="method"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the method is not supported.
    /// </exception>
    public ZeroParameterMethodInvoker(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (method.IsStatic)
        {
            throw new ArgumentException(
                "Static methods are not supported.",
                nameof(method));
        }

        if (method.GetParameters().Length != 0)
        {
            throw new ArgumentException(
                "Method must not declare any parameters.",
                nameof(method));
        }

        _method = method;
    }

    /// <inheritdoc/>
    public object? Invoke(
    object instance,
    params object?[] arguments)
    {
        return _method.Invoke(instance, null);
    }
}
