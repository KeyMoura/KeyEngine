namespace KeyEngine.Web;

/// <summary>
/// Handles an HTTP request and writes its response.
/// </summary>
/// <param name="request">
/// The incoming request.
/// </param>
/// <param name="response">
/// The response to populate.
/// </param>
public delegate void RouteHandler(
    HttpRequestContext request,
    HttpResponseContext response);
