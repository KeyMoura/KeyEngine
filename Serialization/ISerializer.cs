namespace KeyEngine.Serialization;

/// <summary>
/// Converts objects to and from text.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// Serializes a value to text.
    /// </summary>
    /// <typeparam name="T">
    /// The value type.
    /// </typeparam>
    /// <param name="value">
    /// The value to serialize.
    /// </param>
    /// <returns>
    /// The serialized text.
    /// </returns>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializes a value from text.
    /// </summary>
    /// <typeparam name="T">
    /// The value type.
    /// </typeparam>
    /// <param name="text">
    /// The serialized text.
    /// </param>
    /// <returns>
    /// The deserialized value, or <see langword="null"/> when the serialized
    /// value is <see langword="null"/>.
    /// </returns>
    T? Deserialize<T>(string text);
}
