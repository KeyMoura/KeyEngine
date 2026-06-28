namespace KeyEngine.Annotations;

/// <summary>
/// Indicates that a method should be invoked once every engine update.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class OnUpdateAttribute : Attribute
{
}