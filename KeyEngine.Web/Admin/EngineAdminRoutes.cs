using KeyEngine.Core;
using KeyEngine.Parameters;
using System.Text.Json;

namespace KeyEngine.Web.Admin;

/// <summary>
/// Maps basic engine administration routes onto an HTTP server.
/// </summary>
/// <remarks>
/// Mutation routes require the <c>X-KeyEngine-Admin-Token</c> header only
/// when the <c>admin.token</c> parameter is configured with a non-blank
/// string value. When <c>admin.token</c> is missing or blank, mutation routes
/// remain open for local development foundations.
/// </remarks>
public static class EngineAdminRoutes
{
    private const string AdminTokenParameterKey = "admin.token";

    private const string AdminTokenHeaderName = "X-KeyEngine-Admin-Token";

    /// <summary>
    /// Maps the basic KeyEngine health, status, plugin diagnostic, and
    /// parameter routes.
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
                engine.Parameters.GetAll()
                    .Select(CreateParameterResponse)
                    .ToArray()));

        server.MapGet(
            "/api/parameters/{key}",
            (request, response) => GetParameter(
                engine,
                request,
                response));

        server.MapPost(
            "/api/parameters",
            (request, response) => SetParameter(
                engine,
                request,
                response));

        server.MapPost(
            "/api/parameters/save",
            (request, response) => SaveParameters(
                engine,
                request,
                response));

        server.MapPost(
            "/api/parameters/load",
            (request, response) => LoadParameters(
                engine,
                request,
                response));

        server.Map(
            "DELETE",
            "/api/parameters/{key}",
            (request, response) => DeleteParameter(
                engine,
                request,
                response));
    }

    private static void GetParameter(
        Engine engine,
        HttpRequestContext request,
        HttpResponseContext response)
    {
        string key = request.RouteValues["key"];
        Parameter? parameter = engine.Parameters
            .GetAll()
            .FirstOrDefault(candidate =>
                StringComparer.Ordinal.Equals(
                    candidate.Key,
                    key));

        if (parameter is null)
        {
            WriteError(
                engine,
                response,
                404,
                $"Parameter '{key}' was not found.");
            return;
        }

        response.Body = engine.Serializer.Serialize(
            CreateParameterResponse(parameter));
    }

    private static void DeleteParameter(
        Engine engine,
        HttpRequestContext request,
        HttpResponseContext response)
    {
        if (!AuthorizeMutation(
                engine,
                request,
                response))
        {
            return;
        }

        string key = request.RouteValues["key"];

        try
        {
            if (!engine.Parameters.Remove(key))
            {
                WriteError(
                    engine,
                    response,
                    404,
                    $"Parameter '{key}' was not found.");
                return;
            }

            response.Body = engine.Serializer.Serialize(new
            {
                Key = key,
                Removed = true
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
    }

    private static void SaveParameters(
        Engine engine,
        HttpRequestContext request,
        HttpResponseContext response)
    {
        if (!AuthorizeMutation(
                engine,
                request,
                response))
        {
            return;
        }

        try
        {
            string? path =
                ReadPersistencePath(
                    engine,
                    request,
                    response);

            if (path is null)
            {
                return;
            }

            engine.Parameters.Save(path);

            response.Body = engine.Serializer.Serialize(new
            {
                Saved = true,
                Path = path
            });
        }
        catch (Exception exception) when (
            exception is ArgumentException or
            InvalidOperationException or
            NotSupportedException or
            JsonException or
            FormatException or
            IOException)
        {
            WriteError(
                engine,
                response,
                400,
                exception.Message);
        }
    }

    private static void LoadParameters(
        Engine engine,
        HttpRequestContext request,
        HttpResponseContext response)
    {
        if (!AuthorizeMutation(
                engine,
                request,
                response))
        {
            return;
        }

        try
        {
            string? path =
                ReadPersistencePath(
                    engine,
                    request,
                    response);

            if (path is null)
            {
                return;
            }

            engine.Parameters.Load(path);

            response.Body = engine.Serializer.Serialize(new
            {
                Loaded = true,
                Path = path
            });
        }
        catch (Exception exception) when (
            exception is ArgumentException or
            InvalidOperationException or
            NotSupportedException or
            JsonException or
            FormatException or
            IOException)
        {
            WriteError(
                engine,
                response,
                400,
                exception.Message);
        }
    }

    private static void SetParameter(
        Engine engine,
        HttpRequestContext request,
        HttpResponseContext response)
    {
        if (!AuthorizeMutation(
                engine,
                request,
                response))
        {
            return;
        }

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

    private static bool AuthorizeMutation(
        Engine engine,
        HttpRequestContext request,
        HttpResponseContext response)
    {
        if (!TryGetConfiguredAdminToken(
                engine,
                out string? configuredToken,
                out string? error))
        {
            WriteError(
                engine,
                response,
                401,
                error ?? "Admin mutation route is not authorized.");
            return false;
        }

        if (configuredToken is null)
        {
            return true;
        }

        if (TryGetHeader(
                request,
                AdminTokenHeaderName,
                out string? suppliedToken) &&
            StringComparer.Ordinal.Equals(
                configuredToken,
                suppliedToken))
        {
            return true;
        }

        WriteError(
            engine,
            response,
            401,
            $"Admin mutation route requires a valid {AdminTokenHeaderName} header.");
        return false;
    }

    private static bool TryGetConfiguredAdminToken(
        Engine engine,
        out string? token,
        out string? error)
    {
        token = null;
        error = null;

        if (!engine.Parameters.Contains(AdminTokenParameterKey))
        {
            return true;
        }

        if (!engine.Parameters.TryGet(
                AdminTokenParameterKey,
                out string configuredToken))
        {
            error = $"Parameter '{AdminTokenParameterKey}' must be a string when configured.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(configuredToken))
        {
            return true;
        }

        token = configuredToken;
        return true;
    }

    private static bool TryGetHeader(
        HttpRequestContext request,
        string name,
        out string? value)
    {
        if (request.Headers.TryGetValue(
                name,
                out value))
        {
            return true;
        }

        foreach ((string key, string candidate) in request.Headers)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(
                    key,
                    name))
            {
                value = candidate;
                return true;
            }
        }

        value = null;
        return false;
    }

    private static string? ReadPersistencePath(
        Engine engine,
        HttpRequestContext request,
        HttpResponseContext response)
    {
        ParameterPersistenceRequest? persistence =
            engine.Serializer.Deserialize<ParameterPersistenceRequest>(
                request.Body);

        if (persistence is null ||
            string.IsNullOrWhiteSpace(persistence.Path))
        {
            WriteError(
                engine,
                response,
                400,
                "The request must include a non-blank path.");
            return null;
        }

        return persistence.Path;
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

    private static ParameterResponse CreateParameterResponse(
        Parameter parameter)
    {
        return new ParameterResponse
        {
            Key = parameter.Key,
            Value = parameter.Value,
            ValueType = parameter.ValueType.FullName,
            Description = parameter.Description,
            Category = parameter.Category,
            IsReadOnly = parameter.IsReadOnly
        };
    }

    private sealed class ParameterWriteRequest
    {
        public string? Key { get; init; }

        public JsonElement Value { get; init; }

        public string? Description { get; init; }

        public string? Category { get; init; }

        public bool IsReadOnly { get; init; }
    }

    private sealed class ParameterPersistenceRequest
    {
        public string? Path { get; init; }
    }

    private sealed class ParameterResponse
    {
        public required string Key { get; init; }

        public object? Value { get; init; }

        public string? ValueType { get; init; }

        public string? Description { get; init; }

        public string? Category { get; init; }

        public bool IsReadOnly { get; init; }
    }
}
