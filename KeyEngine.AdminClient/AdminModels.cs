using System.Text.Json;

namespace KeyEngine.AdminClient;

/// <summary>Represents status data returned by the KeyEngine admin API.</summary>
public sealed record AdminStatus
{
    public string? ApplicationName { get; init; }
    public string? ApplicationVersion { get; init; }
    public string? State { get; init; }
    public long FrameNumber { get; init; }
    public TimeSpan Uptime { get; init; }
    public int PluginCount { get; init; }
    public int CommandCount { get; init; }
    public int EventListenerCount { get; init; }
    public int ActiveTimerCount { get; init; }
    public int ParameterCount { get; init; }
    public int RuntimeLogCount { get; init; }
    public DateTimeOffset LocalTimestamp { get; init; }
    public int ProcessId { get; init; }
    public string? MachineName { get; init; }
    public string? OSDescription { get; init; }
    public string? WorkingDirectory { get; init; }
}

/// <summary>Represents plugin data returned by the KeyEngine admin API.</summary>
public sealed record AdminPlugin
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Version { get; init; }
    public string? State { get; init; }
    public int DependencyCount { get; init; }
    public int LoadBeforeCount { get; init; }
    public int LoadAfterCount { get; init; }
}

/// <summary>Represents parameter data returned by the KeyEngine admin API.</summary>
public sealed record AdminParameter
{
    public string? Key { get; init; }
    public JsonElement Value { get; init; }
    public string? ValueType { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
    public bool IsReadOnly { get; init; }
}

/// <summary>Represents runtime log data returned by the KeyEngine admin API.</summary>
public sealed record AdminLogEntry
{
    public DateTimeOffset Timestamp { get; init; }
    public string? Level { get; init; }
    public string? Message { get; init; }
    public string? Category { get; init; }
    public string? Source { get; init; }
}

/// <summary>Represents route metadata returned by the KeyEngine admin API.</summary>
public sealed record AdminRoute
{
    public string? Method { get; init; }
    public string? Path { get; init; }
    public string? Description { get; init; }
    public bool RequiresAdminToken { get; init; }
    public string? Category { get; init; }
}
