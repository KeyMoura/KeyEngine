using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace KeyEngine.Plugins;

/// <summary>
/// Loads plugin manifests.
/// </summary>
internal sealed class PluginManifestLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Loads the plugin manifest embedded in an assembly.
    /// </summary>
    /// <param name="assembly">
    /// The plugin assembly.
    /// </param>
    /// <returns>
    /// The loaded manifest.
    /// </returns>
    public bool TryLoad(
        Assembly assembly,
        [NotNullWhen(true)]
        out PluginManifest? manifest)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        string? resourceName =
            assembly.GetManifestResourceNames()
                .FirstOrDefault(name =>
                    name.EndsWith(
                        "plugin.json",
                        StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            manifest = null;
            return false;
        }

        using Stream stream =
            assembly.GetManifestResourceStream(resourceName)!
            ?? throw new InvalidOperationException(
                "Unable to open the embedded plugin manifest.");

        using StreamReader reader = new(stream);

        string json = reader.ReadToEnd();

        try
        {
            manifest =
                JsonSerializer.Deserialize<PluginManifest>(
                    json,
                    JsonOptions);

            if (manifest is null)
            {
                throw new InvalidOperationException(
                    "The plugin manifest is invalid.");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            throw;
        }
    }
}
