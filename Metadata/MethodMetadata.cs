using KeyEngine.Invocation;
using System.Reflection;

namespace KeyEngine.Metadata;

/// <summary>
/// Represents a discovered engine method.
/// </summary>
internal sealed class MethodMetadata
{
    /// <summary>
    /// Gets the method kind.
    /// </summary>
    public required MethodKind Kind { get; init; }

    /// <summary>
    /// Gets the type that declares the method.
    /// </summary>
    public required Type DeclaringType { get; init; }

    /// <summary>
    /// Gets the reflected method.
    /// </summary>
    public required MethodInfo Method { get; init; }

    /// <summary>
    /// Gets the invoker used to execute the method.
    /// </summary>
    public required IMethodInvoker Invoker { get; init; }

    /// <summary>
    /// Gets the method parameter type, if one exists.
    /// </summary>
    public IReadOnlyList<Type> ParameterTypes { get; init; } = [];
}
