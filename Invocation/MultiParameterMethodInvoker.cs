using System.Reflection;

namespace KeyEngine.Invocation;

/// <summary>
/// Invokes an instance method with multiple parameters.
/// </summary>
public sealed class MultiParameterMethodInvoker : IMethodInvoker
{
    private readonly MethodInfo _method;
    private readonly int _parameterCount;

    public MultiParameterMethodInvoker(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        _method = method;
        _parameterCount = method.GetParameters().Length;
    }

    public object? Invoke(
        object instance,
        params object?[] arguments)
    {
        if (arguments.Length != _parameterCount)
        {
            throw new ArgumentException(
                $"Expected {_parameterCount} argument(s), received {arguments.Length}.",
                nameof(arguments));
        }

        return _method.Invoke(instance, arguments);
    }
}