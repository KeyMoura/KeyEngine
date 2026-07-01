namespace KeyEngine.AdminApp;

/// <summary>
/// Interactive console shell for inspecting a KeyEngine admin API server.
/// </summary>
public sealed class AdminShell
{
    private readonly Uri _baseUri;
    private readonly IAdminApiClient _client;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly TextWriter _error;

    /// <summary>
    /// Initializes a new admin shell.
    /// </summary>
    public AdminShell(
        Uri baseUri,
        IAdminApiClient client,
        TextReader input,
        TextWriter output,
        TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        _baseUri = baseUri;
        _client = client;
        _input = input;
        _output = output;
        _error = error;
    }

    /// <summary>
    /// Runs the interactive shell until the user exits.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        PrintBanner();
        await ExecuteCommandAsync(
            "status",
            cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            _output.Write("> ");

            string? command =
                await _input.ReadLineAsync(cancellationToken);

            if (command is null)
            {
                return;
            }

            bool shouldContinue =
                await ExecuteCommandAsync(
                    command,
                    cancellationToken);

            if (!shouldContinue)
            {
                return;
            }
        }
    }

    /// <summary>
    /// Executes one shell command.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> to continue the shell loop; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public async Task<bool> ExecuteCommandAsync(
        string command,
        CancellationToken cancellationToken = default)
    {
        string normalized =
            command.Trim().ToLowerInvariant();

        try
        {
            switch (normalized)
            {
                case "":
                    return true;

                case "help":
                    PrintHelp();
                    return true;

                case "status":
                    PrintStatus(await _client.GetStatusAsync(cancellationToken));
                    return true;

                case "plugins":
                    PrintPlugins(await _client.GetPluginsAsync(cancellationToken));
                    return true;

                case "parameters":
                    PrintParameters(await _client.GetParametersAsync(cancellationToken));
                    return true;

                case "logs":
                    PrintLogs(await _client.GetLogsAsync(cancellationToken));
                    return true;

                case "routes":
                    PrintRoutes(await _client.GetRoutesAsync(cancellationToken));
                    return true;

                case "clear":
                    Console.Clear();
                    return true;

                case "exit":
                case "quit":
                    return false;

                default:
                    _output.WriteLine(
                        $"Unknown command '{command}'. Type 'help' for available commands.");
                    return true;
            }
        }
        catch (HttpRequestException exception)
        {
            _error.WriteLine(
                $"Unable to reach KeyEngine admin API at {_baseUri}: {exception.Message}");
            return true;
        }
        catch (TaskCanceledException exception)
        {
            _error.WriteLine(
                $"Admin API request timed out or was cancelled: {exception.Message}");
            return true;
        }
    }

    private void PrintBanner()
    {
        _output.WriteLine("KeyEngine Admin Shell");
        _output.WriteLine($"Server: {_baseUri}");
        _output.WriteLine("Type 'help' for commands. Type 'exit' or 'quit' to close.");
        _output.WriteLine();
    }

    private void PrintHelp()
    {
        _output.WriteLine("Commands:");
        _output.WriteLine("  help        Show available commands.");
        _output.WriteLine("  status      Show server status.");
        _output.WriteLine("  plugins     Show plugins.");
        _output.WriteLine("  parameters  Show runtime parameters.");
        _output.WriteLine("  logs        Show recent runtime logs.");
        _output.WriteLine("  routes      Show registered web/admin routes.");
        _output.WriteLine("  clear       Clear the console.");
        _output.WriteLine("  exit        Close the shell.");
        _output.WriteLine("  quit        Close the shell.");
    }

    private void PrintStatus(AdminStatus? status)
    {
        if (status is null)
        {
            _output.WriteLine("Status: unavailable");
            return;
        }

        _output.WriteLine(
            $"Application: {status.ApplicationName} {status.ApplicationVersion}");
        _output.WriteLine($"State: {status.State}");
        _output.WriteLine($"Frame: {status.FrameNumber}");
        _output.WriteLine($"Uptime: {status.Uptime}");
        _output.WriteLine(
            $"Plugins: {status.PluginCount}, Commands: {status.CommandCount}, Event listeners: {status.EventListenerCount}, Active timers: {status.ActiveTimerCount}");
        _output.WriteLine(
            $"Parameters: {status.ParameterCount}, Runtime logs: {status.RuntimeLogCount}");
        _output.WriteLine(
            $"Process: {status.ProcessId} on {status.MachineName}");
        _output.WriteLine($"OS: {status.OSDescription}");
        _output.WriteLine($"Working directory: {status.WorkingDirectory}");
    }

    private void PrintPlugins(IReadOnlyList<AdminPlugin> plugins)
    {
        _output.WriteLine($"Plugins ({plugins.Count})");

        foreach (AdminPlugin plugin in plugins)
        {
            _output.WriteLine(
                $"- {plugin.Id} {plugin.Version} [{plugin.State}] {plugin.Name}");
        }
    }

    private void PrintParameters(IReadOnlyList<AdminParameter> parameters)
    {
        _output.WriteLine($"Parameters ({parameters.Count})");

        foreach (AdminParameter parameter in parameters)
        {
            string readOnly = parameter.IsReadOnly
                ? " read-only"
                : string.Empty;

            _output.WriteLine(
                $"- {parameter.Key} ({parameter.ValueType}){readOnly}");
        }
    }

    private void PrintLogs(IReadOnlyList<AdminLogEntry> logs)
    {
        _output.WriteLine($"Recent logs ({logs.Count})");

        foreach (AdminLogEntry entry in logs.Take(10))
        {
            _output.WriteLine(
                $"- {entry.Timestamp:u} [{entry.Level}] {entry.Message}");
        }
    }

    private void PrintRoutes(IReadOnlyList<AdminRoute> routes)
    {
        _output.WriteLine($"Routes ({routes.Count})");

        foreach (AdminRoute route in routes)
        {
            string protectedMarker = route.RequiresAdminToken
                ? " protected"
                : string.Empty;

            _output.WriteLine(
                $"- {route.Method} {route.Path}{protectedMarker}");
        }
    }
}
