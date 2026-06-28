namespace KeyEngine.Annotations;

/// <summary>
/// Marks a method to be invoked during engine shutdown.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class OnShutdownAttribute : Attribute
{
}