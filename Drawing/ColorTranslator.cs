using System.Text;

namespace KeyEngine.Drawing;

/// <summary>
/// Translates color codes in text.
/// </summary>
public static class ColorTranslator
{
    private static readonly Dictionary<string, Color> _namedColors =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["black"] = Color.Black,
        ["white"] = Color.White,
        ["red"] = Color.Red,
        ["green"] = Color.Green,
        ["blue"] = Color.Blue,
        ["yellow"] = Color.Yellow,
        ["cyan"] = Color.Cyan,
        ["magenta"] = Color.Magenta
    };

    /// <summary>
    /// Registers a named color.
    /// </summary>
    public static void Register(
        string name,
        Color color)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _namedColors[name] = color;
    }

    /// <summary>
    /// Translates hexadecimal color codes into ANSI escape sequences.
    /// </summary>
    /// <param name="text">
    /// The input text.
    /// </param>
    /// <param name="prefix">
    /// The color prefix.
    /// </param>
    /// <returns>
    /// The translated text.
    /// </returns>
    public static string TranslateAnsi(
        string text,
        char prefix = '&')
    {
        ArgumentNullException.ThrowIfNull(text);

        StringBuilder builder = new();

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] != prefix)
            {
                builder.Append(text[i]);
                continue;
            }

            // Not enough characters for a hex color.
            if (i + 7 >= text.Length)
            {
                builder.Append(prefix);
                continue;
            }

            // Require '&#RRGGBB'
            if (text[i + 1] != '#')
            {
                builder.Append(prefix);
                continue;
            }

            string hex =
                text.Substring(i + 2, 6);

            if (!Color.TryParse(
                    hex,
                    out Color color))
            {
                builder.Append(prefix);
                continue;
            }

            builder.Append(
                $"\u001b[38;2;{color.R};{color.G};{color.B}m");

            i += 7;
        }

        builder.Append("\u001b[0m");

        return builder.ToString();
    }
}