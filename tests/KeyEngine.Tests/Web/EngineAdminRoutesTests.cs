using KeyEngine.Core;
using KeyEngine.Web;
using KeyEngine.Web.Admin;
using System.Text.Json;

namespace KeyEngine.Tests.Web;

public sealed class EngineAdminRoutesTests
{
    [Fact]
    public void HealthRoute_ReturnsOk()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext response = server.Dispatch(
                new HttpRequestContext("GET", "/api/health"));

            Assert.Equal(200, response.StatusCode);
            Assert.Equal("OK", response.Body);
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void StatusRoute_ReturnsPublicEngineDiagnostics()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext response = server.Dispatch(
                new HttpRequestContext("GET", "/api/status"));
            using JsonDocument json = JsonDocument.Parse(response.Body);
            JsonElement root = json.RootElement;

            Assert.Equal("Running", root.GetProperty("State").GetString());
            Assert.True(root.TryGetProperty("FrameNumber", out _));
            Assert.True(root.TryGetProperty("Uptime", out _));
            Assert.True(root.TryGetProperty("PluginCount", out _));
            Assert.True(root.TryGetProperty("CommandCount", out _));
            Assert.True(root.TryGetProperty("EventListenerCount", out _));
            Assert.True(root.TryGetProperty("ActiveTimerCount", out _));
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void PluginsRoute_ReturnsPluginDiagnosticsArray()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext response = server.Dispatch(
                new HttpRequestContext("GET", "/api/plugins"));
            using JsonDocument json = JsonDocument.Parse(response.Body);

            Assert.Equal(
                JsonValueKind.Array,
                json.RootElement.ValueKind);
        }
        finally
        {
            engine.Shutdown();
        }
    }

    private static HttpServer CreateServer()
    {
        return new HttpServer("http://127.0.0.1:8080/");
    }
}
