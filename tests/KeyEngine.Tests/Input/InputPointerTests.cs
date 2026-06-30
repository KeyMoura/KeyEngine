using KeyEngine.Core;
using KeyEngine.Input;
using KeyEngine.Input.Keyboard;
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

    [Fact]
    public void SharedKey_RemainsDownUntilEverySourceReleasesIt()
    {
        Engine engine = TestEngineFactory.Create();
        KeyInputSource first = new(Key.A);
        KeyInputSource second = new(Key.A);
        engine.Input.RegisterSource(first);
        engine.Input.RegisterSource(second);

        try
        {
            engine.Initialize();
            engine.Tick();

            Assert.True(engine.Input.Keyboard.IsDown(Key.A));
            Assert.True(engine.Input.Keyboard.WasPressed(Key.A));

            first.IsDown = false;
            engine.Tick();

            Assert.True(engine.Input.Keyboard.IsDown(Key.A));
            Assert.False(engine.Input.Keyboard.WasReleased(Key.A));

            second.IsDown = false;
            engine.Tick();

            Assert.False(engine.Input.Keyboard.IsDown(Key.A));
            Assert.True(engine.Input.Keyboard.WasReleased(Key.A));
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

    private sealed class KeyInputSource(Key key)
        : IInputSource
    {
        public bool IsDown { get; set; } = true;

        public void Update(InputUpdate update)
        {
            update.SetKey(key, IsDown);
        }
    }
}
