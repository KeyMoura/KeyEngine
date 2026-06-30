using KeyEngine.Core;
using KeyEngine.Input;
using KeyEngine.Numerics;

namespace KeyEngine.Tests.Input;

public sealed class InputPointerTests
{
    [Fact]
    public void FirstPointerPosition_EstablishesBaselineWithZeroDelta()
    {
        Engine engine = TestEngineFactory.Create();
        PositionInputSource source = new(new Vector2(25f, 40f));
        engine.Input.RegisterSource(source);

        try
        {
            engine.Initialize();
            engine.Tick();

            Assert.Equal(new Vector2(25f, 40f), engine.Input.Pointer.Position);
            Assert.Equal(Vector2.Zero, engine.Input.Pointer.Delta);
        }
        finally
        {
            engine.Shutdown();
        }
    }

    private sealed class PositionInputSource(Vector2 position)
        : IInputSource
    {
        public void Update(InputUpdate update)
        {
            update.SetPointerPosition(position);
        }
    }
}
