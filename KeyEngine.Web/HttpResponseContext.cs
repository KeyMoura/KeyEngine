namespace KeyEngine.Web;

/// <summary>
/// Represents the response produced by an HTTP route.
/// </summary>
public sealed class HttpResponseContext
{
    private string _body = string.Empty;
    private byte[]? _bodyBytes;

    internal string ContentType { get; private set; } = "text/plain; charset=utf-8";

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// Gets or sets the UTF-8 response body text.
    /// </summary>
    public string Body
    {
        get => _body;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _body = value;
            _bodyBytes = null;
            ContentType = "text/plain; charset=utf-8";
        }
    }

    internal byte[] GetBodyBytes()
    {
        return _bodyBytes ?? System.Text.Encoding.UTF8.GetBytes(_body);
    }

    internal void SetBody(
        byte[] body,
        string contentType)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        _bodyBytes = body;
        _body = System.Text.Encoding.UTF8.GetString(body);
        ContentType = contentType;
    }
}
