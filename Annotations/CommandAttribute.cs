namespace KeyEngine.Annotations;

/// <summary>
/// Marks a method as an engine command.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the command name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets alternate names for the command.
    /// </summary>
    public string[] Aliases { get; init; } = [];

    /// <summary>
    /// Gets or sets a short description of the command.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the command usage string.
    /// </summary>
    public string Usage { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the command category.
    /// </summary>
    public string Category { get; init; } = string.Empty;
}