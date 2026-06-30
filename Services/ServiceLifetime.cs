namespace KeyEngine.Services;

/// <summary>
/// Specifies the lifetime of a registered service.
/// </summary>
internal enum ServiceLifetime
{
    /// <summary>
    /// A single instance is created and reused.
    /// </summary>
    Singleton,

    /// <summary>
    /// A new instance is created every request.
    /// </summary>
    Transient
}
