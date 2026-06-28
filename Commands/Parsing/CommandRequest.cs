namespace KeyEngine.Commands.Parsing;

/// <summary>
/// Represents a parsed command request.
/// </summary>
public sealed class CommandRequest
{
    /// <summary>
    /// Gets or sets the command name.
    /// </summary>
    public required string Name
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the command arguments.
    /// </summary>
    public IList<string> Arguments
    {
        get;
        init;
    } = [];

    public string RawText { get; init; }

    /// <summary>
    /// Gets the source that issued the command.
    /// </summary>
    public required ICommandSource Source
    {
        get;
        init;
    }
}