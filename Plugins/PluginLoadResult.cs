using KeyEngine.Plugins;
using System.Reflection;

internal sealed class PluginLoadResult
{
    public required Assembly Assembly { get; init; }

    public required Type PluginType { get; init; }

}
