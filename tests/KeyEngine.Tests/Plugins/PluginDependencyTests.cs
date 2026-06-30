using KeyEngine.Plugins;
using KeyEngine.Validation;

namespace KeyEngine.Tests.Plugins;

public sealed class PluginDependencyTests
{
    [Fact]
    public void OrderByDependencies_PluginsWithoutDependencies_PreserveDiscoveryOrder()
    {
        LoadedPlugin first = CreatePlugin("first");
        LoadedPlugin second = CreatePlugin("second");

        IReadOnlyList<LoadedPlugin> ordered =
            PluginManager.OrderByDependencies([first, second]);

        Assert.Equal([first, second], ordered);
    }

    [Fact]
    public void OrderByDependencies_MissingDependency_ThrowsWithBothPluginIds()
    {
        LoadedPlugin plugin = CreatePlugin(
            "dependent",
            "missing");

        EngineValidationException exception = Assert.Throws<EngineValidationException>(
            () => PluginManager.OrderByDependencies([plugin]));

        Assert.Contains("dependent", exception.Message);
        Assert.Contains("missing", exception.Message);
    }

    [Fact]
    public void OrderByDependencies_DependencyDiscoveredLater_OrdersDependencyFirst()
    {
        LoadedPlugin dependent = CreatePlugin(
            "dependent",
            "dependency");
        LoadedPlugin dependency = CreatePlugin("dependency");

        IReadOnlyList<LoadedPlugin> ordered =
            PluginManager.OrderByDependencies([dependent, dependency]);

        Assert.Equal([dependency, dependent], ordered);
    }

    [Fact]
    public void OrderByDependencies_DuplicatePluginId_Throws()
    {
        LoadedPlugin first = CreatePlugin("duplicate");
        LoadedPlugin second = CreatePlugin("duplicate");

        EngineValidationException exception = Assert.Throws<EngineValidationException>(
            () => PluginManager.OrderByDependencies([first, second]));

        Assert.Contains("duplicate", exception.Message);
    }

    [Fact]
    public void OrderByDependencies_DependencyCycle_ThrowsWithInvolvedPluginIds()
    {
        LoadedPlugin first = CreatePlugin(
            "first",
            "second");
        LoadedPlugin second = CreatePlugin(
            "second",
            "first");

        EngineValidationException exception = Assert.Throws<EngineValidationException>(
            () => PluginManager.OrderByDependencies([first, second]));

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
