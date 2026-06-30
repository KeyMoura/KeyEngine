using KeyEngine.Configuration;
using KeyEngine.IO;

namespace KeyEngine.Plugins;

/// <summary>
/// Creates plugin contexts.
/// </summary>
internal sealed class PluginContextFactory
{

    /// <summary>
    /// Creates a plugin context.
    /// </summary>
    /// <param name="pluginsDirectory">
    /// The engine's plugins directory.
    /// </param>
    /// <param name="manifest">
    /// The plugin manifest.
    /// </param>
    /// <returns>
    /// The created plugin context.
    /// </returns>
    public PluginContext Create(
        string pluginsDirectory,
        PluginManifest manifest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginsDirectory);
        ArgumentNullException.ThrowIfNull(manifest);

        string pluginDirectory =
            Path.Combine(
                pluginsDirectory,
                manifest.Id);

        ConfigurationManager configuration =
            new(
                Path.Combine(
                    pluginDirectory,
                    "config"),
                new PhysicalFileSystem());

        PluginContext context = new()
        {
            Manifest = manifest,
            PluginDirectory = pluginDirectory,
            Configuration = configuration
        };

        Directory.CreateDirectory(context.PluginDirectory);
        Directory.CreateDirectory(context.ConfigDirectory);
        Directory.CreateDirectory(context.DataDirectory);
        Directory.CreateDirectory(context.CacheDirectory);
        Directory.CreateDirectory(context.LogsDirectory);

        string manifestPath =
            Path.Combine(
                context.PluginDirectory,
                "plugin.json");

        string json =
            System.Text.Json.JsonSerializer.Serialize(
                manifest,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(
            manifestPath,
            json);

        return context;
    }

}
