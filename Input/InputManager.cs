using KeyEngine.Input.Keyboard;
using KeyEngine.Input.Pointer;
using KeyEngine.Numerics;

namespace KeyEngine.Input;

/// <summary>
/// Manages input sources and exposes the current input state.
/// </summary>
/// <remarks>
/// Input sources are updated synchronously on the engine thread. Instances are
/// not guaranteed to be thread-safe. The first source to report pointer
/// position becomes its owner until that source is unregistered. Position
/// updates from other sources are rejected.
/// </remarks>
public sealed class InputManager
{
    private readonly List<(IInputSource Source, InputUpdate Update)> _sources = [];
    private readonly HashSet<Key> _keys = [];
    private readonly HashSet<MouseButton> _pointerButtons = [];
    private IInputSource? _pointerSource;
    private bool _isUpdating;

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
    /// Thrown when the source is already registered or input is updating.
    /// </exception>
    public void RegisterSource(IInputSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        EnsureNotUpdating();

        if (_sources.Any(entry => ReferenceEquals(
                entry.Source,
                source)))
        {
            throw new InvalidOperationException(
                "The input source is already registered.");
        }

        _sources.Add((source, new InputUpdate()));
    }

    /// <summary>
    /// Removes a registered input source.
    /// </summary>
    /// <param name="source">
    /// The input source.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the source was removed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when input is updating.
    /// </exception>
    public bool UnregisterSource(IInputSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        EnsureNotUpdating();

        int index = _sources.FindIndex(entry =>
            ReferenceEquals(
                entry.Source,
                source));

        if (index < 0)
        {
            return false;
        }

        _sources.RemoveAt(index);

        if (ReferenceEquals(
                _pointerSource,
                source))
        {
            _pointerSource = null;
            Pointer.ResetPositionBaseline();
        }

        return true;
    }

    internal void Update()
    {
        _isUpdating = true;

        try
        {
            foreach ((IInputSource source, InputUpdate update) in _sources)
            {
                update.BeginUpdate();

                try
                {
                    source.Update(update);
                }
                finally
                {
                    update.EndUpdate();
                }
            }

            ApplySources();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void ApplySources()
    {
        _keys.Clear();
        _pointerButtons.Clear();

        bool hasPointerPosition = false;
        Vector2 pointerPosition = Vector2.Zero;
        Vector2 pointerScroll = Vector2.Zero;

        foreach ((IInputSource source, InputUpdate update) in _sources)
        {
            _keys.UnionWith(update.Keys);
            _pointerButtons.UnionWith(update.PointerButtons);
            pointerScroll += update.PointerScroll;

            if (!update.HasPointerPosition)
            {
                continue;
            }

            if (_pointerSource is null)
            {
                _pointerSource = source;
            }

            if (!ReferenceEquals(
                    _pointerSource,
                    source))
            {
                throw new InvalidOperationException(
                    "Pointer position is already owned by another input source.");
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

    private void EnsureNotUpdating()
    {
        if (_isUpdating)
        {
            throw new InvalidOperationException(
                "Input sources cannot be changed while input is updating.");
        }
    }
}
