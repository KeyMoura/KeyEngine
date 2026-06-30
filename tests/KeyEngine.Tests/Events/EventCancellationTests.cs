using KeyEngine.Abstractions;
using KeyEngine.Annotations;
using KeyEngine.Core;
using KeyEngine.Events;

namespace KeyEngine.Tests.Events;

public sealed class EventCancellationTests
{
    [Fact]
    public void Publish_AfterCancellation_SkipsOnlyListenersThatIgnoreCancelledEvents()
    {
        CancellationTestSystem.Reset();
        Engine engine = TestEngineFactory.Create();

        try
        {
            engine.Initialize();

            TestCancellableEvent published =
                engine.Events.Publish(new TestCancellableEvent());

            Assert.True(published.IsCancelled);
            Assert.Equal(1, CancellationTestSystem.CancellingCount);
            Assert.Equal(0, CancellationTestSystem.IgnoringCount);
            Assert.Equal(1, CancellationTestSystem.ObservingCount);
        }
        finally
        {
            engine.Shutdown();
        }
    }
}

internal sealed class TestCancellableEvent
    : CancellableEvent
{
}

internal sealed class CancellationTestSystem
    : IEngineSystem
{
    public static int CancellingCount { get; private set; }

    public static int IgnoringCount { get; private set; }

    public static int ObservingCount { get; private set; }

    public static void Reset()
    {
        CancellingCount = 0;
        IgnoringCount = 0;
        ObservingCount = 0;
    }

    [EventListener(Order = 0)]
    private void Cancel(TestCancellableEvent @event)
    {
        CancellingCount++;
        @event.IsCancelled = true;
    }

    [EventListener(IgnoreCancelled = true, Order = 1)]
    private void IgnoreCancelled(TestCancellableEvent @event)
    {
        IgnoringCount++;
    }

    [EventListener(Order = 2)]
    private void ObserveCancelled(TestCancellableEvent @event)
    {
        ObservingCount++;
    }
}
