using System.Net;
using System.Net.Sockets;

namespace KeyEngine.Networking;

/// <summary>
/// Represents an established TCP connection.
/// </summary>
/// <remarks>
/// Instances are not guaranteed to be thread-safe.
/// </remarks>
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
    /// <remarks>
    /// This method blocks until bytes are sent or the operation fails.
    /// </remarks>
    public int Send(ReadOnlySpan<byte> data)
    {
        EnsureConnected();

        try
        {
            return _socket.Send(data);
        }
        catch (Exception exception) when (
            exception is SocketException or
            IOException or
            ObjectDisposedException)
        {
            Close();
            throw;
        }
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
    /// <remarks>
    /// This method blocks until bytes are received, the remote endpoint closes
    /// the connection, or the operation fails.
    /// </remarks>
    public int Receive(Span<byte> buffer)
    {
        EnsureConnected();

        int received;

        try
        {
            received = _socket.Receive(buffer);
        }
        catch (Exception exception) when (
            exception is SocketException or
            IOException or
            ObjectDisposedException)
        {
            Close();
            throw;
        }

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

        State = ConnectionState.Disconnected;
        _socket.Dispose();
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
