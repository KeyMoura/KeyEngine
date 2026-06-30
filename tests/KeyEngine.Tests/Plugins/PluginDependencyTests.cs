using KeyEngine.Plugins;
using KeyEngine.Validation;

namespace KeyEngine.Tests.Plugins;

public sealed class PluginDependencyTests
{
    [Fact]
    public void OrderPlugins_PluginsWithoutDependencies_PreserveDiscoveryOrder()
    {
        LoadedPlugin first = CreatePlugin("first");
        LoadedPlugin second = CreatePlugin("second");

        IReadOnlyList<LoadedPlugin> ordered =
            PluginManager.OrderPlugins([first, second]);

        Assert.Equal([first, second], ordered);
    }

    [Fact]
    public void OrderPlugins_MissingDependency_ThrowsWithBothPluginIds()
    {
        LoadedPlugin plugin = CreatePlugin(
            "dependent",
            "missing");

        EngineValidationException exception = Assert.Throws<EngineValidationException>(
            () => PluginManager.OrderPlugins([plugin]));

        Assert.Contains("dependent", exception.Message);
        Assert.Contains("missing", exception.Message);
    }

    [Fact]
    public void OrderPlugins_DependencyDiscoveredLater_OrdersDependencyFirst()
    {
        LoadedPlugin dependent = CreatePlugin(
            "dependent",
            "dependency");
        LoadedPlugin dependency = CreatePlugin("dependency");

        IReadOnlyList<LoadedPlugin> ordered =
            PluginManager.OrderPlugins([dependent, dependency]);

        Assert.Equal([dependency, dependent], ordered);
    }

    [Fact]
    public void OrderPlugins_DuplicatePluginId_Throws()
    {
        LoadedPlugin first = CreatePlugin("duplicate");
        LoadedPlugin second = CreatePlugin("duplicate");

        EngineValidationException exception = Assert.Throws<EngineValidationException>(
            () => PluginManager.OrderPlugins([first, second]));

        Assert.Contains("duplicate", exception.Message);
    }

    [Fact]
    public void OrderPlugins_DependencyCycle_ThrowsWithInvolvedPluginIds()
    {
        LoadedPlugin first = CreatePlugin(
            "first",
            "second");
        LoadedPlugin second = CreatePlugin(
            "second",
            "first");

        EngineValidationException exception = Assert.Throws<EngineValidationException>(
            () => PluginManager.OrderPlugins([first, second]));

        Assert.Contains("first", exception.Message);
        Assert.Contains("second", exception.Message);
    }

    [Fact]
    public void OrderPlugins_LoadAfterTargetExists_OrdersPluginAfterTarget()
    {
        LoadedPlugin plugin = CreatePlugin("plugin");
        LoadedPlugin target = CreatePlugin("target");
        plugin.Manifest.LoadAfter.Add(target.Manifest.Id);

        IReadOnlyList<LoadedPlugin> ordered =
            PluginManager.OrderPlugins([plugin, target]);

        Assert.Equal([target, plugin], ordered);
    }

    [Fact]
    public void OrderPlugins_LoadAfterTargetMissing_IgnoresHint()
    {
        LoadedPlugin plugin = CreatePlugin("plugin");
        plugin.Manifest.LoadAfter.Add("missing");

        IReadOnlyList<LoadedPlugin> ordered =
            PluginManager.OrderPlugins([plugin]);

        Assert.Equal([plugin], ordered);
    }

    [Fact]
    public void OrderPlugins_LoadBeforeTargetExists_OrdersPluginBeforeTarget()
    {
        LoadedPlugin target = CreatePlugin("target");
        LoadedPlugin plugin = CreatePlugin("plugin");
        plugin.Manifest.LoadBefore.Add(target.Manifest.Id);

        IReadOnlyList<LoadedPlugin> ordered =
            PluginManager.OrderPlugins([target, plugin]);

        Assert.Equal([plugin, target], ordered);
    }

    [Fact]
    public void OrderPlugins_LoadBeforeTargetMissing_IgnoresHint()
    {
        LoadedPlugin plugin = CreatePlugin("plugin");
        plugin.Manifest.LoadBefore.Add("missing");

        IReadOnlyList<LoadedPlugin> ordered =
            PluginManager.OrderPlugins([plugin]);

        Assert.Equal([plugin], ordered);
    }

    [Fact]
    public void OrderPlugins_LoadHintsCreateCycle_ThrowsWithInvolvedPluginIds()
    {
        LoadedPlugin first = CreatePlugin("first");
        LoadedPlugin second = CreatePlugin("second");
        first.Manifest.LoadAfter.Add(second.Manifest.Id);
        second.Manifest.LoadAfter.Add(first.Manifest.Id);

        EngineValidationException exception = Assert.Throws<EngineValidationException>(
            () => PluginManager.OrderPlugins([first, second]));

        Assert.Contains("first", exception.Message);
        Assert.Contains("second", exception.Message);
    }

    private static LoadedPlugin CreatePlugin(
        string id,
        params string[] dependencies)
    {
        return new LoadedPlugin
        {
            Assembly = typeof(PluginDependencyTests).Assembly,
            Manifest = new PluginManifest
            {
                Id = id,
                Main = typeof(TestPlugin).FullName!,
                Name = id,
                Dependencies = dependencies
            },
            Instance = new TestPlugin(),
            Context = null!
        };
    }

    private sealed class TestPlugin
        : IPlugin
    {
        public void Configure(
            PluginContext context,
            IPluginBuilder builder)
        {
        }
    }
}
