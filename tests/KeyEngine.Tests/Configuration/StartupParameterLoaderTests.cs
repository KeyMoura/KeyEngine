using KeyEngine.Core;
using KeyEngine.Parameters;

namespace KeyEngine.Tests.Configuration;

public sealed class StartupParameterLoaderTests
{
    [Fact]
    public void LoadIfExists_MissingFile_LogsAndContinues()
    {
        ParameterManager parameters = TestEngineFactory.Create().Parameters;
        StringWriter output = new();
        string path = Path.Combine(
            Path.GetTempPath(),
            $"KeyEngine.Tests.{Guid.NewGuid():N}.json");

        bool loaded = StartupParameterLoader.LoadIfExists(
            parameters,
            path,
            output);

        Assert.False(loaded);
        Assert.Contains("loading skipped", output.ToString());
    }

    [Fact]
    public void LoadIfExists_ExistingFile_LoadsBeforeUse()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"KeyEngine.Tests.{Guid.NewGuid():N}.json");

        try
        {
            Engine sourceEngine = TestEngineFactory.Create();
            sourceEngine.Parameters.Set(
                "web.static.root",
                "configured-site");
            sourceEngine.Parameters.Save(path);

            ParameterManager target = TestEngineFactory.Create().Parameters;
            StringWriter output = new();

            bool loaded = StartupParameterLoader.LoadIfExists(
                target,
                path,
                output);

            Assert.True(loaded);
            Assert.Equal(
                "configured-site",
                target.Get<string>("web.static.root"));
            Assert.Contains("Parameters loaded", output.ToString());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadIfExists_InvalidFile_ThrowsClearException()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"KeyEngine.Tests.{Guid.NewGuid():N}.json");
        File.WriteAllText(path, "not valid json");

        try
        {
            ParameterManager parameters = TestEngineFactory.Create().Parameters;

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => StartupParameterLoader.LoadIfExists(
                    parameters,
                    path,
                    TextWriter.Null));

            Assert.Contains(path, exception.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
