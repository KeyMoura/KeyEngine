using KeyEngine.Metadata;
using KeyEngine.Systems;

namespace KeyEngine.Commands;

/// <summary>
/// Invokes engine commands.
/// </summary>
internal sealed class CommandInvoker
{
    private readonly SystemRegistry _systems;

    public CommandInvoker(SystemRegistry systems)
    {
        _systems = systems;
    }

    public object? Invoke(
        CommandMetadata command,
        params object?[] arguments)
    {
        object instance =
            _systems.GetOrCreate(
                command.Method.DeclaringType);

        return command.Method.Invoker.Invoke(
            instance,
            arguments);
    }
}
