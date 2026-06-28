using System.Reflection;

namespace KeyEngine.Invocation;

/// <summary>
/// Creates invokers for reflected methods.
/// </summary>
public sealed class InvokerFactory
{
    /// <summary>
    /// Creates an invoker for the specified method.
    /// </summary>
    /// <param name="method">
    /// The reflected method.
    /// </param>
    /// <returns>
    /// The created invoker.
    /// </returns>
    public IMethodInvoker Create(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        return method.GetParameters().Length switch
        {
            0 => new ZeroParameterMethodInvoker(method),

            1 => new OneParameterMethodInvoker(method),

            _ => throw new NotSupportedException(
                $"Methods with {method.GetParameters().Length} parameters are not supported.")
        };
    }
}