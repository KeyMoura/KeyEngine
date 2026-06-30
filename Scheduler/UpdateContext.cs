namespace KeyEngine.Scheduler;

/// <summary>
/// Provides timing information for an update invocation.
/// </summary>
public sealed class UpdateContext
{
    /// <summary>
    /// Gets the current frame number.
    /// </summary>
    public required long FrameNumber { get; init; }

    /// <summary>
    /// Gets the elapsed time since the previous frame.
    /// </summary>
    public required TimeSpan DeltaTime { get; init; }

    /// <summary>
    /// Gets the total elapsed engine time.
    /// </summary>
    public required TimeSpan ElapsedTime { get; init; }
}