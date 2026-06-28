using KeyEngine.Plugins;
using System.Reflection;

public sealed class PluginLoadResult
{
    public required Assembly Assembly { get; init; }

    public required Type PluginType { get; init; }

}