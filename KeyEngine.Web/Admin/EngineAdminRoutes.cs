using KeyEngine.Core;

namespace KeyEngine.Web.Admin;

/// <summary>
/// Maps read-only engine status routes onto an HTTP server.
/// </summary>
public static class EngineAdminRoutes
{
    /// <summary>
    /// Maps the basic KeyEngine health, status, and plugin diagnostic routes.
    /// </summary>
    /// <param name="server">
    /// The server that receives the routes.
    /// </param>
    /// <param name="engine">
    /// The engine that provides public diagnostic information.
    /// </param>
    public static void Map(
        HttpServer server,
        Engine engine)
    {
        ArgumentNullException.ThrowIfNull(server);
        ArgumentNullException.ThrowIfNull(engine);

        server.MapGet(
            "/api/health",
            (_, response) => response.Body = "OK");

        server.MapGet(
            "/api/status",
            (_, response) =>
            {
                var diagnostics = engine.Diagnostics;

                response.Body = engine.Serializer.Serialize(new
                {
                    State = diagnostics.State.ToString(),
                    diagnostics.FrameNumber,
                    diagnostics.Uptime,
                    diagnostics.PluginCount,
                    diagnostics.CommandCount,
                    diagnostics.EventListenerCount,
                    diagnostics.ActiveTimerCount
                });
            });

        server.MapGet(
            "/api/plugins",
            (_, response) => response.Body = engine.Serializer.Serialize(
                engine.Diagnostics.Plugins.Select(plugin => new
                {
                    plugin.Id,
                    plugin.Name,
                    Version = plugin.Version.ToString(),
                    State = plugin.State.ToString(),
                    plugin.DependencyCount,
                    plugin.LoadBeforeCount,
                    plugin.LoadAfterCount
                }).ToArray()));
    }
}
