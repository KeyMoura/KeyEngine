namespace KeyEngine.Timers;

/// <summary>
/// Manages active timers.
/// </summary>
/// <remarks>
/// The manager owns timers created by <see cref="Start(TimeSpan)"/> and
/// <see cref="StartRepeating(TimeSpan)"/>. Completed and cancelled timers are
/// removed from manager tracking during the next update.
/// </remarks>
public sealed class TimerManager
{
    private readonly List<Timer> _timers = [];

    internal int ActiveTimerCount =>
        _timers.Count(timer =>
            timer.State is TimerState.Running or TimerState.Paused);

    /// <summary>
    /// Gets all timers currently tracked by the manager.
    /// </summary>
    /// <remarks>
    /// A completed or cancelled timer may remain in this collection until the
    /// next manager update, but is no longer considered active.
    /// </remarks>
    public IReadOnlyList<Timer> Timers => _timers;

    /// <summary>
    /// Creates and registers a timer.
    /// </summary>
    /// <param name="duration">
    /// The timer duration.
    /// </param>
    /// <returns>
    /// The created timer. The manager retains ownership and removes it from
    /// tracking after it completes or is cancelled.
    /// </returns>
    public Timer Start(
        TimeSpan duration)
    {
        Timer timer = new(duration);

        timer.Start();

        _timers.Add(timer);

        return timer;
    }

    /// <summary>
    /// Starts a repeating timer.
    /// </summary>
    /// <param name="duration">
    /// The duration of each repetition.
    /// </param>
    /// <returns>
    /// The created timer. The manager retains ownership until it is cancelled.
    /// </returns>
    public Timer StartRepeating(
        TimeSpan duration)
    {
        Timer timer = new(
            duration,
            true);

        timer.Start();

        _timers.Add(timer);

        return timer;
    }

    /// <summary>
    /// Adds a timer.
    /// </summary>
    /// <param name="timer">
    /// The timer to track. Completed and cancelled timers are removed during
    /// the next manager update.
    /// </param>
    public void Add(
        Timer timer)
    {
        ArgumentNullException.ThrowIfNull(timer);

        _timers.Add(timer);
    }

    /// <summary>
    /// Removes a timer.
    /// </summary>
    /// <param name="timer">
    /// The timer.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if removed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool Remove(
        Timer timer)
    {
        ArgumentNullException.ThrowIfNull(timer);

        return _timers.Remove(timer);
    }

    /// <summary>
    /// Updates all active timers.
    /// </summary>
    /// <param name="deltaTime">
    /// The elapsed frame time.
    /// </param>
    internal void Update(
        TimeSpan deltaTime)
    {
        for (int i = _timers.Count - 1; i >= 0; i--)
        {
            Timer timer = _timers[i];

            timer.Update(deltaTime);

            if (timer.State is TimerState.Completed or TimerState.Cancelled)
            {
                _timers.Remove(timer);
            }
        }
    }
}
