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

    [Fact]
    public void ParametersRoute_ReturnsRegisteredParameters()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            engine.Parameters.Set(
                "server.port",
                5000,
                "HTTP port",
                "Server");
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext response = server.Dispatch(
                new HttpRequestContext("GET", "/api/parameters"));
            using JsonDocument json = JsonDocument.Parse(response.Body);
            JsonElement parameter = Assert.Single(
                json.RootElement.EnumerateArray());

            Assert.Equal("server.port", parameter.GetProperty("Key").GetString());
            Assert.Equal(5000, parameter.GetProperty("Value").GetInt32());
            Assert.Equal("System.Int32", parameter.GetProperty("ValueType").GetString());
            Assert.Equal("HTTP port", parameter.GetProperty("Description").GetString());
            Assert.Equal("Server", parameter.GetProperty("Category").GetString());
            Assert.False(parameter.GetProperty("IsReadOnly").GetBoolean());
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void ParametersPost_CreatesAndUpdatesParameter()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext created = server.Dispatch(
                CreateParameterRequest("enabled"));
            HttpResponseContext updated = server.Dispatch(
                CreateParameterRequest("disabled"));

            Assert.Equal(200, created.StatusCode);
            Assert.Equal(200, updated.StatusCode);
            Assert.Equal(
                "disabled",
                engine.Parameters.Get<string>("feature.mode"));
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void ParametersPost_ReadOnlyParameter_ReturnsConflict()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            engine.Parameters.Set(
                "feature.mode",
                "locked",
                isReadOnly: true);
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext response = server.Dispatch(
                CreateParameterRequest("changed"));

            Assert.Equal(409, response.StatusCode);
            Assert.Contains("read-only", response.Body);
            Assert.Equal(
                "locked",
                engine.Parameters.Get<string>("feature.mode"));
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void ParametersPost_MissingKey_ReturnsBadRequest()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext response = server.Dispatch(
                new HttpRequestContext(
                    "POST",
                    "/api/parameters",
                    body: """{"value":"enabled"}"""));

            Assert.Equal(400, response.StatusCode);
            Assert.Contains("key", response.Body);
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void ParameterRoute_ReturnsSingleParameterOrNotFound()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            engine.Parameters.Set("server.port", 5000);
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext found = server.Dispatch(
                new HttpRequestContext(
                    "GET",
                    "/api/parameters/server.port"));
            HttpResponseContext missing = server.Dispatch(
                new HttpRequestContext(
                    "GET",
                    "/api/parameters/missing"));
            using JsonDocument json = JsonDocument.Parse(found.Body);

            Assert.Equal(200, found.StatusCode);
            Assert.Equal(
                "server.port",
                json.RootElement.GetProperty("Key").GetString());
            Assert.Equal(5000, json.RootElement.GetProperty("Value").GetInt32());
            Assert.Equal(404, missing.StatusCode);
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void ParameterDelete_RemovesParameterOrReturnsNotFound()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            engine.Parameters.Set("server.port", 5000);
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext removed = server.Dispatch(
                new HttpRequestContext(
                    "DELETE",
                    "/api/parameters/server.port"));
            HttpResponseContext missing = server.Dispatch(
                new HttpRequestContext(
                    "DELETE",
                    "/api/parameters/server.port"));

            Assert.Equal(200, removed.StatusCode);
            Assert.False(engine.Parameters.Contains("server.port"));
            Assert.Equal(404, missing.StatusCode);
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void ParameterDelete_ReadOnlyParameter_ReturnsConflict()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            engine.Parameters.Set(
                "server.port",
                5000,
                isReadOnly: true);
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext response = server.Dispatch(
                new HttpRequestContext(
                    "DELETE",
                    "/api/parameters/server.port"));

            Assert.Equal(409, response.StatusCode);
            Assert.Contains("read-only", response.Body);
            Assert.True(engine.Parameters.Contains("server.port"));
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void ParametersSave_BlankPath_ReturnsBadRequest()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext response = server.Dispatch(
                CreatePersistenceRequest(
                    "/api/parameters/save",
                    " "));

            Assert.Equal(400, response.StatusCode);
            Assert.Contains("path", response.Body);
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void ParametersLoad_BlankPath_ReturnsBadRequest()
    {
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext response = server.Dispatch(
                CreatePersistenceRequest(
                    "/api/parameters/load",
                    " "));

            Assert.Equal(400, response.StatusCode);
            Assert.Contains("path", response.Body);
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void ParametersSave_SavesParametersSuccessfully()
    {
        string path = CreateTemporaryPath();
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            engine.Parameters.Set("feature.mode", "enabled");
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext response = server.Dispatch(
                CreatePersistenceRequest(
                    "/api/parameters/save",
                    path));

            Assert.Equal(200, response.StatusCode);
            Assert.True(File.Exists(path));
            Assert.Contains("Saved", response.Body);
        }
        finally
        {
            engine.Shutdown();
            DeleteTemporaryPath(path);
        }
    }

    [Fact]
    public void ParametersLoad_RestoresSavedParametersSuccessfully()
    {
        string path = CreateTemporaryPath();
        Engine engine = TestEngineFactory.Create();
        using HttpServer server = CreateServer();

        try
        {
            engine.Initialize();
            engine.Parameters.Set(
                "feature.mode",
                "enabled",
                "Feature mode",
                "Features");
            engine.Parameters.Save(path);
            Assert.True(engine.Parameters.Remove("feature.mode"));
            EngineAdminRoutes.Map(server, engine);

            HttpResponseContext response = server.Dispatch(
                CreatePersistenceRequest(
                    "/api/parameters/load",
                    path));

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(
                "enabled",
                engine.Parameters.Get<string>("feature.mode"));

            var parameter = Assert.Single(
                engine.Parameters.GetAll());
            Assert.Equal("Feature mode", parameter.Description);
            Assert.Equal("Features", parameter.Category);
        }
        finally
        {
            engine.Shutdown();
            DeleteTemporaryPath(path);
        }
    }

    private static HttpServer CreateServer()
    {
        return new HttpServer("http://127.0.0.1:8080/");
    }

    private static HttpRequestContext CreateParameterRequest(string value)
    {
        return new HttpRequestContext(
            "POST",
            "/api/parameters",
            body: $$"""
                {
                  "key": "feature.mode",
                  "value": "{{value}}",
                  "description": "Feature mode",
                  "category": "Features"
                }
                """);
    }

    private static HttpRequestContext CreatePersistenceRequest(
        string path,
        string persistencePath)
    {
        string body = JsonSerializer.Serialize(new
        {
            Path = persistencePath
        });

        return new HttpRequestContext(
            "POST",
            path,
            body: body);
    }

    private static string CreateTemporaryPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}.parameters.json");
    }

    private static void DeleteTemporaryPath(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
