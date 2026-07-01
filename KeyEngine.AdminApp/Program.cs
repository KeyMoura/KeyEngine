using KeyEngine.AdminApp;

Uri baseUri = GetBaseUri(args);
using HttpClient httpClient = new()
{
    BaseAddress = baseUri
};

AdminApiClient client = new(httpClient);

try
{
    AdminStatus? status =
        await client.GetStatusAsync();

    IReadOnlyList<AdminPlugin> plugins =
        await client.GetPluginsAsync();

    IReadOnlyList<AdminParameter> parameters =
        await client.GetParametersAsync();

    IReadOnlyList<AdminLogEntry> logs =
        await client.GetLogsAsync();

    IReadOnlyList<AdminRoute> routes =
        await client.GetRoutesAsync();

    PrintStatus(
        baseUri,
        status);

    PrintPlugins(plugins);
    PrintParameters(parameters);
    PrintLogs(logs);
    PrintRoutes(routes);
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine(
        $"Failed to connect to KeyEngine admin API at {baseUri}: {exception.Message}");
    Environment.ExitCode = 1;
}

static Uri GetBaseUri(string[] args)
{
    string value = args.Length > 0
        ? args[0]
        : "http://localhost:5000";

    if (!Uri.TryCreate(
            value,
            UriKind.Absolute,
            out Uri? uri))
    {
        throw new ArgumentException(
            $"'{value}' is not a valid absolute URL.");
    }

    return uri;
}

static void PrintStatus(
    Uri baseUri,
    AdminStatus? status)
{
    Console.WriteLine($"KeyEngine Admin API: {baseUri}");

    if (status is null)
    {
        Console.WriteLine("Status: unavailable");
        return;
    }

    Console.WriteLine(
        $"Application: {status.ApplicationName} {status.ApplicationVersion}");
    Console.WriteLine($"State: {status.State}");
    Console.WriteLine($"Frame: {status.FrameNumber}");
    Console.WriteLine($"Uptime: {status.Uptime}");
    Console.WriteLine(
        $"Plugins: {status.PluginCount}, Commands: {status.CommandCount}, Event listeners: {status.EventListenerCount}, Active timers: {status.ActiveTimerCount}");
    Console.WriteLine(
        $"Parameters: {status.ParameterCount}, Runtime logs: {status.RuntimeLogCount}");
    Console.WriteLine(
        $"Process: {status.ProcessId} on {status.MachineName}");
    Console.WriteLine($"OS: {status.OSDescription}");
    Console.WriteLine($"Working directory: {status.WorkingDirectory}");
}

static void PrintPlugins(IReadOnlyList<AdminPlugin> plugins)
{
    Console.WriteLine();
    Console.WriteLine($"Plugins ({plugins.Count})");

    foreach (AdminPlugin plugin in plugins)
    {
        Console.WriteLine(
            $"- {plugin.Id} {plugin.Version} [{plugin.State}] {plugin.Name}");
    }
}

static void PrintParameters(IReadOnlyList<AdminParameter> parameters)
{
    Console.WriteLine();
    Console.WriteLine($"Parameters ({parameters.Count})");

    foreach (AdminParameter parameter in parameters)
    {
        string readOnly = parameter.IsReadOnly
            ? " read-only"
            : string.Empty;

        Console.WriteLine(
            $"- {parameter.Key} ({parameter.ValueType}){readOnly}");
    }
}

static void PrintLogs(IReadOnlyList<AdminLogEntry> logs)
{
    Console.WriteLine();
    Console.WriteLine($"Recent logs ({logs.Count})");

    foreach (AdminLogEntry entry in logs.Take(10))
    {
        Console.WriteLine(
            $"- {entry.Timestamp:u} [{entry.Level}] {entry.Message}");
    }
}

static void PrintRoutes(IReadOnlyList<AdminRoute> routes)
{
    Console.WriteLine();
    Console.WriteLine($"Routes ({routes.Count})");

    foreach (AdminRoute route in routes)
    {
        string protectedMarker = route.RequiresAdminToken
            ? " protected"
            : string.Empty;

        Console.WriteLine(
            $"- {route.Method} {route.Path}{protectedMarker}");
    }
}
