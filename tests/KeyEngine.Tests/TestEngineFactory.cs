using KeyEngine.Abstractions;
using KeyEngine.Annotations;
using KeyEngine.Core;
using KeyEngine.Timers;

namespace KeyEngine.Tests;

internal static class TestEngineFactory
{
    public static Engine Create()
    {
        return new EngineBuilder()
            .SetPluginDirectory(Path.Combine(
                Path.GetTempPath(),
                $"KeyEngine.Tests-{Guid.NewGuid():N}"))
            .RegisterAssembly(typeof(TestEngineFactory).Assembly)
            .Build();
    }
}

internal sealed class LifecycleTestSystem
    : IEngineSystem
{
    public static int StartupCount { get; private set; }

    public static int UpdateCount { get; private set; }

    public static int ShutdownCount { get; private set; }

    public static void Reset()
    {
        StartupCount = 0;
        UpdateCount = 0;
        ShutdownCount = 0;
    }

    [OnStart]
    private void Start()
    {
        StartupCount++;
    }

    [OnUpdate]
    private void Update()
    {
        UpdateCount++;
    }

    [OnShutdown]
    private void Shutdown()
    {
        ShutdownCount++;
    }
}

internal sealed class TimerAccessSystem
    : IEngineSystem
{
    private readonly TimerManager _timers;

    public static TimerManager? Timers { get; private set; }

    public TimerAccessSystem(TimerManager timers)
    {
        _timers = timers;
    }

    [OnStart]
    private void Start()
    {
        Timers = _timers;
    }
}
