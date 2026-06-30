using KeyEngine.Numerics;

namespace KeyEngine.Input.Pointer;

/// <summary>
/// Provides read-only access to pointer state for the current frame.
/// </summary>
public sealed class PointerState
{
    private readonly HashSet<MouseButton> _down = [];
    private readonly HashSet<MouseButton> _pressed = [];
    private readonly HashSet<MouseButton> _released = [];
    private bool _hasPosition;

    /// <summary>
    /// Gets the current pointer position in source-defined coordinates.
    /// </summary>
    public Vector2 Position { get; private set; }

    /// <summary>
    /// Gets the pointer movement during the current frame.
    /// </summary>
    public Vector2 Delta { get; private set; }

    /// <summary>
    /// Gets the pointer scroll movement during the current frame.
    /// </summary>
    public Vector2 ScrollDelta { get; private set; }

    internal PointerState()
    {
    }

    /// <summary>
    /// Determines whether a pointer button is currently down.
    /// </summary>
    /// <param name="button">
    /// The pointer button.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the button is down; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool IsDown(MouseButton button)
    {
        return _down.Contains(button);
    }

    /// <summary>
    /// Determines whether a pointer button was pressed during the current frame.
    /// </summary>
    /// <param name="button">
    /// The pointer button.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the button was pressed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool WasPressed(MouseButton button)
    {
        return _pressed.Contains(button);
    }

    /// <summary>
    /// Determines whether a pointer button was released during the current frame.
    /// </summary>
    /// <param name="button">
    /// The pointer button.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the button was released; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool WasReleased(MouseButton button)
    {
        return _released.Contains(button);
    }

    internal void Apply(
        IReadOnlySet<MouseButton> buttons,
        bool hasPosition,
        Vector2 position,
        Vector2 scrollDelta)
    {
        _pressed.Clear();
        _released.Clear();

        foreach (MouseButton button in buttons)
        {
            if (!_down.Contains(button))
            {
                _pressed.Add(button);
            }
        }

        foreach (MouseButton button in _down)
        {
            if (!buttons.Contains(button))
            {
                _released.Add(button);
            }
        }

        _down.Clear();
        _down.UnionWith(buttons);

        Delta = hasPosition && _hasPosition
            ? position - Position
            : Vector2.Zero;

        if (hasPosition)
        {
            Position = position;
            _hasPosition = true;
        }

        ScrollDelta = scrollDelta;
    }

    internal void ResetPositionBaseline()
    {
        _hasPosition = false;
        Delta = Vector2.Zero;
    }
}
