using KeyEngine.Commands.Parsing;
using KeyEngine.Events;
using KeyEngine.Events.Models;

namespace KeyEngine.Commands.Events;

/// <summary>
/// Raised before a command is invoked.
/// </summary>
public sealed class CommandInvokingEvent
    : CancellableEvent
{
    /// <summary>
    /// Gets the command request.
    /// </summary>
    public required CommandRequest Request
    {
        get;
        init;
    }
}