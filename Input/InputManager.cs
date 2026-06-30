using KeyEngine.Input.Keyboard;
using KeyEngine.Input.Pointer;
using KeyEngine.Numerics;

namespace KeyEngine.Input;

/// <summary>
/// Manages input sources and exposes the current input state.
/// </summary>
public sealed class InputManager
{
    private readonly List<(IInputSource Source, InputUpdate Update)> _sources = [];
    private readonly HashSet<Key> _keys = [];
    private readonly HashSet<MouseButton> _pointerButtons = [];

    /// <summary>
    /// Gets the current keyboard state.
    /// </summary>
    public KeyboardState Keyboard { get; } = new();

    /// <summary>
    /// Gets the current pointer state.
    /// </summary>
    public PointerState Pointer { get; } = new();

    /// <summary>
    /// Initializes a new input manager.
    /// </summary>
    public InputManager()
    {
    }

    /// <summary>
    /// Registers an input source.
    /// </summary>
    /// <param name="source">
    /// The input source.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the source is already registered.
    /// </exception>
    public void RegisterSource(IInputSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (_sources.Any(entry => entry.Source.Equals(source)))
        {
            throw new InvalidOperationException(
                "The input source is already registered.");
        }

        _sources.Add((source, new InputUpdate()));
    }

    internal void Update()
    {
        foreach ((IInputSource _, InputUpdate update) in _sources)
        {
            update.BeginFrame();
        }

        foreach ((IInputSource source, InputUpdate update) in _sources)
        {
            source.Update(update);
        }

        _keys.Clear();
        _pointerButtons.Clear();

        bool hasPointerPosition = false;
        Vector2 pointerPosition = Vector2.Zero;
        Vector2 pointerScroll = Vector2.Zero;

        foreach ((IInputSource _, InputUpdate update) in _sources)
        {
            _keys.UnionWith(update.Keys);
            _pointerButtons.UnionWith(update.PointerButtons);
            pointerScroll += update.PointerScroll;

            if (!update.HasPointerPosition)
            {
                continue;
            }

            if (hasPointerPosition)
            {
                throw new InvalidOperationException(
                    "Multiple input sources reported a pointer position during the same frame.");
            }

            hasPointerPosition = true;
            pointerPosition = update.PointerPosition;
        }

        Keyboard.Apply(_keys);
        Pointer.Apply(
            _pointerButtons,
            hasPointerPosition,
            pointerPosition,
            pointerScroll);
    }
}
