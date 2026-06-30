namespace KeyEngine.Scheduler;

/// <summary>
/// Represents configuration options for the engine scheduler.
/// </summary>
public sealed class SchedulerOptions
{
    private int _ticksPerSecond = 60;

    /// <summary>
    /// Gets or sets the number of fixed updates executed per second.
    /// </summary>
    /// <remarks>
    /// The default value is 60 ticks per second.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than or equal to zero.
    /// </exception>
    public int TicksPerSecond
    {
        get => _ticksPerSecond;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Ticks per second must be greater than zero.");
            }

            _ticksPerSecond = value;
        }
    }

    /// <summary>
    /// Gets or sets the maximum frame rate.
    /// </summary>
    /// <remarks>
    /// Frame limiting is not currently supported. The only supported value is
    /// 0, which disables frame limiting.
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Thrown when the value is not zero.
    /// </exception>
    public int TargetFrameRate
    {
        get => 0;
        set
        {
            if (value != 0)
            {
                throw new NotSupportedException(
                    "Frame limiting is not currently supported.");
            }
        }
    }
}
