using KeyEngine.IO;
using KeyEngine.Serialization;

namespace KeyEngine.Configuration;

/// <summary>
/// Default configuration manager.
/// </summary>
public sealed class ConfigurationManager
    : IConfigurationManager
{
    private readonly string _directory;

    private readonly IFileSystem _fileSystem;

    private readonly ISerializer _serializer;

    private readonly Dictionary<Type, object> _loaded = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationManager"/> class.
    /// </summary>
    /// <param name="directory">
    /// The configuration directory.
    /// </param>
    /// <param name="fileSystem">
    /// The filesystem used to read and write configuration files.
    /// </param>
    public ConfigurationManager(
    string directory,
    IFileSystem fileSystem)
        : this(
            directory,
            fileSystem,
            new JsonSerializerAdapter())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationManager"/> class.
    /// </summary>
    /// <param name="directory">
    /// The configuration directory.
    /// </param>
    /// <param name="fileSystem">
    /// The filesystem used to read and write configuration files.
    /// </param>
    /// <param name="serializer">
    /// The serializer used for configuration values.
    /// </param>
    public ConfigurationManager(
        string directory,
        IFileSystem fileSystem,
        ISerializer serializer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(serializer);

        _directory = directory;
        _fileSystem = fileSystem;
        _serializer = serializer;
    }

    /// <inheritdoc/>
    public T Get<T>()
    where T : class, new()
    {
        if (_loaded.TryGetValue(typeof(T), out object? existing))
        {
            return (T)existing;
        }

        string path =
            Path.Combine(
                _directory,
                 $"{typeof(T).Name}.json");

        T configuration;

        if (_fileSystem.FileExists(path))
        {
            string json =
                _fileSystem.ReadAllText(path);

            configuration =
                _serializer.Deserialize<T>(json)
                ?? new T();
        }
        else
        {
            configuration = new T();

            string json =
                _serializer.Serialize(configuration);

            _fileSystem.WriteAllText(
                path,
                json);
        }

        _loaded.Add(
            typeof(T),
            configuration);

        return configuration;
    }

    /// <inheritdoc/>
    public void Save()
    {
        foreach ((Type type, object configuration) in _loaded)
        {
            string path =
                Path.Combine(
                    _directory,
                    $"{type.Name}.json");

            string json =
                _serializer.Serialize(configuration);

            _fileSystem.WriteAllText(
                path,
                json);
        }
    }
}
