using System.Reflection;

using KeyEngine.Scheduler;

namespace KeyEngine.Validation;

/// <summary>
/// Validates lifecycle methods.
/// </summary>
internal sealed class LifecycleMethodValidator : IMethodValidator
{
    /// <inheritdoc/>
    public void Validate(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        ParameterInfo[] parameters = method.GetParameters();

        if (method.ReturnType != typeof(void))
        {
            throw new EngineValidationException(
                $"Lifecycle method '{method.DeclaringType?.FullName}.{method.Name}' must return void.");
        }

        if (parameters.Length > 1)
        {
            throw new EngineValidationException(
                $"Lifecycle method '{method.DeclaringType?.FullName}.{method.Name}' may declare at most one parameter.");
        }

        if (parameters.Length == 1 &&
            parameters[0].ParameterType != typeof(UpdateContext))
        {
            throw new EngineValidationException(
                $"Parameter '{parameters[0].Name}' on '{method.DeclaringType?.FullName}.{method.Name}' must be an UpdateContext.");
        }
    }
}
