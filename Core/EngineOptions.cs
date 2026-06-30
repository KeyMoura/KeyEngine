using KeyEngine.Scheduler;
using System.Reflection;

namespace KeyEngine.Core;

/// <summary>
/// Represents the configuration used to create an <see cref="Engine"/>.
/// </summary>
internal sealed class EngineOptions
{
    /// <summary>
    /// Gets the assemblies that should be scanned by the engine.
    /// </summary>
    public required IReadOnlyList<Assembly> Assemblies { get; init; }

    /// <summary>
    /// Gets the plugin directory.
    /// </summary>
    public required string PluginDirectory { get; init; }

    /// <summary>
    /// Gets the scheduler configuration.
    /// </summary>
    public required SchedulerOptions Scheduler { get; init; }

    /// <summary>
    /// Gets or sets the engine metadata.
    /// </summary>
    public required ApplicationInfo Info { get; init; } = new();
}
