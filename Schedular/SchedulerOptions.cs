namespace KeyEngine.Scheduler;

/// <summary>
/// Represents configuration options for the engine scheduler.
/// </summary>
public sealed class SchedulerOptions
{
    /// <summary>
    /// Gets or sets the number of fixed updates executed per second.
    /// </summary>
    /// <remarks>
    /// The default value is 60 ticks per second.
    /// </remarks>
    public int TicksPerSecond { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum frame rate.
    /// </summary>
    /// <remarks>
    /// A value of 0 disables frame limiting.
    /// </remarks>
    public int TargetFrameRate { get; set; } = 0;
}