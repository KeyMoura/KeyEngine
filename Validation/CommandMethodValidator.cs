using System.Reflection;

namespace KeyEngine.Validation;

/// <summary>
/// Validates command methods.
/// </summary>
public sealed class CommandMethodValidator
{
    /// <summary>
    /// Validates a command method.
    /// </summary>
    /// <param name="method">
    /// The reflected method.
    /// </param>
    public void Validate(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (method.IsStatic)
        {
            throw new InvalidOperationException(
                $"Command '{method.Name}' cannot be static.");
        }

        if (method.IsGenericMethod)
        {
            throw new InvalidOperationException(
                $"Command '{method.Name}' cannot be generic.");
        }
    }
}