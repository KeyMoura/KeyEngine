using System.Net.Http.Json;

namespace KeyEngine.AdminApp;

/// <summary>
/// Minimal client for the KeyEngine.Web admin API.
/// </summary>
public sealed class AdminApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new admin API client.
    /// </summary>
    /// <param name="httpClient">
    /// The HTTP client used to call the admin API.
    /// </param>
    public AdminApiClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets the server status.
    /// </summary>
    public async Task<AdminStatus?> GetStatusAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<AdminStatus>(
            "/api/status",
            cancellationToken);
    }

    /// <summary>
    /// Gets plugin diagnostics.
    /// </summary>
    public async Task<IReadOnlyList<AdminPlugin>> GetPluginsAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetListAsync<AdminPlugin>(
            "/api/plugins",
            cancellationToken);
    }

    /// <summary>
    /// Gets runtime parameters.
    /// </summary>
    public async Task<IReadOnlyList<AdminParameter>> GetParametersAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetListAsync<AdminParameter>(
            "/api/parameters",
            cancellationToken);
    }

    /// <summary>
    /// Gets recent runtime log entries.
    /// </summary>
    public async Task<IReadOnlyList<AdminLogEntry>> GetLogsAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetListAsync<AdminLogEntry>(
            "/api/logs",
            cancellationToken);
    }

    /// <summary>
    /// Gets registered web/admin route metadata.
    /// </summary>
    public async Task<IReadOnlyList<AdminRoute>> GetRoutesAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetListAsync<AdminRoute>(
            "/api/routes",
            cancellationToken);
    }

    private async Task<T?> GetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response =
            await _httpClient.GetAsync(
                path,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(
            cancellationToken);
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        T[]? values =
            await GetAsync<T[]>(
                path,
                cancellationToken);

        return values ?? [];
    }
}
