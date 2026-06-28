namespace KeyEngine.Core;

/// <summary>
/// Represents metadata about the engine application.
/// </summary>
public class ApplicationInfo
{
    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public string Name { get; set; } = "Unnamed Application";

    /// <summary>
    /// Gets or sets the application description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application version.
    /// </summary>
    public Version Version { get; set; } = new(1, 0, 0);

    /// <summary>
    /// Gets the application authors.
    /// </summary>
    public IList<string> Authors { get; } = new List<string>();

    /// <summary>
    /// Gets or sets the application company.
    /// </summary>
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application website URL.
    /// </summary>
    public string WebsiteUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application license.
    /// </summary>
    public string License { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application repository URL.
    /// </summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application copyright.
    /// </summary>
    public string Copyright { get; set; } = string.Empty;
}