using KeyEngine.Parameters;
using KeyEngine.TestPlugin.Systems;

namespace KeyEngine.Tests.Web;

public sealed class WebServerSystemTests
{
    [Fact]
    public void ResolveStaticRoot_MissingParameter_UsesDefaultRoot()
    {
        ParameterManager parameters = TestEngineFactory.Create().Parameters;
        string defaultRoot = Path.Combine(
            Path.GetTempPath(),
            "KeyEngine.DefaultSite");

        string result = WebServerSystem.ResolveStaticRoot(
            parameters,
            defaultRoot);

        Assert.Equal(
            Path.GetFullPath(defaultRoot),
            result);
    }

    [Fact]
    public void ResolveStaticRoot_ConfiguredParameter_UsesConfiguredRoot()
    {
        ParameterManager parameters = TestEngineFactory.Create().Parameters;
        string configuredRoot = Path.Combine(
            Path.GetTempPath(),
            "KeyEngine.ConfiguredSite");
        parameters.Set(
            WebServerSystem.StaticRootParameterKey,
            configuredRoot);

        string result = WebServerSystem.ResolveStaticRoot(
            parameters,
            "unused-default");

        Assert.Equal(
            Path.GetFullPath(configuredRoot),
            result);
    }
}
