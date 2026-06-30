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
    /// Gets the provider-specific resource identifier.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Initializes a new resource location.
    /// </summary>
    /// <param name="scheme">
    /// The scheme that identifies the resource provider, such as <c>file</c>,
    /// <c>embedded</c>, or <c>memory</c>.
    /// </param>
    /// <param name="identifier">
    /// The provider-specific resource identifier.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the scheme or identifier is blank, or the scheme is malformed.
    /// </exception>
    public ResourceLocation(
        string scheme,
        string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        Scheme = NormalizeScheme(scheme);
        Identifier = identifier;
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
                   Identifier,
                   other.Identifier);
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
            Identifier is null
                ? 0
                : StringComparer.Ordinal.GetHashCode(Identifier));
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
        return $"{Scheme}://{Identifier}";
    }
}
