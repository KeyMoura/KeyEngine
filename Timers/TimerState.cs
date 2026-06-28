namespace KeyEngine.Timers;

/// <summary>
/// Represents the state of a timer.
/// </summary>
public enum TimerState
{
    Ready,

    Running,

    Paused,

    Completed,

    Cancelled
}