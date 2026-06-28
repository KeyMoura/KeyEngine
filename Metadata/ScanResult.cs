using KeyEngine.Commands;
using KeyEngine.Events.Models;

namespace KeyEngine.Metadata;

/// <summary>
/// Represents the metadata discovered while scanning an assembly.
/// </summary>
public sealed class ScanResult
{
    private readonly List<MethodMetadata> _methods = new();
    private readonly List<EventListenerMetadata> _eventListeners = new();
    private readonly List<Type> _systems = new();
    private readonly List<CommandMetadata> _commands = new();

    /// <summary>
    /// Gets all discovered methods.
    /// </summary>
    public IReadOnlyList<MethodMetadata> Methods => _methods;

    /// <summary>
    /// Gets all discovered event listeners.
    /// </summary>
    public IReadOnlyList<EventListenerMetadata> EventListeners => _eventListeners;

    /// <summary>
    /// Gets all discovered systems.
    /// </summary>
    public IReadOnlyList<Type> Systems => _systems;

    /// <summary>
    /// Gets all discovered commands.
    /// </summary>
    public IReadOnlyList<CommandMetadata> Commands => _commands;

    /// <summary>
    /// Adds a discovered method.
    /// </summary>
    /// <param name="method">
    /// The discovered method metadata.
    /// </param>
    public void Add(MethodMetadata method)
    {
        ArgumentNullException.ThrowIfNull(method);

        _methods.Add(method);
    }

    /// <summary>
    /// Adds a discovered event listener.
    /// </summary>
    /// <param name="listener">
    /// The discovered event listener metadata.
    /// </param>
    public void Add(EventListenerMetadata listener)
    {
        ArgumentNullException.ThrowIfNull(listener);

        _eventListeners.Add(listener);
    }

    /// <summary>
    /// Adds a discovered command.
    /// </summary>
    public void Add(CommandMetadata command)
    {
        ArgumentNullException.ThrowIfNull(command);

        _commands.Add(command);
    }

    /// <summary>
    /// Adds all discovered metadata from another scan result.
    /// </summary>
    /// <param name="result">
    /// The scan result whose metadata should be added.
    /// </param>
    public void AddRange(ScanResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        _methods.AddRange(result._methods);
        _eventListeners.AddRange(result._eventListeners);
        _systems.AddRange(result._systems);
        _commands.AddRange(result._commands);
    }

    public void AddSystem(Type systemType)
    {
        ArgumentNullException.ThrowIfNull(systemType);

        if (!_systems.Contains(systemType))
        {
            _systems.Add(systemType);
        }
    }

    /// <summary>
    /// Gets all methods of the specified kind.
    /// </summary>
    /// <param name="kind">
    /// The method kind.
    /// </param>
    /// <returns>
    /// A sequence of matching methods.
    /// </returns>
    public IEnumerable<MethodMetadata> GetMethods(MethodKind kind)
    {
        return _methods.Where(m => m.Kind == kind);
    }
}