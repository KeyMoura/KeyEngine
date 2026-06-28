namespace KeyEngine.Validation;

/// <summary>
/// Represents an error encountered while validating engine metadata.
/// </summary>
public sealed class EngineValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EngineValidationException"/> class.
    /// </summary>
    /// <param name="message">
    /// The validation error.
    /// </param>
    public EngineValidationException(string message)
        : base(message)
    {
    }
}