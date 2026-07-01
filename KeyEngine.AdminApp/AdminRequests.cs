namespace KeyEngine.AdminApp;

public sealed record ParameterWriteRequest(
    string Key,
    string Value);

public sealed record ParameterPersistenceRequest(
    string Path);
