namespace KeyEngine.Commands;

/// <summary>
/// Represents the source of a command.
/// </summary>
public interface ICommandSource
{
    /// <summary>
    /// Gets the source name.
    /// </summary>
    string Name
    {
        get;
    }
}