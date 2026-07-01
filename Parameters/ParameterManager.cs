using KeyEngine.Events;
using KeyEngine.Parameters.Events;

namespace KeyEngine.Parameters;

/// <summary>
/// Stores typed runtime parameters and publishes value changes.
/// </summary>
/// <remarks>
/// Dot-separated keys such as <c>server.port</c>, <c>email.smtp.host</c>, and
/// <c>auth.requireVerification</c> are recommended for organization. Key
/// matching is exact and ordinal. Instances are not guaranteed thread-safe.
/// </remarks>
public sealed class ParameterManager
{
    private readonly Dictionary<string, Parameter> _parameters =
        new(StringComparer.Ordinal);

    private readonly EventBus _events;

    /// <summary>
    /// Initializes a new parameter manager.
    /// </summary>
    /// <param name="events">
    /// The event bus that receives parameter changes.
    /// </param>
    public ParameterManager(EventBus events)
    {
        ArgumentNullException.ThrowIfNull(events);

        _events = events;
    }

    /// <summary>
    /// Creates or updates a typed parameter.
    /// </summary>
    /// <typeparam name="T">
    /// The declared value type.
    /// </typeparam>
    /// <param name="key">
    /// The non-blank parameter key. Dot-separated keys are recommended.
    /// </param>
    /// <param name="value">
    /// The parameter value.
    /// </param>
    /// <param name="description">
    /// An optional description.
    /// </param>
    /// <param name="category">
    /// An optional category.
    /// </param>
    /// <param name="isReadOnly">
    /// Whether later changes and removal are prohibited.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when an existing parameter is read-only.
    /// </exception>
    public void Set<T>(
        string key,
        T value,
        string? description = null,
        string? category = null,
        bool isReadOnly = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _parameters.TryGetValue(
            key,
            out Parameter? existing);

        if (existing?.IsReadOnly == true)
        {
            throw new InvalidOperationException(
                $"Parameter '{key}' is read-only and cannot be changed.");
        }

        Parameter parameter = new()
        {
            Key = key,
            Value = value,
            ValueType = typeof(T),
            Description = description,
            Category = category,
            IsReadOnly = isReadOnly
        };

        _parameters[key] = parameter;

        if (existing is not null &&
            Equals(
                existing.Value,
                value))
        {
            return;
        }

        _events.Publish(new ParameterChangedEvent
        {
            Key = key,
            OldValue = existing?.Value,
            NewValue = value
        });
    }

    /// <summary>
    /// Gets a typed parameter value.
    /// </summary>
    /// <typeparam name="T">
    /// The expected value type.
    /// </typeparam>
    /// <param name="key">
    /// The parameter key.
    /// </param>
    /// <returns>
    /// The typed value.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the key is not registered.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value cannot be read as <typeparamref name="T"/>.
    /// </exception>
    public T Get<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!_parameters.TryGetValue(
                key,
                out Parameter? parameter))
        {
            throw new KeyNotFoundException(
                $"Parameter '{key}' was not found.");
        }

        if (TryGetValue(
                parameter,
                out T value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Parameter '{key}' has type '{parameter.ValueType.FullName}' and cannot be read as '{typeof(T).FullName}'.");
    }

    /// <summary>
    /// Attempts to get a typed parameter value.
    /// </summary>
    /// <typeparam name="T">
    /// The expected value type.
    /// </typeparam>
    /// <param name="key">
    /// The parameter key.
    /// </param>
    /// <param name="value">
    /// The typed value when found and compatible.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the parameter exists and has a compatible
    /// value; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet<T>(
        string key,
        out T value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_parameters.TryGetValue(
                key,
                out Parameter? parameter) &&
            TryGetValue(
                parameter,
                out value))
        {
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Determines whether a parameter is registered.
    /// </summary>
    /// <param name="key">
    /// The parameter key.
    /// </param>
    public bool Contains(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return _parameters.ContainsKey(key);
    }

    /// <summary>
    /// Removes a parameter.
    /// </summary>
    /// <param name="key">
    /// The parameter key.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the parameter was removed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the parameter is read-only.
    /// </exception>
    public bool Remove(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!_parameters.TryGetValue(
                key,
                out Parameter? parameter))
        {
            return false;
        }

        if (parameter.IsReadOnly)
        {
            throw new InvalidOperationException(
                $"Parameter '{key}' is read-only and cannot be removed.");
        }

        return _parameters.Remove(key);
    }

    /// <summary>
    /// Gets snapshots of all registered parameters.
    /// </summary>
    public IReadOnlyList<Parameter> GetAll()
    {
        return _parameters.Values.ToArray();
    }

    private static bool TryGetValue<T>(
        Parameter parameter,
        out T value)
    {
        if (parameter.Value is T typed)
        {
            value = typed;
            return true;
        }

        if (parameter.Value is null &&
            default(T) is null)
        {
            value = default!;
            return true;
        }

        value = default!;
        return false;
    }
}
