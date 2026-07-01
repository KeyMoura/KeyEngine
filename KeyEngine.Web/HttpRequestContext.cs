namespace KeyEngine.Web;

/// <summary>
/// Represents the safe request data supplied to an HTTP route.
/// </summary>
public sealed class HttpRequestContext
{
    /// <summary>
    /// Gets the normalized HTTP method.
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// Gets the absolute request path.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the request query values.
    /// </summary>
    public IReadOnlyDictionary<string, string> Query { get; }

    /// <summary>
    /// Gets the request headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the request body text.
    /// </summary>
    public string Body { get; }

    internal HttpRequestContext(
        string method,
        string path,
        IReadOnlyDictionary<string, string>? query = null,
        IReadOnlyDictionary<string, string>? headers = null,
        string body = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Method = method.ToUpperInvariant();
        Path = path;
        Query = query ?? new Dictionary<string, string>();
        Headers = headers ?? new Dictionary<string, string>();
        Body = body ?? string.Empty;
    }
}
