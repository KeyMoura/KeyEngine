namespace KeyEngine.Resources;

/// <summary>
/// Identifies the provider and provider-specific location of a resource.
/// </summary>
public readonly struct ResourceLocation
    : IEquatable<ResourceLocation>
{
    /// <summary>
    /// Gets the scheme that identifies the resource provider.
    /// </summary>
    public string Scheme { get; }

    /// <summary>
    /// Gets the provider-specific resource location.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new resource location.
    /// </summary>
    /// <param name="scheme">
    /// The scheme that identifies the resource provider, such as <c>file</c>,
    /// <c>embedded</c>, or <c>memory</c>.
    /// </param>
    /// <param name="value">
    /// The provider-specific resource location.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the scheme or value is blank, or the scheme is malformed.
    /// </exception>
    public ResourceLocation(
        string scheme,
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Scheme = NormalizeScheme(scheme);
        Value = value;
    }

    internal static string NormalizeScheme(string scheme)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheme);

        string normalized = scheme.Trim().ToLowerInvariant();

        if (!char.IsAsciiLetter(normalized[0]))
        {
            throw new ArgumentException(
                "The resource scheme must start with a letter.",
                nameof(scheme));
        }

        foreach (char character in normalized.AsSpan(1))
        {
            if (!char.IsAsciiLetterOrDigit(character) &&
                character is not '+' and not '-' and not '.')
            {
                throw new ArgumentException(
                    "The resource scheme contains an invalid character.",
                    nameof(scheme));
            }
        }

        return normalized;
    }

    /// <inheritdoc/>
    public bool Equals(ResourceLocation other)
    {
        return StringComparer.OrdinalIgnoreCase.Equals(
                   Scheme,
                   other.Scheme) &&
               StringComparer.Ordinal.Equals(
                   Value,
                   other.Value);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ResourceLocation other &&
               Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Scheme is null
                ? 0
                : StringComparer.OrdinalIgnoreCase.GetHashCode(Scheme),
            Value is null
                ? 0
                : StringComparer.Ordinal.GetHashCode(Value));
    }

    /// <summary>
    /// Determines whether two resource locations are equal.
    /// </summary>
    public static bool operator ==(
        ResourceLocation left,
        ResourceLocation right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two resource locations are not equal.
    /// </summary>
    public static bool operator !=(
        ResourceLocation left,
        ResourceLocation right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Scheme}://{Value}";
    }
}
