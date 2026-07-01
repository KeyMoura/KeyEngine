namespace KeyEngine.Logging;

/// <summary>
/// Represents a runtime log entry captured by the engine.
/// </summary>
/// <param name="Timestamp">
/// The time the entry was recorded.
/// </param>
/// <param name="Level">
/// The log level, such as <c>Info</c>, <c>Warning</c>, or <c>Error</c>.
/// </param>
/// <param name="Message">
/// The log message.
/// </param>
/// <param name="Category">
/// An optional category for grouping related entries.
/// </param>
/// <param name="Source">
/// An optional source that produced the entry.
/// </param>
public sealed record RuntimeLogEntry(
    DateTimeOffset Timestamp,
    string Level,
    string Message,
    string? Category = null,
    string? Source = null);
