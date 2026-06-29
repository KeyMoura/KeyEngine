using KeyEngine.Commands;
using KeyEngine.Core;
using KeyEngine.Events;
using KeyEngine.Plugins;
using KeyEngine.Systems;
using KeyEngine.Timers;

namespace KeyEngine.Diagnostics;

/// <summary>
/// Provides live diagnostic information about the engine.
/// </summary>
public sealed class EngineDiagnostics
{
    private readonly Engine _engine;

    internal EngineDiagnostics(
        Engine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);

        _engine = engine;
    }

    /// <summary>
    /// Gets the current engine state.
    /// </summary>
    public EngineState State =>
        _engine.State;

    /// <summary>
    /// Gets the current frame number.
    /// </summary>
    public long FrameNumber =>
        _engine.Scheduler.FrameNumber;

    /// <summary>
    /// Gets the engine uptime.
    /// </summary>
    public TimeSpan Uptime =>
        _engine.Scheduler.ElapsedTime;

    /// <summary>
    /// Gets the number of loaded plugins.
    /// </summary>
    public int PluginCount =>
        _engine.PluginManager.Plugins.Count;

    /// <summary>
    /// Gets the number of registered commands.
    /// </summary>
    public int CommandCount =>
        _engine.ScanResult.Commands.Count;

    /// <summary>
    /// Gets the number of event listeners.
    /// </summary>
    public int EventListenerCount =>
        _engine.ScanResult.EventListeners.Count;

    /// <summary>
    /// Gets the number of registered systems.
    /// </summary>
    public int SystemCount =>
        _engine.ScanResult.Systems.Count;

    /// <summary>
    /// Gets the number of active timers.
    /// </summary>
    public int ActiveTimerCount =>
        _engine.TimerManager.Timers.Count;
}