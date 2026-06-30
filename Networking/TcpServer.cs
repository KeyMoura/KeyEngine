using System.Net;
using System.Net.Sockets;

namespace KeyEngine.Networking;

/// <summary>
/// Listens for inbound TCP connections.
/// </summary>
public sealed class TcpServer
    : IDisposable
{
    private readonly List<TcpConnection> _connections = [];
    private Socket? _listener;

    /// <summary>
    /// Gets the current server state.
    /// </summary>
    public ConnectionState State { get; private set; }
        = ConnectionState.Disconnected;

    /// <summary>
    /// Gets the endpoint on which the server is listening.
    /// </summary>
    public EndPoint? LocalEndPoint => _listener?.LocalEndPoint;

    /// <summary>
    /// Gets the connections accepted by the server.
    /// </summary>
    public IReadOnlyList<TcpConnection> Connections => _connections;

    internal TcpServer()
    {
    }

    /// <summary>
    /// Starts listening on the specified endpoint.
    /// </summary>
    /// <param name="endPoint">
    /// The local endpoint to bind.
    /// </param>
    /// <param name="backlog">
    /// The maximum pending connection queue length.
    /// </param>
    public void Start(
        IPEndPoint endPoint,
        int backlog = 100)
    {
        ArgumentNullException.ThrowIfNull(endPoint);

        if (backlog <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(backlog));
        }

        if (State != ConnectionState.Disconnected)
        {
            throw new InvalidOperationException(
                "The TCP server is already listening.");
        }

        Socket listener = new(
            endPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        try
        {
            listener.Bind(endPoint);
            listener.Listen(backlog);

            _listener = listener;
            State = ConnectionState.Listening;
        }
        catch
        {
            listener.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Waits for and accepts the next inbound connection.
    /// </summary>
    /// <returns>
    /// The accepted connection.
    /// </returns>
    public TcpConnection Accept()
    {
        if (State != ConnectionState.Listening ||
            _listener is null)
        {
            throw new InvalidOperationException(
                "The TCP server is not listening.");
        }

        TcpConnection connection = new(
            _listener.Accept());

        _connections.Add(connection);

        return connection;
    }

    /// <summary>
    /// Stops listening and closes accepted connections.
    /// </summary>
    public void Stop()
    {
        if (State == ConnectionState.Disconnected)
        {
            return;
        }

        _listener?.Dispose();
        _listener = null;

        foreach (TcpConnection connection in _connections)
        {
            connection.Close();
        }

        _connections.Clear();
        State = ConnectionState.Disconnected;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Stop();
    }
}
