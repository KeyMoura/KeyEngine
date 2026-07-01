using KeyEngine.AdminApp;
using System.Net;
using System.Text;

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

        public string? RequestPath { get; private set; }

        public FakeHandler(string responseBody)
        {
            _responseBody = responseBody;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestPath = request.RequestUri?.AbsolutePath;

            HttpResponseMessage response = new(HttpStatusCode.OK)
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
