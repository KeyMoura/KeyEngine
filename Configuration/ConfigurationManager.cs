using KeyEngine.IO;
using System.Text.Json;

namespace KeyEngine.Configuration;

/// <summary>
/// Default configuration manager.
/// </summary>
public sealed class ConfigurationManager
    : IConfigurationManager
{
    private readonly string _directory;

    private readonly IFileSystem _fileSystem;

    private readonly Dictionary<Type, object> _loaded = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationManager"/> class.
    /// </summary>
    /// <param name="directory">
    /// The configuration directory.
    /// </param>
    public ConfigurationManager(
    string directory,
    IFileSystem fileSystem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _directory = directory;
        _fileSystem = fileSystem;
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
                JsonSerializer.Deserialize<T>(
                    json,
                    JsonOptions)
                ?? new T();
        }
        else
        {
            configuration = new T();

            string json =
                JsonSerializer.Serialize(
                    configuration,
                    JsonOptions);

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
                JsonSerializer.Serialize(
                    configuration,
                    type,
                    JsonOptions);

            _fileSystem.WriteAllText(
                path,
                json);
        }
    }
}