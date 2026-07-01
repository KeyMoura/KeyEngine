using System.Globalization;
using KeyEngine.Events;
using KeyEngine.IO;
using KeyEngine.Parameters.Events;
using KeyEngine.Serialization;

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

    private readonly IFileSystem _fileSystem;

    private readonly ISerializer _serializer;

    /// <summary>
    /// Initializes a new parameter manager.
    /// </summary>
    /// <param name="events">
    /// The event bus that receives parameter changes.
    /// </param>
    public ParameterManager(EventBus events)
        : this(
            events,
            new PhysicalFileSystem(),
            new JsonSerializerAdapter())
    {
    }

    /// <summary>
    /// Initializes a new parameter manager.
    /// </summary>
    /// <param name="events">
    /// The event bus that receives parameter changes.
    /// </param>
    /// <param name="fileSystem">
    /// The filesystem used to save and load parameter files.
    /// </param>
    /// <param name="serializer">
    /// The serializer used to save and load parameter files.
    /// </param>
    public ParameterManager(
        EventBus events,
        IFileSystem fileSystem,
        ISerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(serializer);

        _events = events;
        _fileSystem = fileSystem;
        _serializer = serializer;
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

    /// <summary>
    /// Saves all registered parameters to a file.
    /// </summary>
    /// <remarks>
    /// Parameter persistence is file-based and supports <see cref="string"/>,
    /// <see cref="int"/>, <see cref="double"/>, and <see cref="bool"/> values.
    /// Unsupported value types fail clearly instead of being skipped.
    /// </remarks>
    /// <param name="path">
    /// The destination file path.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="path"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when a parameter value type cannot be persisted.
    /// </exception>
    public void Save(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        PersistedParameterCollection collection = new()
        {
            Parameters = _parameters.Values
                .Select(CreatePersistedParameter)
                .ToArray()
        };

        string text =
            _serializer.Serialize(collection);

        _fileSystem.WriteAllText(
            path,
            text);
    }

    /// <summary>
    /// Loads parameters from a file.
    /// </summary>
    /// <remarks>
    /// Loading replaces the current parameter set with the file contents. It
    /// restores metadata and read-only flags, validates all entries before
    /// applying them, and does not publish <see cref="ParameterChangedEvent"/>
    /// for loaded values.
    /// </remarks>
    /// <param name="path">
    /// The source file path.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="path"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the persisted data is missing or invalid.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when a persisted value type is not supported.
    /// </exception>
    public void Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string text =
            _fileSystem.ReadAllText(path);

        PersistedParameterCollection collection =
            _serializer.Deserialize<PersistedParameterCollection>(text)
            ?? throw new InvalidOperationException(
                "Parameter file did not contain a parameter collection.");

        Dictionary<string, Parameter> loaded =
            new(StringComparer.Ordinal);

        foreach (PersistedParameter persisted in collection.Parameters)
        {
            Parameter parameter =
                CreateParameter(persisted);

            if (!loaded.TryAdd(
                    parameter.Key,
                    parameter))
            {
                throw new InvalidOperationException(
                    $"Parameter file contains duplicate key '{parameter.Key}'.");
            }
        }

        _parameters.Clear();

        foreach ((string key, Parameter parameter) in loaded)
        {
            _parameters.Add(
                key,
                parameter);
        }
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

    private static PersistedParameter CreatePersistedParameter(
        Parameter parameter)
    {
        string valueType =
            GetPersistedValueType(parameter.ValueType);

        return new PersistedParameter
        {
            Key = parameter.Key,
            Value = FormatValue(
                parameter.Value,
                parameter.ValueType),
            ValueType = valueType,
            Description = parameter.Description,
            Category = parameter.Category,
            IsReadOnly = parameter.IsReadOnly
        };
    }

    private static Parameter CreateParameter(
        PersistedParameter persisted)
    {
        ArgumentNullException.ThrowIfNull(persisted);
        ArgumentException.ThrowIfNullOrWhiteSpace(persisted.Key);
        ArgumentException.ThrowIfNullOrWhiteSpace(persisted.ValueType);

        Type valueType =
            GetRuntimeValueType(persisted.ValueType);

        return new Parameter
        {
            Key = persisted.Key,
            Value = ParseValue(
                persisted.Value,
                valueType,
                persisted.Key),
            ValueType = valueType,
            Description = persisted.Description,
            Category = persisted.Category,
            IsReadOnly = persisted.IsReadOnly
        };
    }

    private static string GetPersistedValueType(Type valueType)
    {
        if (valueType == typeof(string))
        {
            return "string";
        }

        if (valueType == typeof(int))
        {
            return "int";
        }

        if (valueType == typeof(double))
        {
            return "double";
        }

        if (valueType == typeof(bool))
        {
            return "bool";
        }

        throw new NotSupportedException(
            $"Parameter value type '{valueType.FullName}' is not supported for persistence.");
    }

    private static Type GetRuntimeValueType(string valueType)
    {
        return valueType switch
        {
            "string" => typeof(string),
            "int" => typeof(int),
            "double" => typeof(double),
            "bool" => typeof(bool),
            _ => throw new NotSupportedException(
                $"Persisted parameter value type '{valueType}' is not supported.")
        };
    }

    private static string? FormatValue(
        object? value,
        Type valueType)
    {
        if (value is null)
        {
            return null;
        }

        if (valueType == typeof(string))
        {
            return (string)value;
        }

        if (valueType == typeof(int))
        {
            return ((int)value).ToString(
                CultureInfo.InvariantCulture);
        }

        if (valueType == typeof(double))
        {
            return ((double)value).ToString(
                "R",
                CultureInfo.InvariantCulture);
        }

        if (valueType == typeof(bool))
        {
            return ((bool)value).ToString(
                CultureInfo.InvariantCulture);
        }

        throw new NotSupportedException(
            $"Parameter value type '{valueType.FullName}' is not supported for persistence.");
    }

    private static object? ParseValue(
        string? value,
        Type valueType,
        string key)
    {
        if (valueType == typeof(string))
        {
            return value;
        }

        if (value is null)
        {
            throw new InvalidOperationException(
                $"Persisted parameter '{key}' has a null value for non-nullable type '{valueType.FullName}'.");
        }

        if (valueType == typeof(int))
        {
            return int.Parse(
                value,
                CultureInfo.InvariantCulture);
        }

        if (valueType == typeof(double))
        {
            return double.Parse(
                value,
                CultureInfo.InvariantCulture);
        }

        if (valueType == typeof(bool))
        {
            return bool.Parse(value);
        }

        throw new NotSupportedException(
            $"Persisted parameter value type '{valueType.FullName}' is not supported.");
    }

    private sealed class PersistedParameterCollection
    {
        public PersistedParameter[] Parameters { get; init; } = [];
    }

    private sealed class PersistedParameter
    {
        public string Key { get; init; } = string.Empty;

        public string? Value { get; init; }

        public string ValueType { get; init; } = string.Empty;

        public string? Description { get; init; }

        public string? Category { get; init; }

        public bool IsReadOnly { get; init; }
    }
}
