namespace KeyEngine.Annotations;

/// <summary>
/// Marks a method to be invoked during each fixed engine update.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class OnFixedUpdateAttribute : Attribute
{
}