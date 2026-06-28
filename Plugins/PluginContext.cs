using KeyEngine.Configuration;

namespace KeyEngine.Plugins
{
    public sealed class PluginContext
    {
        public required PluginManifest Manifest { get; init; }

        public required string PluginDirectory { get; init; }

        /// <summary>
        /// Gets the plugin configuration manager.
        /// </summary>
        public required IConfigurationManager Configuration
        {
            get;
            init;
        }

        public string ConfigDirectory =>
            Path.Combine(PluginDirectory, "config");

        public string DataDirectory =>
            Path.Combine(PluginDirectory, "data");

        public string CacheDirectory =>
            Path.Combine(PluginDirectory, "cache");

        public string LogsDirectory =>
            Path.Combine(PluginDirectory, "logs");
    }
}
