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
    /// <exception cref="InvalidOperationException">
    /// Thrown when the method does not have a supported command signature.
    /// </exception>
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

        ParameterInfo[] parameters = method.GetParameters();

        if (parameters.Length > 1)
        {
            throw new InvalidOperationException(
                $"Command '{method.Name}' may declare at most one parameter.");
        }

        if (parameters.Length == 1 &&
            parameters[0].ParameterType != typeof(string))
        {
            throw new InvalidOperationException(
                $"Parameter '{parameters[0].Name}' on command '{method.Name}' must be a string.");
        }
    }
}
