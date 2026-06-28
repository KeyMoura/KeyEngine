using System.Reflection;

namespace KeyEngine.Invocation;

/// <summary>
/// Invokes an instance method that declares a single parameter.
/// </summary>
public sealed class OneParameterMethodInvoker : IMethodInvoker
{
    private readonly MethodInfo _method;

    /// <summary>
    /// Initializes a new instance of the <see cref="OneParameterMethodInvoker"/> class.
    /// </summary>
    /// <param name="method">
    /// The reflected method.
    /// </param>
    public OneParameterMethodInvoker(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        ParameterInfo[] parameters = method.GetParameters();

        if (parameters.Length != 1)
        {
            throw new ArgumentException(
                "Method must declare exactly one parameter.",
                nameof(method));
        }

        _method = method;
    }

    /// <inheritdoc/>
    public object? Invoke(
    object instance,
    params object?[] arguments)
    {
        if (arguments.Length != 1)
        {
            throw new ArgumentException(
                "Exactly one argument is required.",
                nameof(arguments));
        }

        return _method.Invoke(instance, arguments);
    }
}