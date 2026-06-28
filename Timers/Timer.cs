namespace KeyEngine.Timers;

/// <summary>
/// Represents a timer.
/// </summary>
public sealed class Timer
{
    /// <summary>
    /// Gets the timer duration.
    /// </summary>
    public TimeSpan Duration
    {
        get;
    }

    /// <summary>
    /// Gets the elapsed time.
    /// </summary>
    public TimeSpan Elapsed
    {
        get;
        internal set;
    }

    /// <summary>
    /// Gets a value indicating whether the timer repeats.
    /// </summary>
    public bool IsRepeating
    {
        get;
    }

    /// <summary>
    /// Gets the remaining time.
    /// </summary>
    public TimeSpan Remaining =>
        Duration - Elapsed;

    /// <summary>
    /// Gets the completion progress.
    /// </summary>
    public double Progress =>
        Duration == TimeSpan.Zero
            ? 1.0
            : Elapsed.TotalSeconds / Duration.TotalSeconds;

    /// <summary>
    /// Gets the current timer state.
    /// </summary>
    public TimerState State
    {
        get;
        private set;
    } = TimerState.Ready;

    /// <summary>
    /// Gets a value indicating whether the timer is running.
    /// </summary>
    public bool IsRunning =>
        State == TimerState.Running;

    /// <summary>
    /// Gets a value indicating whether the timer has completed.
    /// </summary>
    public bool IsCompleted =>
        State == TimerState.Completed;

    /// <summary>
    /// Occurs when the timer completes.
    /// </summary>
    public event EventHandler? Completed;

    /// <summary>
    /// Occurs when the timer is cancelled.
    /// </summary>
    public event EventHandler? Cancelled;

    /// <summary>
    /// Occurs when the timer is paused.
    /// </summary>
    public event EventHandler? Paused;

    /// <summary>
    /// Occurs when the timer resumes.
    /// </summary>
    public event EventHandler? Resumed;

    /// <summary>
    /// Initializes a new timer.
    /// </summary>
    public Timer(
        TimeSpan duration,
        bool repeating = false)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration));
        }

        Duration = duration;

        IsRepeating = repeating;
    }

    /// <summary>
    /// Starts the timer.
    /// </summary>
    public void Start()
    {
        if (State != TimerState.Ready)
        {
            return;
        }

        State = TimerState.Running;
    }

    /// <summary>
    /// Pauses the timer.
    /// </summary>
    public void Pause()
    {
        if (State != TimerState.Running)
        {
            return;
        }

        State = TimerState.Paused;

        Paused?.Invoke(
            this,
            EventArgs.Empty);
    }

    /// <summary>
    /// Resumes the timer.
    /// </summary>
    public void Resume()
    {
        if (State != TimerState.Paused)
        {
            return;
        }

        State = TimerState.Running;

        Resumed?.Invoke(
            this,
            EventArgs.Empty);
    }

    /// <summary>
    /// Cancels the timer.
    /// </summary>
    public void Cancel()
    {
        if (State == TimerState.Completed ||
            State == TimerState.Cancelled)
        {
            return;
        }

        State = TimerState.Cancelled;

        Cancelled?.Invoke(
            this,
            EventArgs.Empty);
    }

    /// <summary>
    /// Restarts the timer.
    /// </summary>
    public void Restart()
    {
        Elapsed = TimeSpan.Zero;

        State = TimerState.Running;
    }

    /// <summary>
    /// Advances the timer.
    /// </summary>
    /// <param name="deltaTime">
    /// The elapsed time.
    /// </param>
    internal void Update(
        TimeSpan deltaTime)
    {
        if (!IsRunning)
        {
            return;
        }

        Elapsed += deltaTime;

        if (Elapsed >= Duration)
        {
            Elapsed = Duration;

            Completed?.Invoke(
                this,
                EventArgs.Empty);

            if (IsRepeating)
            {
                Elapsed = TimeSpan.Zero;
            }
            else
            {
                State = TimerState.Completed;
            }
        }
    }
}