using KeyEngine.Core;

namespace KeyEngine.Tests.Core;

public sealed class EngineLifecycleTests
{
    [Fact]
    public void Lifecycle_InitializeTickStopAndShutdown_TransitionsAndInvokesSystems()
    {
        LifecycleTestSystem.Reset();
        Engine engine = TestEngineFactory.Create();

        Assert.Equal(EngineState.Stopped, engine.State);

        engine.Initialize();

        Assert.Equal(EngineState.Running, engine.State);
        Assert.Equal(1, LifecycleTestSystem.StartupCount);

        engine.Tick();

        Assert.Equal(1, LifecycleTestSystem.UpdateCount);

        engine.Stop();

        Assert.Equal(EngineState.ShuttingDown, engine.State);

        engine.Shutdown();
        engine.Shutdown();

        Assert.Equal(EngineState.Stopped, engine.State);
        Assert.Equal(1, LifecycleTestSystem.ShutdownCount);
    }
}
