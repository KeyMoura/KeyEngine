namespace KeyEngine.Plugins;

/// <summary>
/// Represents a KeyEngine plugin.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Configures the plugin.
    /// </summary>
    /// <param name="builder">
    /// The plugin builder.
    /// </param>
    void Configure(
        PluginContext context,
        IPluginBuilder builder);
}