using System.Net;
using System.Net.Sockets;

namespace KeyEngine.Networking;

/// <summary>
/// Creates an outbound TCP connection.
/// </summary>
/// <remarks>
/// Instances are not guaranteed to be thread-safe.
/// </remarks>
public sealed class TcpClient
    : IDisposable
{
    private ConnectionState _state = ConnectionState.Disconnected;

    /// <summary>
    /// Gets the current client state.
    /// </summary>
    public ConnectionState State =>
        Connection?.State ?? _state;

    /// <summary>
    /// Gets the active connection, if one exists.
    /// </summary>
    public TcpConnection? Connection { get; private set; }

    internal TcpClient()
    {
    }

    /// <summary>
    /// Connects to a TCP server.
    /// </summary>
    /// <param name="host">
    /// The server host name or address.
    /// </param>
    /// <param name="port">
    /// The server port.
    /// </param>
    /// <returns>
    /// The established connection.
    /// </returns>
    /// <remarks>
    /// This method blocks until the connection succeeds or fails.
    /// </remarks>
    public TcpConnection Connect(
        string host,
        int port)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        if (port <= IPEndPoint.MinPort ||
            port > IPEndPoint.MaxPort)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        if (State != ConnectionState.Disconnected)
        {
            throw new InvalidOperationException(
                "The TCP client is already connected or connecting.");
        }

        _state = ConnectionState.Connecting;

        Socket socket = new(
            SocketType.Stream,
            ProtocolType.Tcp);

        try
        {
            socket.Connect(host, port);

            Connection = new TcpConnection(socket);
            _state = ConnectionState.Connected;

            return Connection;
        }
        catch
        {
            socket.Dispose();
            _state = ConnectionState.Disconnected;
            throw;
        }
    }

    /// <summary>
    /// Disconnects the client.
    /// </summary>
    public void Disconnect()
    {
        Connection?.Close();
        Connection = null;
        _state = ConnectionState.Disconnected;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Disconnect();
    }
}
