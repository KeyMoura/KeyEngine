namespace KeyEngine.Input.Keyboard;

/// <summary>
/// Provides read-only access to keyboard state for the current frame.
/// </summary>
public sealed class KeyboardState
{
    private readonly HashSet<Key> _down = [];
    private readonly HashSet<Key> _pressed = [];
    private readonly HashSet<Key> _released = [];

    internal KeyboardState()
    {
    }

    /// <summary>
    /// Determines whether a key is currently down.
    /// </summary>
    /// <param name="key">
    /// The keyboard key.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the key is down; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool IsDown(Key key)
    {
        return _down.Contains(key);
    }

    /// <summary>
    /// Determines whether a key was pressed during the current frame.
    /// </summary>
    /// <param name="key">
    /// The keyboard key.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the key was pressed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool WasPressed(Key key)
    {
        return _pressed.Contains(key);
    }

    /// <summary>
    /// Determines whether a key was released during the current frame.
    /// </summary>
    /// <param name="key">
    /// The keyboard key.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the key was released; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool WasReleased(Key key)
    {
        return _released.Contains(key);
    }

    internal void Apply(IReadOnlySet<Key> keys)
    {
        _pressed.Clear();
        _released.Clear();

        foreach (Key key in keys)
        {
            if (!_down.Contains(key))
            {
                _pressed.Add(key);
            }
        }

        foreach (Key key in _down)
        {
            if (!keys.Contains(key))
            {
                _released.Add(key);
            }
        }

        _down.Clear();
        _down.UnionWith(keys);
    }
}
