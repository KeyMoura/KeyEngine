namespace KeyEngine.Drawing;

/// <summary>
/// Represents an RGBA color.
/// </summary>
public readonly partial struct Color
    : IEquatable<Color>
{
    /// <summary>
    /// Gets the red component.
    /// </summary>
    public byte R { get; }

    /// <summary>
    /// Gets the green component.
    /// </summary>
    public byte G { get; }

    /// <summary>
    /// Gets the blue component.
    /// </summary>
    public byte B { get; }

    /// <summary>
    /// Gets the alpha component.
    /// </summary>
    public byte A { get; }

    /// <summary>
    /// Represents a fully transparent color.
    /// </summary>
    public static Color Transparent { get; } =
        new(0, 0, 0, 0);

    /// <summary>
    /// Represents black.
    /// </summary>
    public static Color Black { get; } =
        new(0, 0, 0);

    /// <summary>
    /// Represents white.
    /// </summary>
    public static Color White { get; } =
        new(255, 255, 255);

    /// <summary>
    /// Represents red.
    /// </summary>
    public static Color Red { get; } =
        new(255, 0, 0);

    /// <summary>
    /// Represents green.
    /// </summary>
    public static Color Green { get; } =
        new(0, 255, 0);

    /// <summary>
    /// Represents blue.
    /// </summary>
    public static Color Blue { get; } =
        new(0, 0, 255);

    /// <summary>
    /// Represents yellow.
    /// </summary>
    public static Color Yellow { get; } =
        new(255, 255, 0);

    /// <summary>
    /// Represents cyan.
    /// </summary>
    public static Color Cyan { get; } =
        new(0, 255, 255);

    /// <summary>
    /// Represents magenta.
    /// </summary>
    public static Color Magenta { get; } =
        new(255, 0, 255);

    /// <summary>
    /// Creates a color from RGB components.
    /// </summary>
    public static Color FromRgb(
        byte r,
        byte g,
        byte b)
    {
        return new(
            r,
            g,
            b);
    }

    /// <summary>
    /// Creates a color from ARGB components.
    /// </summary>
    public static Color FromArgb(
        byte a,
        byte r,
        byte g,
        byte b)
    {
        return new(
            r,
            g,
            b,
            a);
    }

    /// <summary>
    /// Attempts to parse a hexadecimal color.
    /// </summary>
    /// <param name="text">
    /// The hexadecimal color.
    /// </param>
    /// <param name="color">
    /// The parsed color.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if successful; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryParse(
        string text,
        out Color color)
    {
        ArgumentNullException.ThrowIfNull(text);

        color = default;

        if (text.StartsWith('#'))
        {
            text = text[1..];
        }

        if (text.Length == 6)
        {
            if (!byte.TryParse(
                    text[..2],
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out byte r) ||
                !byte.TryParse(
                    text.Substring(2, 2),
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out byte g) ||
                !byte.TryParse(
                    text.Substring(4, 2),
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out byte b))
            {
                return false;
            }

            color = new Color(
                r,
                g,
                b);

            return true;
        }

        if (text.Length == 8)
        {
            if (!byte.TryParse(
                    text[..2],
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out byte a) ||
                !byte.TryParse(
                    text.Substring(2, 2),
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out byte r) ||
                !byte.TryParse(
                    text.Substring(4, 2),
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out byte g) ||
                !byte.TryParse(
                    text.Substring(6, 2),
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out byte b))
            {
                return false;
            }

            color = new Color(
                r,
                g,
                b,
                a);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Parses a hexadecimal color.
    /// </summary>
    /// <param name="text">
    /// The hexadecimal color.
    /// </param>
    /// <returns>
    /// The parsed color.
    /// </returns>
    public static Color FromHex(
        string text)
    {
        if (!TryParse(
                text,
                out Color color))
        {
            throw new FormatException(
                $"'{text}' is not a valid color.");
        }

        return color;
    }

    /// <summary>
    /// Returns the color as a hexadecimal string.
    /// </summary>
    public string ToHex()
    {
        if (A == 255)
        {
            return $"#{R:X2}{G:X2}{B:X2}";
        }

        return $"#{A:X2}{R:X2}{G:X2}{B:X2}";
    }

    /// <summary>
    /// Initializes a new color.
    /// </summary>
    public Color(
        byte r,
        byte g,
        byte b,
        byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <inheritdoc/>
    public bool Equals(
        Color other)
    {
        return R == other.R &&
               G == other.G &&
               B == other.B &&
               A == other.A;
    }

    /// <inheritdoc/>
    public override bool Equals(
        object? obj)
    {
        return obj is Color other &&
               Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            R,
            G,
            B,
            A);
    }

    /// <summary>
    /// Determines whether two colors are equal.
    /// </summary>
    public static bool operator ==(
        Color left,
        Color right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two colors are not equal.
    /// </summary>
    public static bool operator !=(
        Color left,
        Color right)
    {
        return !left.Equals(right);
    }
}