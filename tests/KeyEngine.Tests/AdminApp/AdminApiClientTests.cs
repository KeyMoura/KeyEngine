using KeyEngine.AdminClient;
using System.Net;
using System.Text;
using System.Text.Json;

namespace KeyEngine.Tests.AdminApp;

public sealed class AdminApiClientTests
{
    [Fact]
    public async Task GetStatusAsync_CallsStatusEndpoint()
    {
        FakeHandler handler = new(
            """{"ApplicationName":"KeyEngine","State":"Running"}""");
        AdminApiClient client = CreateClient(handler);

        AdminStatus? status =
            await client.GetStatusAsync();

        Assert.Equal("/api/status", handler.RequestPath);
        Assert.Equal("KeyEngine", status?.ApplicationName);
        Assert.Equal("Running", status?.State);
    }

    [Fact]
    public async Task GetPluginsAsync_CallsPluginsEndpoint()
    {
        FakeHandler handler = new(
            """[{"Id":"example","Name":"Example","Version":"1.0.0","State":"Registered"}]""");
        AdminApiClient client = CreateClient(handler);

        IReadOnlyList<AdminPlugin> plugins =
            await client.GetPluginsAsync();

        Assert.Equal("/api/plugins", handler.RequestPath);
        AdminPlugin plugin = Assert.Single(plugins);
        Assert.Equal("example", plugin.Id);
    }

    [Fact]
    public async Task GetParametersAsync_CallsParametersEndpoint()
    {
        FakeHandler handler = new(
            """[{"Key":"server.port","Value":5000,"ValueType":"System.Int32"}]""");
        AdminApiClient client = CreateClient(handler);

        IReadOnlyList<AdminParameter> parameters =
            await client.GetParametersAsync();

        Assert.Equal("/api/parameters", handler.RequestPath);
        AdminParameter parameter = Assert.Single(parameters);
        Assert.Equal("server.port", parameter.Key);
    }

    [Fact]
    public async Task GetLogsAsync_CallsLogsEndpoint()
    {
        FakeHandler handler = new(
            """[{"Level":"Info","Message":"Running"}]""");
        AdminApiClient client = CreateClient(handler);

        IReadOnlyList<AdminLogEntry> logs =
            await client.GetLogsAsync();

        Assert.Equal("/api/logs", handler.RequestPath);
        AdminLogEntry entry = Assert.Single(logs);
        Assert.Equal("Running", entry.Message);
    }

    [Fact]
    public async Task GetRoutesAsync_CallsRoutesEndpoint()
    {
        FakeHandler handler = new(
            """[{"Method":"GET","Path":"/api/status","RequiresAdminToken":false}]""");
        AdminApiClient client = CreateClient(handler);

        IReadOnlyList<AdminRoute> routes =
            await client.GetRoutesAsync();

        Assert.Equal("/api/routes", handler.RequestPath);
        AdminRoute route = Assert.Single(routes);
        Assert.Equal("/api/status", route.Path);
    }

    [Fact]
    public async Task SetParameterAsync_PostsStringValueWithAdminToken()
    {
        FakeHandler handler = new("{}");
        AdminApiClient client = CreateClient(handler);
        client.AdminToken = "secret";

        await client.SetParameterAsync(
            "feature.mode",
            "enabled",
            "Feature mode",
            "Features");

        using JsonDocument json = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal(HttpMethod.Post, handler.RequestMethod);
        Assert.Equal("/api/parameters", handler.RequestPath);
        Assert.Equal("secret", handler.AdminToken);
        Assert.Equal("feature.mode", json.RootElement.GetProperty("Key").GetString());
        Assert.Equal("enabled", json.RootElement.GetProperty("Value").GetString());
        Assert.Equal("Feature mode", json.RootElement.GetProperty("Description").GetString());
        Assert.Equal("Features", json.RootElement.GetProperty("Category").GetString());
    }

    [Fact]
    public async Task DeleteParameterAsync_SendsDeleteWithEscapedKey()
    {
        FakeHandler handler = new("{}");
        AdminApiClient client = CreateClient(handler);

        await client.DeleteParameterAsync("server.port");

        Assert.Equal(HttpMethod.Delete, handler.RequestMethod);
        Assert.Equal("/api/parameters/server.port", handler.RequestPath);
    }

    [Fact]
    public async Task ClearLogsAsync_SendsDeleteLogs()
    {
        FakeHandler handler = new("{}");
        AdminApiClient client = CreateClient(handler);

        await client.ClearLogsAsync();

        Assert.Equal(HttpMethod.Delete, handler.RequestMethod);
        Assert.Equal("/api/logs", handler.RequestPath);
    }

    [Fact]
    public async Task MutationFailure_ThrowsHttpRequestExceptionWithStatusCode()
    {
        FakeHandler handler = new(
            """{"Error":"Admin mutation route requires a valid token."}""",
            HttpStatusCode.Unauthorized);
        AdminApiClient client = CreateClient(handler);

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.ClearLogsAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Contains("valid token", exception.Message);
    }

    private static AdminApiClient CreateClient(FakeHandler handler)
    {
        HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        return new AdminApiClient(httpClient);
    }

    private sealed class FakeHandler
        : HttpMessageHandler
    {
        private readonly string _responseBody;
        private readonly HttpStatusCode _statusCode;

        public HttpMethod? RequestMethod { get; private set; }
        public string? RequestPath { get; private set; }
        public string? RequestBody { get; private set; }
        public string? AdminToken { get; private set; }

        public FakeHandler(
            string responseBody,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseBody = responseBody;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestMethod = request.Method;
            RequestPath = request.RequestUri?.AbsolutePath;
            RequestBody = request.Content?.ReadAsStringAsync(cancellationToken)
                .GetAwaiter()
                .GetResult();
            AdminToken = request.Headers.TryGetValues(
                "X-KeyEngine-Admin-Token",
                out IEnumerable<string>? values)
                ? values.Single()
                : null;

            HttpResponseMessage response = new(_statusCode)
            {
                Content = new StringContent(
                    _responseBody,
                    Encoding.UTF8,
                    "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
