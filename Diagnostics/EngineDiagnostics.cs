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
    /// <remarks>
    /// Running and paused timers are active. Ready, completed, and cancelled
    /// timers are not included.
    /// </remarks>
    public int ActiveTimerCount =>
        _engine.TimerManager.ActiveTimerCount;

    /// <summary>
    /// Gets diagnostic information for all loaded plugins.
    /// </summary>
    public IReadOnlyList<PluginDiagnostics> Plugins
    {
        get
        {
            List<PluginDiagnostics> plugins = new();

            foreach (LoadedPlugin plugin in _engine.PluginManager.Plugins)
            {
                PluginBuilder builder =
                    _engine.PluginManager.GetBuilder(plugin);

                plugins.Add(new PluginDiagnostics
                {
                    Id = plugin.Manifest.Id,
                    Name = plugin.Manifest.Name,
                    Version = plugin.Manifest.Version,
                    State = PluginState.Running,
                    SystemCount = builder.ScanResult.Systems.Count,
                    CommandCount = builder.ScanResult.Commands.Count,
                    EventListenerCount = builder.ScanResult.EventListeners.Count,
                    ServiceCount = builder.Services.Services.Count
                });
            }

            return plugins;
        }
    }
}
