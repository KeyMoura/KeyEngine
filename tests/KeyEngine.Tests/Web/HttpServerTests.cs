using KeyEngine.Web;
using System.Net;
using System.Net.Sockets;

namespace KeyEngine.Tests.Web;

public sealed class HttpServerTests
{
    [Fact]
    public void Register_RouteDispatchesToHandler()
    {
        using HttpServer server = CreateServer();
        server.Register(
            "GET",
            "/health",
            (_, response) => response.Body = "OK");

        HttpResponseContext response = server.Dispatch(
            new HttpRequestContext("GET", "/health"));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("OK", response.Body);
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
}
