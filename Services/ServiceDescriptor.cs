namespace KeyEngine.Services;

/// <summary>
/// Represents a registered service.
/// </summary>
public sealed class ServiceDescriptor
{
    /// <summary>
    /// Gets the service type.
    /// </summary>
    public required Type ServiceType { get; init; }

    /// <summary>
    /// Gets the implementation type.
    /// </summary>
    public required Type ImplementationType { get; init; }

    /// <summary>
    /// Gets the service lifetime.
    /// </summary>
    public required ServiceLifetime Lifetime { get; init; }

    /// <summary>
    /// Gets or sets the singleton instance.
    /// </summary>
    internal object? Instance { get; set; }
}