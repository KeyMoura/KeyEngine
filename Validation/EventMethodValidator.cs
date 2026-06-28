using KeyEngine.Events;
using System.Reflection;

namespace KeyEngine.Validation;

/// <summary>
/// Validates event listener methods.
/// </summary>
public sealed class EventMethodValidator : IMethodValidator
{
    /// <inheritdoc/>
    public void Validate(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        ParameterInfo[] parameters = method.GetParameters();

        if (method.ReturnType != typeof(void))
        {
            throw new EngineValidationException(
                $"Event listener '{method.DeclaringType?.FullName}.{method.Name}' must return void.");
        }

        if (parameters.Length != 1)
        {
            throw new EngineValidationException(
                $"Event listener '{method.DeclaringType?.FullName}.{method.Name}' must declare exactly one parameter.");
        }

        if (!typeof(IEvent).IsAssignableFrom(parameters[0].ParameterType))
        {
            throw new EngineValidationException(
                $"Parameter '{parameters[0].Name}' on '{method.DeclaringType?.FullName}.{method.Name}' must implement IEvent.");
        }
    }
}