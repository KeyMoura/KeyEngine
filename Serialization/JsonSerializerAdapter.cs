using System.Text.Json;

namespace KeyEngine.Serialization;

/// <summary>
/// Serializes values using the System.Text.Json serializer.
/// </summary>
public sealed class JsonSerializerAdapter
    : ISerializer
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new JSON serializer using the default options.
    /// </summary>
    public JsonSerializerAdapter()
        : this(new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        })
    {
    }

    /// <summary>
    /// Initializes a new JSON serializer using the specified options.
    /// </summary>
    /// <param name="options">
    /// The JSON serializer options.
    /// </param>
    public JsonSerializerAdapter(
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(
            value,
            _options);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        return JsonSerializer.Deserialize<T>(
            text,
            _options);
    }
}
