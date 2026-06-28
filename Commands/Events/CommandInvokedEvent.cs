using KeyEngine.Commands.Parsing;
using KeyEngine.Events;

namespace KeyEngine.Commands.Events;

/// <summary>
/// Raised after a command has been invoked.
/// </summary>
public sealed class CommandInvokedEvent
    : IEvent
{
    /// <summary>
    /// Gets the executed command.
    /// </summary>
    public required CommandRequest Request
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the command result.
    /// </summary>
    public object? Result
    {
        get;
        init;
    }
}