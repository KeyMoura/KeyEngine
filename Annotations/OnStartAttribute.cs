namespace KeyEngine.Annotations;

/// <summary>
/// Indicates that a method should be invoked once when the engine starts.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class OnStartAttribute : Attribute
{
}