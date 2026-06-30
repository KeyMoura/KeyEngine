namespace KeyEngine.Networking;

/// <summary>
/// Represents the current state of a network endpoint or connection.
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// The endpoint is not connected or listening.
    /// </summary>
    Disconnected,

    /// <summary>
    /// A connection attempt is in progress.
    /// </summary>
    Connecting,

    /// <summary>
    /// The connection is established.
    /// </summary>
    Connected,

    /// <summary>
    /// The server is listening for connections.
    /// </summary>
    Listening
}
