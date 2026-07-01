using KeyEngine.AdminApp;

namespace KeyEngine.Tests.AdminApp;

public sealed class AdminShellTests
{
    [Fact]
    public async Task ExecuteCommandAsync_Help_PrintsCommandsAndContinues()
    {
        StringWriter output = new();
        AdminShell shell = CreateShell(
            output: output);

        bool shouldContinue =
            await shell.ExecuteCommandAsync("help");

        Assert.True(shouldContinue);
        Assert.Contains("status", output.ToString());
        Assert.Contains("plugins", output.ToString());
        Assert.Contains("exit", output.ToString());
    }

    [Fact]
    public async Task ExecuteCommandAsync_Status_PrintsStatusAndContinues()
    {
        StringWriter output = new();
        StubAdminApiClient client = new()
        {
            Status = new AdminStatus
            {
                ApplicationName = "KeyEngine",
                ApplicationVersion = "1.0.0",
                State = "Running"
            }
        };
        AdminShell shell = CreateShell(
            client,
            output);

        bool shouldContinue =
            await shell.ExecuteCommandAsync("status");

        Assert.True(shouldContinue);
        Assert.Contains("KeyEngine", output.ToString());
        Assert.Contains("Running", output.ToString());
    }

    [Fact]
    public async Task ExecuteCommandAsync_InvalidCommand_DoesNotCrashAndContinues()
    {
        StringWriter output = new();
        AdminShell shell = CreateShell(
            output: output);

        bool shouldContinue =
            await shell.ExecuteCommandAsync("missing");

        Assert.True(shouldContinue);
        Assert.Contains("Unknown command", output.ToString());
    }

    [Theory]
    [InlineData("exit")]
    [InlineData("quit")]
    public async Task ExecuteCommandAsync_ExitCommands_StopShell(string command)
    {
        AdminShell shell = CreateShell();

        bool shouldContinue =
            await shell.ExecuteCommandAsync(command);

        Assert.False(shouldContinue);
    }

    [Fact]
    public async Task ExecuteCommandAsync_ConnectionFailure_PrintsErrorAndContinues()
    {
        StringWriter error = new();
        StubAdminApiClient client = new()
        {
            ThrowOnStatus = true
        };
        AdminShell shell = CreateShell(
            client,
            error: error);

        bool shouldContinue =
            await shell.ExecuteCommandAsync("status");

        Assert.True(shouldContinue);
        Assert.Contains("Unable to reach", error.ToString());
    }

    private static AdminShell CreateShell(
        IAdminApiClient? client = null,
        TextWriter? output = null,
        TextWriter? error = null)
    {
        return new AdminShell(
            new Uri("http://localhost:5000"),
            client ?? new StubAdminApiClient(),
            new StringReader(string.Empty),
            output ?? new StringWriter(),
            error ?? new StringWriter());
    }

    private sealed class StubAdminApiClient
        : IAdminApiClient
    {
        public AdminStatus? Status { get; init; } = new()
        {
            ApplicationName = "KeyEngine",
            ApplicationVersion = "1.0.0",
            State = "Running"
        };

        public bool ThrowOnStatus { get; init; }

        public Task<AdminStatus?> GetStatusAsync(
            CancellationToken cancellationToken = default)
        {
            if (ThrowOnStatus)
            {
                throw new HttpRequestException("Connection failed.");
            }

            return Task.FromResult(Status);
        }

        public Task<IReadOnlyList<AdminPlugin>> GetPluginsAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AdminPlugin>>([]);
        }

        public Task<IReadOnlyList<AdminParameter>> GetParametersAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AdminParameter>>([]);
        }

        public Task<IReadOnlyList<AdminLogEntry>> GetLogsAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AdminLogEntry>>([]);
        }

        public Task<IReadOnlyList<AdminRoute>> GetRoutesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AdminRoute>>([]);
        }
    }
}
