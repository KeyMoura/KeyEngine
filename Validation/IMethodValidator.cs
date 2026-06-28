using System.Reflection;

namespace KeyEngine.Validation;

/// <summary>
/// Validates reflected engine methods.
/// </summary>
public interface IMethodValidator
{
    /// <summary>
    /// Validates a reflected method.
    /// </summary>
    /// <param name="method">
    /// The reflected method.
    /// </param>
    void Validate(MethodInfo method);
}