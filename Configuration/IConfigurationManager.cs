namespace KeyEngine.Configuration;

/// <summary>
/// Provides access to plugin configuration.
/// </summary>
public interface IConfigurationManager
{
    /// <summary>
    /// Gets the configuration object.
    /// </summary>
    /// <typeparam name="T">
    /// The configuration type.
    /// </typeparam>
    /// <returns>
    /// The configuration instance.
    /// </returns>
    T Get<T>()
        where T : class, new();

    /// <summary>
    /// Saves all loaded configuration files.
    /// </summary>
    void Save();
}