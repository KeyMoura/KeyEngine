namespace KeyEngine.Input;

/// <summary>
/// Provides input state to the engine.
/// </summary>
public interface IInputSource
{
    /// <summary>
    /// Updates the current input state.
    /// </summary>
    /// <param name="update">
    /// The input update used to report source state.
    /// </param>
    void Update(InputUpdate update);
}
