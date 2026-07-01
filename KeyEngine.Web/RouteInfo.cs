namespace KeyEngine.Web;

/// <summary>
/// Describes a registered HTTP route without exposing its handler.
/// </summary>
/// <param name="Method">
/// The normalized HTTP method.
/// </param>
/// <param name="Path">
/// The route path.
/// </param>
/// <param name="Description">
/// An optional route description.
/// </param>
/// <param name="RequiresAdminToken">
/// Whether the route is expected to require the admin token.
/// </param>
/// <param name="Category">
/// An optional category for grouping routes.
/// </param>
public sealed record RouteInfo(
    string Method,
    string Path,
    string? Description = null,
    bool RequiresAdminToken = false,
    string? Category = null);
