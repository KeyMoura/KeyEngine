namespace KeyEngine.Timers;

/// <summary>
/// Manages active timers.
/// </summary>
public sealed class TimerManager
{
    private readonly List<Timer> _timers = [];

    /// <summary>
    /// Gets all active timers.
    /// </summary>
    public IReadOnlyList<Timer> Timers => _timers;

    /// <summary>
    /// Creates and registers a timer.
    /// </summary>
    /// <param name="duration">
    /// The timer duration.
    /// </param>
    /// <returns>
    /// The created timer.
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
    /// The timer.
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
        foreach (Timer timer in _timers)
        {
            timer.Update(deltaTime);
        }
    }
}