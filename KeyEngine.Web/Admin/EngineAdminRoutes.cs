using KeyEngine.Core;
using KeyEngine.Parameters;
using System.Text.Json;

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

        server.MapGet(
            "/api/parameters",
            (_, response) => response.Body = engine.Serializer.Serialize(
                engine.Parameters.GetAll().Select(parameter => new
                {
                    parameter.Key,
                    parameter.Value,
                    ValueType = parameter.ValueType.FullName,
                    parameter.Description,
                    parameter.Category,
                    parameter.IsReadOnly
                }).ToArray()));

        server.MapPost(
            "/api/parameters",
            (request, response) => SetParameter(
                engine,
                request,
                response));
    }

    private static void SetParameter(
        Engine engine,
        HttpRequestContext request,
        HttpResponseContext response)
    {
        try
        {
            ParameterWriteRequest? parameter =
                engine.Serializer.Deserialize<ParameterWriteRequest>(
                    request.Body);

            if (parameter is null ||
                string.IsNullOrWhiteSpace(parameter.Key) ||
                parameter.Value.ValueKind == JsonValueKind.Undefined)
            {
                WriteError(
                    engine,
                    response,
                    400,
                    "The request must include a non-blank key and a value.");
                return;
            }

            SetValue(
                engine.Parameters,
                parameter);

            response.Body = engine.Serializer.Serialize(new
            {
                parameter.Key
            });
        }
        catch (InvalidOperationException exception)
        {
            WriteError(
                engine,
                response,
                409,
                exception.Message);
        }
        catch (Exception exception) when (
            exception is ArgumentException or JsonException)
        {
            WriteError(
                engine,
                response,
                400,
                exception.Message);
        }
    }

    private static void SetValue(
        ParameterManager parameters,
        ParameterWriteRequest parameter)
    {
        object? value = ConvertValue(parameter.Value);

        switch (value)
        {
            case null:
                parameters.Set<object?>(
                    parameter.Key!,
                    null,
                    parameter.Description,
                    parameter.Category,
                    parameter.IsReadOnly);
                break;

            case string text:
                parameters.Set(
                    parameter.Key!,
                    text,
                    parameter.Description,
                    parameter.Category,
                    parameter.IsReadOnly);
                break;

            case long integer:
                parameters.Set(
                    parameter.Key!,
                    integer,
                    parameter.Description,
                    parameter.Category,
                    parameter.IsReadOnly);
                break;

            case double number:
                parameters.Set(
                    parameter.Key!,
                    number,
                    parameter.Description,
                    parameter.Category,
                    parameter.IsReadOnly);
                break;

            case bool boolean:
                parameters.Set(
                    parameter.Key!,
                    boolean,
                    parameter.Description,
                    parameter.Category,
                    parameter.IsReadOnly);
                break;

            case JsonElement json:
                parameters.Set(
                    parameter.Key!,
                    json,
                    parameter.Description,
                    parameter.Category,
                    parameter.IsReadOnly);
                break;
        }
    }

    private static object? ConvertValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number when value.TryGetInt64(out long integer) => integer,
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => value.Clone()
        };
    }

    private static void WriteError(
        Engine engine,
        HttpResponseContext response,
        int statusCode,
        string message)
    {
        response.StatusCode = statusCode;
        response.Body = engine.Serializer.Serialize(new
        {
            Error = message
        });
    }

    private sealed class ParameterWriteRequest
    {
        public string? Key { get; init; }

        public JsonElement Value { get; init; }

        public string? Description { get; init; }

        public string? Category { get; init; }

        public bool IsReadOnly { get; init; }
    }
}
