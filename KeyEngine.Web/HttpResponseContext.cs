namespace KeyEngine.Web;

/// <summary>
/// Represents the response produced by an HTTP route.
/// </summary>
public sealed class HttpResponseContext
{
    private string _body = string.Empty;

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
        }
    }
}
