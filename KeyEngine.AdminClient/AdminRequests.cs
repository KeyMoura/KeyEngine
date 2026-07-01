namespace KeyEngine.AdminClient;

/// <summary>Represents a request to create or update a string parameter.</summary>
public sealed record ParameterWriteRequest(
    string Key,
    string Value,
    string? Description = null,
    string? Category = null);

/// <summary>Represents a parameter persistence request.</summary>
public sealed record ParameterPersistenceRequest(string Path);
