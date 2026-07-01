using KeyEngine.Abstractions;
using KeyEngine.Configuration;
using KeyEngine.Plugins;
using KeyEngine.Web;
using System.Net;
using System.Net.Sockets;

namespace KeyEngine.Tests.Web;

public sealed class HttpServerTests
{
    [Fact]
    public void MapGet_RouteDispatchesToHandler()
    {
        using HttpServer server = CreateServer();
        server.MapGet(
            "/health",
            (_, response) => response.Body = "OK");

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext("GET", "/health"));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("OK", response.Body);
    }

    [Fact]
    public void MapGet_DuplicateRoute_ThrowsClearException()
    {
        using HttpServer server = CreateServer();
        server.MapGet(
            "/health",
            (_, response) => response.Body = "first");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => server.MapGet(
                "/health",
                (_, response) => response.Body = "second"));

        Assert.Contains("GET /health", exception.Message);
    }

    [Fact]
    public void ParameterizedRoute_CapturesPathSegment()
    {
        using HttpServer server = CreateServer();
        string? captured = null;
        server.MapGet(
            "/api/parameters/{key}",
            (request, response) =>
            {
                captured = request.RouteValues["key"];
                response.Body = "matched";
            });

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext(
                "GET",
                "/api/parameters/server.port"));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("matched", response.Body);
        Assert.Equal("server.port", captured);
    }

    [Fact]
    public void ExactRoute_WinsOverParameterizedRoute()
    {
        using HttpServer server = CreateServer();
        server.MapGet(
            "/api/parameters/{key}",
            (_, response) => response.Body = "parameter");
        server.MapGet(
            "/api/parameters/all",
            (_, response) => response.Body = "exact");

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext(
                "GET",
                "/api/parameters/all"));

        Assert.Equal("exact", response.Body);
    }

    [Theory]
    [InlineData("POST", "/health")]
    [InlineData("GET", "/Health")]
    [InlineData("GET", "/health/")]
    public void Dispatch_NonExactMethodOrPath_ReturnsNotFound(
        string method,
        string path)
    {
        using HttpServer server = CreateServer();
        server.Register(
            "GET",
            "/health",
            (_, response) => response.Body = "OK");

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext(method, path));

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("Not Found", response.Body);
    }

    [Fact]
    public void Dispatch_MissingRoute_ReturnsNotFound()
    {
        using HttpServer server = CreateServer();

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext("GET", "/missing"));

        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public void Handler_CanSetStatusCodeAndBody()
    {
        using HttpServer server = CreateServer();
        server.Register(
            "GET",
            "/",
            (_, response) =>
            {
                response.StatusCode = 202;
                response.Body = "KeyEngine Web";
            });

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext("GET", "/"));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal("KeyEngine Web", response.Body);
    }

    [Fact]
    public void StaticFiles_ExistingFile_IsServed()
    {
        using TemporaryStaticSite site = new();
        site.WriteFile("hello.txt", "Hello from KeyEngine");
        using HttpServer server = CreateServer();
        server.MapStaticFiles("/", site.StaticRoot);

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext("GET", "/hello.txt"));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Hello from KeyEngine", response.Body);
        Assert.Equal("text/plain; charset=utf-8", response.ContentType);
    }

    [Fact]
    public void StaticFiles_RootRequest_ServesIndexHtml()
    {
        using TemporaryStaticSite site = new();
        site.WriteFile("index.html", "<h1>KeyEngine</h1>");
        using HttpServer server = CreateServer();
        server.MapStaticFiles("/", site.StaticRoot);

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext("GET", "/"));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("<h1>KeyEngine</h1>", response.Body);
        Assert.Equal("text/html; charset=utf-8", response.ContentType);
    }

    [Fact]
    public void StaticFiles_MissingFile_ReturnsNotFound()
    {
        using TemporaryStaticSite site = new();
        using HttpServer server = CreateServer();
        server.MapStaticFiles("/", site.StaticRoot);

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext("GET", "/missing.txt"));

        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public void StaticFiles_PathTraversal_IsRejected()
    {
        using TemporaryStaticSite site = new();
        site.WriteOutsideFile("secret.txt", "secret");
        using HttpServer server = CreateServer();
        server.MapStaticFiles("/", site.StaticRoot);

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext("GET", "/%2e%2e/secret.txt"));

        Assert.Equal(404, response.StatusCode);
        Assert.NotEqual("secret", response.Body);
    }

    [Fact]
    public void StaticFiles_ExactApiRoute_TakesPrecedence()
    {
        using TemporaryStaticSite site = new();
        site.WriteFile("index.html", "static");
        using HttpServer server = CreateServer();
        server.MapStaticFiles("/", site.StaticRoot);
        server.MapGet(
            "/",
            (_, response) => response.Body = "API route");

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext("GET", "/"));

        Assert.Equal("API route", response.Body);
    }

    [Fact]
    public void StartAndStop_UpdatesRunningState()
    {
        int port = GetAvailablePort();

        using HttpServer server = new(
            $"http://127.0.0.1:{port}/");

        server.Start();

        Assert.True(server.IsRunning);

        server.Stop();

        Assert.False(server.IsRunning);
    }

    [Fact]
    public void Constructor_MultiplePrefixes_PreservesEveryPrefix()
    {
        using HttpServer server = new(
            "http://localhost:5000/",
            "http://127.0.0.1:5000/");

        Assert.Equal("http://localhost:5000/", server.Prefix);
        Assert.Equal(
            [
                "http://localhost:5000/",
                "http://127.0.0.1:5000/"
            ],
            server.Prefixes);
    }

    [Fact]
    public void PluginRegisteredServer_IsInjectedIntoSystemForRouteRegistration()
    {
        using HttpServer server = CreateServer();
        PluginContext context = new()
        {
            Manifest = new PluginManifest
            {
                Id = "web-test",
                Main = typeof(InjectedRouteSystem).FullName!,
                Name = "Web Test"
            },
            PluginDirectory = string.Empty,
            Configuration = new StubConfigurationManager()
        };

        PluginBuilder builder = new(context);
        builder.AddSingleton(server);
        builder.AddSystem<InjectedRouteSystem>();

        InjectedRouteSystem system =
            (InjectedRouteSystem)builder.Services
                .Build()
                .GetService(typeof(InjectedRouteSystem));

        system.Start();

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext("GET", "/plugin"));

        Assert.Equal("plugin route", response.Body);
    }

    private static HttpServer CreateServer()
    {
        return new HttpServer("http://127.0.0.1:8080/");
    }

    private static int GetAvailablePort()
    {
        TcpListener listener = new(
            IPAddress.Loopback,
            0);

        listener.Start();

        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    private sealed class InjectedRouteSystem
        : IEngineSystem
    {
        private readonly HttpServer _server;

        public InjectedRouteSystem(HttpServer server)
        {
            _server = server;
        }

        public void Start()
        {
            _server.MapGet(
                "/plugin",
                (_, response) => response.Body = "plugin route");
        }
    }

    private sealed class StubConfigurationManager
        : IConfigurationManager
    {
        public T Get<T>()
            where T : class, new()
        {
            return new T();
        }

        public void Save()
        {
        }
    }

    private sealed class TemporaryStaticSite
        : IDisposable
    {
        private readonly string _root = Path.Combine(
            Path.GetTempPath(),
            $"KeyEngine.Tests.{Guid.NewGuid():N}");

        public string StaticRoot => Path.Combine(_root, "wwwroot");

        public TemporaryStaticSite()
        {
            Directory.CreateDirectory(StaticRoot);
        }

        public void WriteFile(
            string relativePath,
            string contents)
        {
            File.WriteAllText(
                Path.Combine(StaticRoot, relativePath),
                contents);
        }

        public void WriteOutsideFile(
            string fileName,
            string contents)
        {
            File.WriteAllText(
                Path.Combine(_root, fileName),
                contents);
        }

        public void Dispose()
        {
            Directory.Delete(
                _root,
                true);
        }
    }
}
