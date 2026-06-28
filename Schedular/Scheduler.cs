using System.Diagnostics;

namespace KeyEngine.Scheduler;

/// <summary>
/// Provides timing information for the engine.
/// </summary>
/// <remarks>
/// The scheduler is responsible for tracking elapsed time,
/// frame timing, and fixed update timing.
/// </remarks>
public sealed class Scheduler
{
    private readonly Stopwatch _stopwatch = new();

    private TimeSpan _lastFrameTime;
    private TimeSpan _fixedUpdateAccumulator;

    /// <summary>
    /// Gets the scheduler configuration.
    /// </summary>
    public SchedulerOptions Options { get; }

    /// <summary>
    /// Gets the elapsed time since the previous frame.
    /// </summary>
    public TimeSpan DeltaTime { get; private set; }

    /// <summary>
    /// Gets the total elapsed time.
    /// </summary>
    public TimeSpan ElapsedTime => _stopwatch.Elapsed;

    /// <summary>
    /// Gets the current frame number.
    /// </summary>
    public long FrameNumber { get; private set; }

    /// <summary>
    /// Gets the duration of a single fixed update.
    /// </summary>
    public TimeSpan FixedTimeStep =>
        TimeSpan.FromSeconds(1.0 / Options.TicksPerSecond);

    /// <summary>
    /// Initializes a new instance of the <see cref="Scheduler"/> class.
    /// </summary>
    /// <param name="options">
    /// The scheduler configuration.
    /// </param>
    public Scheduler(SchedulerOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// Starts the scheduler.
    /// </summary>
    public void Start()
    {
        _stopwatch.Start();
        _lastFrameTime = _stopwatch.Elapsed;
    }

    /// <summary>
    /// Begins a new frame.
    /// </summary>
    public void BeginFrame()
    {
        TimeSpan now = _stopwatch.Elapsed;

        DeltaTime = now - _lastFrameTime;

        _lastFrameTime = now;

        _fixedUpdateAccumulator += DeltaTime;

        FrameNumber++;
    }

    /// <summary>
    /// Determines whether a fixed update should execute.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a fixed update should run; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool ShouldRunFixedUpdate()
    {
        if (_fixedUpdateAccumulator < FixedTimeStep)
        {
            return false;
        }

        _fixedUpdateAccumulator -= FixedTimeStep;

        return true;
    }

    /// <summary>
    /// Ends the current frame.
    /// </summary>
    public void EndFrame()
    {
        // Frame limiting will be implemented here later.
    }
}