using KeyEngine.Commands.Events;
using KeyEngine.Commands.Parsing;
using KeyEngine.Events;
using KeyEngine.Metadata;
using KeyEngine.Systems;
using System.Security.Cryptography.X509Certificates;

namespace KeyEngine.Commands;

/// <summary>
/// Manages all registered engine commands.
/// </summary>
public sealed class CommandManager
{
    private readonly Dictionary<string, List<CommandMetadata>> _commands =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly SystemRegistry _systems;
    private readonly CommandResolver _resolver = new();
    private readonly CommandInvoker _invoker;
    private readonly EventBus _events;

    internal CommandManager(
        SystemRegistry systems,
        EventBus events)
    {
        ArgumentNullException.ThrowIfNull(systems);
        ArgumentNullException.ThrowIfNull(events);

        _systems = systems;
        _events = events;
        _invoker = new CommandInvoker(systems);
    }

    /// <summary>
    /// Registers a command.
    /// </summary>
    internal void Register(CommandMetadata command)
    {
        ArgumentNullException.ThrowIfNull(command);

        RegisterName(command.Name, command);

        foreach (string alias in command.Aliases)
        {
            RegisterName(alias, command);
        }
    }

    private void RegisterName(
        string name,
        CommandMetadata command)
    {
        if (!_commands.TryGetValue(name, out List<CommandMetadata>? overloads))
        {
            overloads = [];

            _commands.Add(name, overloads);
        }

        foreach (CommandMetadata existing in overloads)
        {
            if (HaveSameSignature(existing, command))
            {
                throw new InvalidOperationException(
                    $"Command '{name}' already contains an overload with the same signature.");
            }
        }

        overloads.Add(command);
    }

    /// <summary>
    /// Determines whether two commands have identical parameter signatures.
    /// </summary>
    /// <param name="left">
    /// The first command.
    /// </param>
    /// <param name="right">
    /// The second command.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the signatures are identical; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    private static bool HaveSameSignature(
        CommandMetadata left,
        CommandMetadata right)
    {
        IReadOnlyList<Type> leftParameters =
            left.Method.ParameterTypes;

        IReadOnlyList<Type> rightParameters =
            right.Method.ParameterTypes;

        if (leftParameters.Count != rightParameters.Count)
        {
            return false;
        }

        for (int i = 0; i < leftParameters.Count; i++)
        {
            if (leftParameters[i] != rightParameters[i])
            {
                return false;
            }
        }

        return true;
    }

    public bool Exists(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _commands.ContainsKey(name);
    }

    public bool TryGetCommands(
    string name,
    out IReadOnlyList<CommandMetadata>? commands)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (_commands.TryGetValue(name, out List<CommandMetadata>? overloads))
        {
            commands = overloads;
            return true;
        }

        commands = null;
        return false;
    }

    public IEnumerable<CommandMetadata> Commands =>
    _commands.Values
        .SelectMany(x => x)
        .Distinct();

    public object? Call(
    string name,
    params object?[] arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_commands.TryGetValue(name, out List<CommandMetadata>? overloads))
        {
            throw new InvalidOperationException(
                $"Command '{name}' was not found.");
        }

        CommandMetadata command =
            _resolver.Resolve(overloads, arguments);

        return _invoker.Invoke(
            command,
            arguments);
    }

    public T? Call<T>(
    string name,
    params object?[] arguments)
    {
        object? value = Call(name, arguments);

        if (value is null)
        {
            return default;
        }

        if (value is not T typed)
        {
            throw new InvalidOperationException(
                $"Command '{name}' returned '{value.GetType().Name}', expected '{typeof(T).Name}'.");
        }

        return typed;
    }

    /// <summary>
    /// Attempts to invoke a command.
    /// </summary>
    /// <param name="name">
    /// The command name.
    /// </param>
    /// <param name="result">
    /// The value returned by the command.
    /// </param>
    /// <param name="arguments">
    /// The command arguments.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the command was successfully invoked;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryCall(
        string name,
        out object? result,
        params object?[] arguments)
    {
        try
        {
            result = Call(name, arguments);

            return true;
        }
        catch
        {
            result = null;

            return false;
        }
    }

    /// <summary>
    /// Attempts to invoke a command and cast the result.
    /// </summary>
    /// <typeparam name="T">
    /// The expected return type.
    /// </typeparam>
    /// <param name="name">
    /// The command name.
    /// </param>
    /// <param name="result">
    /// The returned value.
    /// </param>
    /// <param name="arguments">
    /// The command arguments.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the command completed successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryCall<T>(
        string name,
        out T? result,
        params object?[] arguments)
    {
        if (!TryCall(name, out object? value, arguments))
        {
            result = default;

            return false;
        }

        if (value is T typed)
        {
            result = typed;

            return true;
        }

        result = default;

        return false;
    }

    /// <summary>
    /// Executes a parsed command.
    /// </summary>
    /// <param name="request">
    /// The command request.
    /// </param>
    public object? Execute(
    CommandRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        CommandInvokingEvent e = new()
        {
            Request = request
        };

        _events.Publish(e);

        if (e.IsCancelled)
        {
            return null;
        }

        object? result =
            Call(
                request.Name,
                request.Arguments.ToArray());

        _events.Publish(new CommandInvokedEvent
        {
            Request = request,
            Result = result
        });

        return result;
    }
}
