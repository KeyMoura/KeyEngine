namespace KeyEngine.Commands;

/// <summary>
/// Represents the engine console as a command source.
/// </summary>
public sealed class ConsoleCommandSource
    : ICommandSource
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static ConsoleCommandSource Instance { get; } = new();

    /// <inheritdoc/>
    public string Name => "Console";

    private ConsoleCommandSource()
    {
    }
}