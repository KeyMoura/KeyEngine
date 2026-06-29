namespace KeyEngine.IO;

/// <summary>
/// Represents a filesystem.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Determines whether a file exists.
    /// </summary>
    /// <param name="path">
    /// The file path.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the file exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    bool FileExists(
        string path);

    /// <summary>
    /// Reads all text from a file.
    /// </summary>
    /// <param name="path">
    /// The file path.
    /// </param>
    /// <returns>
    /// The file contents.
    /// </returns>
    string ReadAllText(
        string path);

    /// <summary>
    /// Writes text to a file.
    /// </summary>
    /// <param name="path">
    /// The file path.
    /// </param>
    /// <param name="contents">
    /// The file contents.
    /// </param>
    void WriteAllText(
        string path,
        string contents);
}