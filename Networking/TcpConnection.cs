using System.Net;
using System.Net.Sockets;

namespace KeyEngine.Networking;

/// <summary>
/// Represents an established TCP connection.
/// </summary>
public sealed class TcpConnection
    : IDisposable
{
    private readonly Socket _socket;

    /// <summary>
    /// Gets the current connection state.
    /// </summary>
    public ConnectionState State { get; private set; }
        = ConnectionState.Connected;

    /// <summary>
    /// Gets the local network endpoint.
    /// </summary>
    public EndPoint? LocalEndPoint { get; }

    /// <summary>
    /// Gets the remote network endpoint.
    /// </summary>
    public EndPoint? RemoteEndPoint { get; }

    internal TcpConnection(Socket socket)
    {
        ArgumentNullException.ThrowIfNull(socket);

        if (!socket.Connected)
        {
            throw new ArgumentException(
                "The socket must be connected.",
                nameof(socket));
        }

        _socket = socket;
        LocalEndPoint = socket.LocalEndPoint;
        RemoteEndPoint = socket.RemoteEndPoint;
    }

    /// <summary>
    /// Sends bytes over the connection.
    /// </summary>
    /// <param name="data">
    /// The bytes to send.
    /// </param>
    /// <returns>
    /// The number of bytes sent.
    /// </returns>
    public int Send(ReadOnlySpan<byte> data)
    {
        EnsureConnected();

        return _socket.Send(data);
    }

    /// <summary>
    /// Receives bytes from the connection.
    /// </summary>
    /// <param name="buffer">
    /// The buffer that receives the bytes.
    /// </param>
    /// <returns>
    /// The number of bytes received, or zero when the remote endpoint closes
    /// the connection.
    /// </returns>
    public int Receive(Span<byte> buffer)
    {
        EnsureConnected();

        int received = _socket.Receive(buffer);

        if (received == 0)
        {
            Close();
        }

        return received;
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    public void Close()
    {
        if (State == ConnectionState.Disconnected)
        {
            return;
        }

        _socket.Dispose();
        State = ConnectionState.Disconnected;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Close();
    }

    private void EnsureConnected()
    {
        if (State != ConnectionState.Connected)
        {
            throw new InvalidOperationException(
                "The TCP connection is not connected.");
        }
    }
}
