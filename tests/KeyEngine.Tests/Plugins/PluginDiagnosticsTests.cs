using KeyEngine.Abstractions;
using KeyEngine.Annotations;
using KeyEngine.Configuration;
using KeyEngine.Diagnostics;
using KeyEngine.Events;
using KeyEngine.Plugins;
using KeyEngine.Reflection;

namespace KeyEngine.Tests.Plugins;

public sealed class PluginDiagnosticsTests
{
    [Fact]
    public void RegisteredPlugin_ReportsManifestAndRegistrationCounts()
    {
        PluginManifest manifest = new()
        {
            Id = "diagnostic",
            Main = typeof(DiagnosticPlugin).FullName!,
            Name = "Diagnostic Plugin",
            Version = new Version(3, 2, 1),
            Dependencies = ["dependency-a", "dependency-b"],
            LoadBefore = ["before"],
            LoadAfter = ["after-a", "after-b", "after-c"]
        };

        PluginContext context = new()
        {
            Manifest = manifest,
            PluginDirectory = string.Empty,
            Configuration = new StubConfigurationManager()
        };

        DiagnosticPlugin instance = new();
        PluginBuilder builder = new(context);
        instance.Configure(context, builder);

        TypeScanner scanner = new();

        foreach (Type system in builder.Systems)
        {
            builder.ScanResult.AddRange(scanner.Scan(system));
        }

        LoadedPlugin plugin = new()
        {
            Assembly = typeof(PluginDiagnosticsTests).Assembly,
            Manifest = manifest,
            Instance = instance,
            Context = context
        };

        PluginDiagnostics diagnostics =
            EngineDiagnostics.CreatePluginDiagnostics(
                plugin,
                builder);

        Assert.Equal("diagnostic", diagnostics.Id);
        Assert.Equal("Diagnostic Plugin", diagnostics.Name);
        Assert.Equal(new Version(3, 2, 1), diagnostics.Version);
        Assert.Equal(PluginState.Registered, diagnostics.State);
        Assert.Equal([PluginState.Registered], Enum.GetValues<PluginState>());
        Assert.Equal(2, diagnostics.DependencyCount);
        Assert.Equal(1, diagnostics.LoadBeforeCount);
        Assert.Equal(3, diagnostics.LoadAfterCount);
        Assert.Equal(2, diagnostics.SystemCount);
        Assert.Equal(1, diagnostics.CommandCount);
        Assert.Equal(1, diagnostics.EventListenerCount);
        Assert.Equal(3, diagnostics.ServiceCount);
    }

    private sealed class DiagnosticPlugin
        : IPlugin
    {
        public void Configure(
            PluginContext context,
            IPluginBuilder builder)
        {
            builder.AddSystem<DiagnosticSystem>();
            builder.AddSystem<EmptySystem>();
            builder.AddSingleton<DiagnosticService>();
        }
    }

    private sealed class DiagnosticSystem
        : IEngineSystem
    {
        [Command(Name = "diagnostic")]
        private void Command()
        {
        }

        [EventListener]
        private void OnEvent(DiagnosticEvent @event)
        {
        }
    }

    private sealed class EmptySystem
        : IEngineSystem
    {
    }

    private sealed class DiagnosticService
    {
    }

    private sealed class DiagnosticEvent
        : IEvent
    {
    }

    private sealed class StubConfigurationManager
        : IConfigurationManager
    {
        public T Get<T>()
            where T : class, new()
        {
            return new T();
        }

        public void Save()
        {
        }
    }
}
