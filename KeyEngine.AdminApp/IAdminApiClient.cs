namespace KeyEngine.AdminApp;

/// <summary>
/// Reads data from the KeyEngine admin API.
/// </summary>
public interface IAdminApiClient
{
    /// <summary>
    /// Gets or sets the optional admin token sent with mutation requests.
    /// </summary>
    string? AdminToken { get; set; }

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

    /// <summary>
    /// Creates or updates a string runtime parameter.
    /// </summary>
    Task SetParameterAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a runtime parameter.
    /// </summary>
    Task DeleteParameterAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves runtime parameters to a file.
    /// </summary>
    Task SaveParametersAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads runtime parameters from a file.
    /// </summary>
    Task LoadParametersAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears recent runtime log entries.
    /// </summary>
    Task ClearLogsAsync(CancellationToken cancellationToken = default);
}
