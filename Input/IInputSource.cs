namespace KeyEngine.Input;

/// <summary>
/// Provides input state to the engine.
/// </summary>
/// <remarks>
/// Sources are updated synchronously on the engine thread once per frame. Each
/// update reports snapshot state. A complete press and release reported during
/// the same callback is not preserved as an aggregate frame transition.
/// </remarks>
public interface IInputSource
{
    /// <summary>
    /// Updates the current input state.
    /// </summary>
    /// <param name="update">
    /// The input update used to report source state. The update is valid only
    /// for the duration of this callback.
    /// </param>
    void Update(InputUpdate update);
}
