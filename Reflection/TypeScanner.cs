using KeyEngine.Abstractions;
using KeyEngine.Annotations;
using KeyEngine.Commands;
using KeyEngine.Events.Models;
using KeyEngine.Invocation;
using KeyEngine.Metadata;
using KeyEngine.Validation;
using System.Reflection;

namespace KeyEngine.Reflection;

/// <summary>
/// Scans assemblies for KeyEngine metadata.
/// </summary>
public sealed class TypeScanner
{
    private readonly InvokerFactory _invokerFactory = new();

    private readonly LifecycleMethodValidator _lifecycleValidator = new();
    private readonly EventMethodValidator _eventValidator = new();
    private readonly CommandMethodValidator _commandValidator = new();

    /// <summary>
    /// Scans the specified assembly for KeyEngine metadata.
    /// </summary>
    /// <param name="assembly">
    /// The assembly to scan.
    /// </param>
    /// <returns>
    /// The discovered metadata.
    /// </returns>
    public ScanResult Scan(Assembly assembly)
    {
        ScanResult result = new();

        foreach (Type type in assembly.GetTypes())
        {
            ScanType(type, result);
        }

        return result;
    }

    /// <summary>
    /// Scans a single type for KeyEngine metadata.
    /// </summary>
    /// <param name="type">
    /// The type to scan.
    /// </param>
    /// <returns>
    /// The discovered metadata.
    /// </returns>
    public ScanResult Scan(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        ScanResult result = new();

        ScanType(type, result);

        return result;
    }

    /// <summary>
    /// Scans a single type for KeyEngine metadata.
    /// </summary>
    private void ScanType(
        Type type,
        ScanResult result)
    {
        if (!typeof(IEngineSystem).IsAssignableFrom(type))
        {
            return;
        }

        result.AddSystem(type);

        foreach (MethodInfo method in type.GetMethods(
                     BindingFlags.Instance |
                     BindingFlags.Public |
                     BindingFlags.NonPublic))
        {
            AddMethod(result, type, method, typeof(OnStartAttribute), MethodKind.Startup);
            AddMethod(result, type, method, typeof(OnUpdateAttribute), MethodKind.Update);
            AddMethod(result, type, method, typeof(OnFixedUpdateAttribute), MethodKind.FixedUpdate);
            AddMethod(result, type, method, typeof(OnShutdownAttribute), MethodKind.Shutdown);

            AddEventListener(result, type, method);

            AddCommand(result, type, method);
        }
    }

    /// <summary>
    /// Adds a discovered engine method to the scan result.
    /// </summary>
    private void AddMethod(
    ScanResult result,
    Type declaringType,
    MethodInfo method,
    Type attributeType,
    MethodKind kind)
    {
        if (method.GetCustomAttribute(attributeType) is null)
        {
            return;
        }

        _lifecycleValidator.Validate(method);

        result.Add(
            CreateMethodMetadata(
                declaringType,
                method,
                kind));
    }

    private void AddCommand(
    ScanResult result,
    Type declaringType,
    MethodInfo method)
    {
        CommandAttribute? attribute =
            method.GetCustomAttribute<CommandAttribute>();

        if (attribute is null)
        {
            return;
        }

        _commandValidator.Validate(method);

        result.Add(new CommandMetadata
        {
            Name = attribute.Name,
            Aliases = attribute.Aliases,
            Description = attribute.Description,
            Usage = attribute.Usage,
            Category = attribute.Category,
            Method = CreateMethodMetadata(
                declaringType,
                method,
                MethodKind.Command)
        });
    }

    /// <summary>
    /// Scans a method for an event listener.
    /// </summary>
    private void AddEventListener(
        ScanResult result,
        Type declaringType,
        MethodInfo method)
    {
        EventListenerAttribute? attribute =
            method.GetCustomAttribute<EventListenerAttribute>();

        if (attribute is null)
        {
            return;
        }

        _eventValidator.Validate(method);

        ParameterInfo[] parameters = method.GetParameters();

        result.Add(new EventListenerMetadata
        {
            Method = CreateMethodMetadata(
                declaringType,
                method,
                MethodKind.EventListener),

            EventType = parameters[0].ParameterType,
            Priority = attribute.Priority,
            IgnoreCancelled = attribute.IgnoreCancelled,
            Order = attribute.Order
        });
    }

    /// <summary>
    /// Creates metadata for a reflected method.
    /// </summary>
    private MethodMetadata CreateMethodMetadata(
        Type declaringType,
        MethodInfo method,
        MethodKind kind)
    {
        return new MethodMetadata
        {
            Kind = kind,
            DeclaringType = declaringType,
            Method = method,
            ParameterTypes = method
                .GetParameters()
                .Select(parameter => parameter.ParameterType)
                .ToArray(),
            Invoker = _invokerFactory.Create(method)
        };
    }
}
