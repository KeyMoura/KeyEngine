namespace KeyEngine.AdminApp;

/// <summary>
/// Reads data from the KeyEngine admin API.
/// </summary>
public interface IAdminApiClient
{
    /// <summary>
    /// Gets the server status.
    /// </summary>
    Task<AdminStatus?> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plugin diagnostics.
    /// </summary>
    Task<IReadOnlyList<AdminPlugin>> GetPluginsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets runtime parameters.
    /// </summary>
    Task<IReadOnlyList<AdminParameter>> GetParametersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent runtime log entries.
    /// </summary>
    Task<IReadOnlyList<AdminLogEntry>> GetLogsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets registered web/admin route metadata.
    /// </summary>
    Task<IReadOnlyList<AdminRoute>> GetRoutesAsync(CancellationToken cancellationToken = default);
}
