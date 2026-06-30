namespace KeyEngine.Networking;

/// <summary>
/// Creates and owns TCP clients and servers.
/// </summary>
public sealed class NetworkManager
    : IDisposable
{
    private readonly List<TcpClient> _clients = [];
    private readonly List<TcpServer> _servers = [];

    /// <summary>
    /// Gets the clients created by the manager.
    /// </summary>
    public IReadOnlyList<TcpClient> Clients => _clients;

    /// <summary>
    /// Gets the servers created by the manager.
    /// </summary>
    public IReadOnlyList<TcpServer> Servers => _servers;

    /// <summary>
    /// Creates a TCP client owned by the manager.
    /// </summary>
    /// <returns>
    /// The created client.
    /// </returns>
    public TcpClient CreateClient()
    {
        TcpClient client = new();

        _clients.Add(client);

        return client;
    }

    /// <summary>
    /// Creates a TCP server owned by the manager.
    /// </summary>
    /// <returns>
    /// The created server.
    /// </returns>
    public TcpServer CreateServer()
    {
        TcpServer server = new();

        _servers.Add(server);

        return server;
    }

    /// <summary>
    /// Closes and releases all clients and servers owned by the manager.
    /// </summary>
    public void CloseAll()
    {
        foreach (TcpClient client in _clients)
        {
            client.Dispose();
        }

        foreach (TcpServer server in _servers)
        {
            server.Dispose();
        }

        _clients.Clear();
        _servers.Clear();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        CloseAll();
    }
}
