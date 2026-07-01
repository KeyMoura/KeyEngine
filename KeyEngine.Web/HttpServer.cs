using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace KeyEngine.Web;

/// <summary>
/// Hosts a small exact-match HTTP route table using <see cref="HttpListener"/>.
/// </summary>
/// <remarks>
/// Route handlers execute synchronously on the server's background accept
/// loop. Register routes before starting the server. Instances are not
/// guaranteed to be thread-safe.
/// </remarks>
public sealed class HttpServer
    : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly Dictionary<(string Method, string Path), RouteHandler>
        _routes = [];

    private CancellationTokenSource? _cancellation;
    private Task? _listenTask;
    private bool _isDisposed;

    /// <summary>
    /// Gets the HTTP listener prefix.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// Gets a value indicating whether the server is listening.
    /// </summary>
    public bool IsRunning => _listener.IsListening;

    /// <summary>
    /// Initializes a new HTTP server.
    /// </summary>
    /// <param name="prefix">
    /// An absolute HTTP listener prefix ending with a slash, such as
    /// <c>http://127.0.0.1:8080/</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the prefix is not an absolute HTTP URI ending with a slash.
    /// </exception>
    public HttpServer(string prefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        if (!Uri.TryCreate(
                prefix,
                UriKind.Absolute,
                out Uri? uri) ||
            uri.Scheme != Uri.UriSchemeHttp ||
            !prefix.EndsWith(
                "/",
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The server prefix must be an absolute HTTP URI ending with a slash.",
                nameof(prefix));
        }

        Prefix = prefix;
        _listener.Prefixes.Add(prefix);
    }

    /// <summary>
    /// Registers an exact HTTP method and path route.
    /// </summary>
    /// <param name="method">
    /// The HTTP method.
    /// </param>
    /// <param name="path">
    /// The exact absolute path beginning with <c>/</c>.
    /// </param>
    /// <param name="handler">
    /// The synchronous route handler.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server is running or the route is already registered.
    /// </exception>
    public void Register(
        string method,
        string path,
        RouteHandler handler)
    {
        Map(
            method,
            path,
            handler);
    }

    /// <summary>
    /// Maps an exact HTTP method and path route.
    /// </summary>
    /// <param name="method">
    /// The HTTP method.
    /// </param>
    /// <param name="path">
    /// The exact absolute path beginning with <c>/</c>.
    /// </param>
    /// <param name="handler">
    /// The synchronous route handler.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server is running or the route is already registered.
    /// </exception>
    public void Map(
        string method,
        string path,
        RouteHandler handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(handler);

        ThrowIfDisposed();

        if (IsRunning)
        {
            throw new InvalidOperationException(
                "Routes cannot be registered while the server is running.");
        }

        if (!path.StartsWith(
                "/",
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The route path must begin with '/'.",
                nameof(path));
        }

        (string Method, string Path) route =
            (method.ToUpperInvariant(), path);

        if (!_routes.TryAdd(
                route,
                handler))
        {
            throw new InvalidOperationException(
                $"The route '{route.Method} {route.Path}' is already registered.");
        }
    }

    /// <summary>
    /// Maps an exact GET route.
    /// </summary>
    /// <param name="path">
    /// The exact absolute path beginning with <c>/</c>.
    /// </param>
    /// <param name="handler">
    /// The synchronous route handler.
    /// </param>
    public void MapGet(
        string path,
        RouteHandler handler)
    {
        Map(
            "GET",
            path,
            handler);
    }

    /// <summary>
    /// Maps an exact POST route.
    /// </summary>
    /// <param name="path">
    /// The exact absolute path beginning with <c>/</c>.
    /// </param>
    /// <param name="handler">
    /// The synchronous route handler.
    /// </param>
    public void MapPost(
        string path,
        RouteHandler handler)
    {
        Map(
            "POST",
            path,
            handler);
    }

    /// <summary>
    /// Starts listening for HTTP requests without blocking the caller.
    /// </summary>
    public void Start()
    {
        ThrowIfDisposed();

        if (IsRunning)
        {
            return;
        }

        _listener.Start();
        _cancellation = new CancellationTokenSource();
        _listenTask = Task.Run(() => ListenAsync(_cancellation.Token));
    }

    /// <summary>
    /// Stops accepting requests and waits for the accept loop to finish.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        _cancellation!.Cancel();
        _listener.Stop();
        _listenTask!.GetAwaiter().GetResult();

        _listenTask = null;
        _cancellation.Dispose();
        _cancellation = null;
    }

    /// <summary>
    /// Stops the server and releases its listener resources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Stop();
        _listener.Close();
        _isDisposed = true;
    }

    internal HttpResponseContext Dispatch(HttpRequestContext request)
    {
        ArgumentNullException.ThrowIfNull(request);

        HttpResponseContext response = new();

        if (!_routes.TryGetValue(
                (request.Method, request.Path),
                out RouteHandler? handler))
        {
            response.StatusCode = 404;
            response.Body = "Not Found";
            return response;
        }

        handler(
            request,
            response);

        return response;
    }

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            HttpListenerContext context;

            try
            {
                context = await _listener
                    .GetContextAsync()
                    .WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (HttpListenerException) when (!IsRunning)
            {
                break;
            }

            Process(context);
        }
    }

    private void Process(HttpListenerContext context)
    {
        HttpResponseContext response;

        try
        {
            response = Dispatch(CreateRequest(context.Request));
        }
        catch
        {
            response = new HttpResponseContext
            {
                StatusCode = 500,
                Body = "Internal Server Error"
            };
        }

        byte[] body = Encoding.UTF8.GetBytes(response.Body);

        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.ContentEncoding = Encoding.UTF8;
        context.Response.ContentLength64 = body.Length;

        try
        {
            context.Response.OutputStream.Write(body);
        }
        finally
        {
            context.Response.Close();
        }
    }

    private static HttpRequestContext CreateRequest(HttpListenerRequest request)
    {
        return new HttpRequestContext(
            request.HttpMethod,
            request.Url?.AbsolutePath ?? "/",
            ToDictionary(request.QueryString),
            ToDictionary(request.Headers));
    }

    private static IReadOnlyDictionary<string, string> ToDictionary(
        NameValueCollection values)
    {
        Dictionary<string, string> result =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (string? key in values.AllKeys)
        {
            if (key is not null)
            {
                result[key] = values[key] ?? string.Empty;
            }
        }

        return result;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            _isDisposed,
            this);
    }
}
