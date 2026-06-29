namespace KeyEngine.IO;

/// <summary>
/// Represents the operating system's filesystem.
/// </summary>
public sealed class PhysicalFileSystem
    : IFileSystem
{
    /// <inheritdoc/>
    public bool FileExists(
        string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return File.Exists(path);
    }

    /// <inheritdoc/>
    public string ReadAllText(
        string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return File.ReadAllText(path);
    }

    /// <inheritdoc/>
    public void WriteAllText(
        string path,
        string contents)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(contents);

        File.WriteAllText(
            path,
            contents);
    }
}