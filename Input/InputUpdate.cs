using KeyEngine.Input.Keyboard;
using KeyEngine.Input.Pointer;
using KeyEngine.Numerics;

namespace KeyEngine.Input;

/// <summary>
/// Provides input sources with controlled access to the current input state.
/// </summary>
public sealed class InputUpdate
{
    private readonly HashSet<Key> _keys = [];
    private readonly HashSet<MouseButton> _pointerButtons = [];

    internal IReadOnlySet<Key> Keys => _keys;

    internal IReadOnlySet<MouseButton> PointerButtons => _pointerButtons;

    internal bool HasPointerPosition { get; private set; }

    internal Vector2 PointerPosition { get; private set; }

    internal Vector2 PointerScroll { get; private set; }

    internal InputUpdate()
    {
    }

    /// <summary>
    /// Sets whether a keyboard key is currently down.
    /// </summary>
    /// <param name="key">
    /// The keyboard key.
    /// </param>
    /// <param name="isDown">
    /// <see langword="true"/> when the key is down; otherwise,
    /// <see langword="false"/>.
    /// </param>
    public void SetKey(
        Key key,
        bool isDown)
    {
        if (isDown)
        {
            _keys.Add(key);
        }
        else
        {
            _keys.Remove(key);
        }
    }

    /// <summary>
    /// Sets whether a pointer button is currently down.
    /// </summary>
    /// <param name="button">
    /// The pointer button.
    /// </param>
    /// <param name="isDown">
    /// <see langword="true"/> when the button is down; otherwise,
    /// <see langword="false"/>.
    /// </param>
    public void SetPointerButton(
        MouseButton button,
        bool isDown)
    {
        if (isDown)
        {
            _pointerButtons.Add(button);
        }
        else
        {
            _pointerButtons.Remove(button);
        }
    }

    /// <summary>
    /// Sets the current pointer position.
    /// </summary>
    /// <param name="position">
    /// The pointer position in source-defined coordinates.
    /// </param>
    public void SetPointerPosition(Vector2 position)
    {
        PointerPosition = position;
        HasPointerPosition = true;
    }

    /// <summary>
    /// Adds a pointer scroll delta for the current frame.
    /// </summary>
    /// <param name="delta">
    /// The scroll delta.
    /// </param>
    public void AddPointerScroll(Vector2 delta)
    {
        PointerScroll += delta;
    }

    internal void BeginFrame()
    {
        HasPointerPosition = false;
        PointerScroll = Vector2.Zero;
    }
}
