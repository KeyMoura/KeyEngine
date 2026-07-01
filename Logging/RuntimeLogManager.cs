namespace KeyEngine.Logging;

/// <summary>
/// Stores recent runtime log entries in memory.
/// </summary>
/// <remarks>
/// The runtime log is bounded and process-local. It is intended for diagnostics
/// and local/admin inspection, not durable logging.
/// </remarks>
public sealed class RuntimeLogManager
{
    /// <summary>
    /// The default maximum number of entries retained by the runtime log.
    /// </summary>
    public const int DefaultCapacity = 500;

    private readonly List<RuntimeLogEntry> _entries = new();

    private readonly int _capacity;

    /// <summary>
    /// Initializes a new runtime log manager.
    /// </summary>
    /// <param name="capacity">
    /// The maximum number of recent entries to retain.
    /// </param>
    public RuntimeLogManager(int capacity = DefaultCapacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                "Runtime log capacity must be greater than zero.");
        }

        _capacity = capacity;
    }

    /// <summary>
    /// Gets the maximum number of entries retained by the runtime log.
    /// </summary>
    public int Capacity => _capacity;

    /// <summary>
    /// Adds a runtime log entry.
    /// </summary>
    /// <param name="level">
    /// The non-blank log level.
    /// </param>
    /// <param name="message">
    /// The non-blank log message.
    /// </param>
    /// <param name="category">
    /// An optional category.
    /// </param>
    /// <param name="source">
    /// An optional source.
    /// </param>
    /// <returns>
    /// The entry that was added.
    /// </returns>
    public RuntimeLogEntry Add(
        string level,
        string message,
        string? category = null,
        string? source = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(level);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        RuntimeLogEntry entry = new(
            DateTimeOffset.UtcNow,
            level,
            message,
            category,
            source);

        _entries.Add(entry);

        while (_entries.Count > _capacity)
        {
            _entries.RemoveAt(0);
        }

        return entry;
    }

    /// <summary>
    /// Gets the recent runtime log entries in insertion order.
    /// </summary>
    public IReadOnlyList<RuntimeLogEntry> GetRecent()
    {
        return _entries.ToArray();
    }

    /// <summary>
    /// Clears all retained runtime log entries.
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
    }
}
