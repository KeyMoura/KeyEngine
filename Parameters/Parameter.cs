namespace KeyEngine.Parameters;

/// <summary>
/// Describes a runtime parameter and its current value.
/// </summary>
public sealed class Parameter
{
    /// <summary>
    /// Gets the parameter key.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the current parameter value.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    /// Gets the declared value type.
    /// </summary>
    public required Type ValueType { get; init; }

    /// <summary>
    /// Gets the optional parameter description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the optional parameter category.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets a value indicating whether the parameter can be changed or removed.
    /// </summary>
    public bool IsReadOnly { get; init; }
}
