using KeyEngine.Abstractions;
using KeyEngine.Annotations;
using KeyEngine.Configuration;
using KeyEngine.Events;
using KeyEngine.Events.Models;
using KeyEngine.Metadata;
using KeyEngine.Plugins;
using KeyEngine.Plugins.Events;
using KeyEngine.Reflection;
using KeyEngine.Services;
using KeyEngine.Systems;

namespace KeyEngine.Tests.Plugins;

public sealed class PluginLifecycleEventTests
{
    [Fact]
    public void LoadPlugins_PublishesLoadedEventsInDependencyOrderWithPublicMetadata()
    {
        List<string> sequence = [];
        PluginLifecycleListener listener = new(sequence);
        PluginManager manager = CreateManager(listener);
        LoadedPlugin dependent = CreatePlugin(
            "dependent",
            dependencies: ["dependency"]);
        LoadedPlugin dependency = CreatePlugin("dependency");

        manager.LoadPlugins([dependent, dependency]);

        Assert.Equal(
            ["dependency", "dependent"],
            listener.LoadedEvents.Select(@event => @event.Id));

        PluginLoadedEvent loaded = listener.LoadedEvents[0];
        Assert.Equal("dependency name", loaded.Name);
        Assert.Equal(new Version(2, 1, 0), loaded.Version);
    }

    [Fact]
    public void LoadPlugins_PublishesRegisteredEventAfterConfigureCompletes()
    {
        List<string> sequence = [];
        PluginLifecycleListener listener = new(sequence);
        PluginManager manager = CreateManager(listener);
        LoadedPlugin plugin = CreatePlugin(
            "plugin",
            configure: () => sequence.Add("configure"));

        manager.LoadPlugins([plugin]);

        Assert.Equal(["configure", "registered"], sequence);

        PluginRegisteredEvent registered = Assert.Single(listener.RegisteredEvents);
        Assert.Equal("plugin", registered.Id);
        Assert.Equal("plugin name", registered.Name);
        Assert.Equal(new Version(2, 1, 0), registered.Version);
    }

    private static PluginManager CreateManager(
        PluginLifecycleListener listener)
    {
        ServiceCollection services = new();
        services.AddSingleton(listener);

        SystemRegistry systems = new(services.Build());
        EventBus events = new(systems);
        ScanResult result = new TypeScanner().Scan(
            typeof(PluginLifecycleListener));

        foreach (EventListenerMetadata metadata in
            result.EventListeners)
        {
            events.Register(metadata);
        }

        return new PluginManager(events);
    }

    private static LoadedPlugin CreatePlugin(
        string id,
        Action? configure = null,
        string[]? dependencies = null)
    {
        PluginManifest manifest = new()
        {
            Id = id,
            Main = typeof(RecordingPlugin).FullName!,
            Name = $"{id} name",
            Version = new Version(2, 1, 0),
            Dependencies = dependencies ?? []
        };

        return new LoadedPlugin
        {
            Assembly = typeof(PluginLifecycleEventTests).Assembly,
            Manifest = manifest,
            Instance = new RecordingPlugin(configure),
            Context = new PluginContext
            {
                Manifest = manifest,
                PluginDirectory = string.Empty,
                Configuration = new StubConfigurationManager()
            }
        };
    }

    private sealed class RecordingPlugin(Action? configure)
        : IPlugin
    {
        public void Configure(
            PluginContext context,
            IPluginBuilder builder)
        {
            configure?.Invoke();
        }
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

internal sealed class PluginLifecycleListener(List<string> sequence)
    : IEngineSystem
{
    public List<PluginLoadedEvent> LoadedEvents { get; } = [];

    public List<PluginRegisteredEvent> RegisteredEvents { get; } = [];

    [EventListener]
    private void OnLoaded(PluginLoadedEvent @event)
    {
        LoadedEvents.Add(@event);
    }

    [EventListener]
    private void OnRegistered(PluginRegisteredEvent @event)
    {
        RegisteredEvents.Add(@event);
        sequence.Add("registered");
    }
}
