using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace KeyEngine.AdminApp;

/// <summary>
/// Minimal client for the KeyEngine.Web admin API.
/// </summary>
public sealed class AdminApiClient
    : IAdminApiClient
{
    private readonly HttpClient _httpClient;

    private const string AdminTokenHeaderName = "X-KeyEngine-Admin-Token";

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

    /// <inheritdoc/>
    public string? AdminToken { get; set; }

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

    /// <inheritdoc/>
    public async Task SetParameterAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using HttpRequestMessage request =
            CreateMutationRequest(
                HttpMethod.Post,
                "/api/parameters",
                new ParameterWriteRequest(
                    key,
                    value));

        await SendMutationAsync(
            request,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DeleteParameterAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using HttpRequestMessage request =
            CreateMutationRequest(
                HttpMethod.Delete,
                $"/api/parameters/{Uri.EscapeDataString(key)}");

        await SendMutationAsync(
            request,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveParametersAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using HttpRequestMessage request =
            CreateMutationRequest(
                HttpMethod.Post,
                "/api/parameters/save",
                new ParameterPersistenceRequest(path));

        await SendMutationAsync(
            request,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task LoadParametersAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using HttpRequestMessage request =
            CreateMutationRequest(
                HttpMethod.Post,
                "/api/parameters/load",
                new ParameterPersistenceRequest(path));

        await SendMutationAsync(
            request,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ClearLogsAsync(
        CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request =
            CreateMutationRequest(
                HttpMethod.Delete,
                "/api/logs");

        await SendMutationAsync(
            request,
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

    private HttpRequestMessage CreateMutationRequest(
        HttpMethod method,
        string path,
        object? body = null)
    {
        HttpRequestMessage request = new(
            method,
            path);

        if (!string.IsNullOrWhiteSpace(AdminToken))
        {
            request.Headers.Add(
                AdminTokenHeaderName,
                AdminToken);
        }

        if (body is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(
                    body,
                    body.GetType()),
                Encoding.UTF8,
                "application/json");
        }

        return request;
    }

    private async Task SendMutationAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string body =
            await response.Content.ReadAsStringAsync(cancellationToken);

        throw new HttpRequestException(
            $"Admin API returned {(int)response.StatusCode} {response.ReasonPhrase}: {ExtractError(body)}",
            null,
            response.StatusCode);
    }

    private static string ExtractError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "No response body.";
        }

        try
        {
            using JsonDocument document =
                JsonDocument.Parse(body);

            if (document.RootElement.TryGetProperty(
                    "Error",
                    out JsonElement error))
            {
                return error.GetString() ?? body;
            }
        }
        catch (JsonException)
        {
        }

        return body;
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
