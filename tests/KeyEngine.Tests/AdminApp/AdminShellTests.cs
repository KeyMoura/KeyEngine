using KeyEngine.AdminApp;
using KeyEngine.AdminClient;

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

    [Fact]
    public async Task ExecuteCommandAsync_SetParam_CallsApiSuccessfully()
    {
        StringWriter output = new();
        StubAdminApiClient client = new();
        AdminShell shell = CreateShell(
            client,
            output);

        bool shouldContinue =
            await shell.ExecuteCommandAsync("set-param feature.mode enabled");

        Assert.True(shouldContinue);
        Assert.Equal("feature.mode", client.SetParameterKey);
        Assert.Equal("enabled", client.SetParameterValue);
        Assert.Contains("updated", output.ToString());
    }

    [Fact]
    public async Task ExecuteCommandAsync_DeleteParam_CallsApiSuccessfully()
    {
        StringWriter output = new();
        StubAdminApiClient client = new();
        AdminShell shell = CreateShell(
            client,
            output);

        bool shouldContinue =
            await shell.ExecuteCommandAsync("delete-param feature.mode");

        Assert.True(shouldContinue);
        Assert.Equal("feature.mode", client.DeletedParameterKey);
        Assert.Contains("deleted", output.ToString());
    }

    [Fact]
    public async Task ExecuteCommandAsync_ClearLogs_CallsApiSuccessfully()
    {
        StringWriter output = new();
        StubAdminApiClient client = new();
        AdminShell shell = CreateShell(
            client,
            output);

        bool shouldContinue =
            await shell.ExecuteCommandAsync("clear-logs");

        Assert.True(shouldContinue);
        Assert.True(client.ClearLogsCalled);
        Assert.Contains("cleared", output.ToString());
    }

    [Fact]
    public async Task ExecuteCommandAsync_Token_SetsAdminToken()
    {
        StringWriter output = new();
        StubAdminApiClient client = new();
        AdminShell shell = CreateShell(
            client,
            output);

        bool shouldContinue =
            await shell.ExecuteCommandAsync("token secret");

        Assert.True(shouldContinue);
        Assert.Equal("secret", client.AdminToken);
        Assert.Contains("Admin token set", output.ToString());
    }

    [Fact]
    public async Task ExecuteCommandAsync_SaveParams_CallsApiSuccessfully()
    {
        StringWriter output = new();
        StubAdminApiClient client = new();
        AdminShell shell = CreateShell(
            client,
            output);

        bool shouldContinue =
            await shell.ExecuteCommandAsync("save-params parameters.json");

        Assert.True(shouldContinue);
        Assert.Equal("parameters.json", client.SavedPath);
        Assert.Contains("saved", output.ToString());
    }

    [Fact]
    public async Task ExecuteCommandAsync_LoadParams_CallsApiSuccessfully()
    {
        StringWriter output = new();
        StubAdminApiClient client = new();
        AdminShell shell = CreateShell(
            client,
            output);

        bool shouldContinue =
            await shell.ExecuteCommandAsync("load-params parameters.json");

        Assert.True(shouldContinue);
        Assert.Equal("parameters.json", client.LoadedPath);
        Assert.Contains("loaded", output.ToString());
    }

    [Theory]
    [InlineData("set-param")]
    [InlineData("delete-param")]
    [InlineData("save-params")]
    [InlineData("load-params")]
    public async Task ExecuteCommandAsync_MissingArguments_ShowsUsage(string command)
    {
        StringWriter output = new();
        AdminShell shell = CreateShell(
            output: output);

        bool shouldContinue =
            await shell.ExecuteCommandAsync(command);

        Assert.True(shouldContinue);
        Assert.Contains("Usage:", output.ToString());
    }

    [Fact]
    public async Task ExecuteCommandAsync_UnauthorizedMutation_ShowsHelpfulError()
    {
        StringWriter error = new();
        StubAdminApiClient client = new()
        {
            ThrowUnauthorizedOnClearLogs = true
        };
        AdminShell shell = CreateShell(
            client,
            error: error);

        bool shouldContinue =
            await shell.ExecuteCommandAsync("clear-logs");

        Assert.True(shouldContinue);
        Assert.Contains("admin token", error.ToString());
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

        public bool ThrowUnauthorizedOnClearLogs { get; init; }

        public string? AdminToken { get; set; }

        public string? SetParameterKey { get; private set; }

        public string? SetParameterValue { get; private set; }

        public string? DeletedParameterKey { get; private set; }

        public string? SavedPath { get; private set; }

        public string? LoadedPath { get; private set; }

        public bool ClearLogsCalled { get; private set; }

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

        public Task SetParameterAsync(
            string key,
            string value,
            CancellationToken cancellationToken = default)
        {
            SetParameterKey = key;
            SetParameterValue = value;
            return Task.CompletedTask;
        }

        public Task SetParameterAsync(
            string key,
            string value,
            string? description,
            string? category,
            CancellationToken cancellationToken = default)
        {
            SetParameterKey = key;
            SetParameterValue = value;
            return Task.CompletedTask;
        }

        public Task DeleteParameterAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            DeletedParameterKey = key;
            return Task.CompletedTask;
        }

        public Task SaveParametersAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            SavedPath = path;
            return Task.CompletedTask;
        }

        public Task LoadParametersAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            LoadedPath = path;
            return Task.CompletedTask;
        }

        public Task ClearLogsAsync(
            CancellationToken cancellationToken = default)
        {
            if (ThrowUnauthorizedOnClearLogs)
            {
                throw new HttpRequestException(
                    "Unauthorized",
                    null,
                    System.Net.HttpStatusCode.Unauthorized);
            }

            ClearLogsCalled = true;
            return Task.CompletedTask;
        }
    }
}
