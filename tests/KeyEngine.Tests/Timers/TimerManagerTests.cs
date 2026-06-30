using KeyEngine.Core;
using KeyEngine.Timers;
using Timer = KeyEngine.Timers.Timer;

namespace KeyEngine.Tests.Timers;

public sealed class TimerManagerTests
{
    [Fact]
    public void Tick_CompletedTimer_RemovesItFromTrackingAndActiveCount()
    {
        Engine engine = TestEngineFactory.Create();

        try
        {
            engine.Initialize();
            TimerManager timers = Assert.IsType<TimerManager>(TimerAccessSystem.Timers);
            Timer timer = timers.Start(TimeSpan.Zero);

            Assert.Equal(1, engine.Diagnostics.ActiveTimerCount);

            engine.Tick();

            Assert.True(timer.IsCompleted);
            Assert.Empty(timers.Timers);
            Assert.Equal(0, engine.Diagnostics.ActiveTimerCount);
        }
        finally
        {
            engine.Shutdown();
        }
    }
}
