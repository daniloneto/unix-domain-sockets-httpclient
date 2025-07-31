using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace UnixDomainSockets.HttpClient;

/// <summary>
/// Factory for <see cref="System.Net.Http.HttpClient"/> instances communicating over Unix Domain Sockets.
/// </summary>
public static class UnixHttpClientFactory
{
    /// <summary>
    /// Creates an <see cref="System.Net.Http.HttpClient"/> for communicating with a server listening on the provided socket path.
    /// </summary>
    /// <param name="socketPath">Path to the Unix domain socket.</param>
    /// <param name="timeout">Optional timeout applied to the client.</param>
    /// <param name="maxConnections">Maximum simultaneous connections.</param>
    /// <returns>Configured <see cref="System.Net.Http.HttpClient"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="socketPath"/> is null or empty.</exception>
    /// <exception cref="NotSupportedException">Thrown on non-Unix platforms.</exception>
    public static System.Net.Http.HttpClient For(string socketPath, TimeSpan? timeout = null, int maxConnections = 256)
    {
        if (string.IsNullOrWhiteSpace(socketPath))
        {
            throw new ArgumentException("Socket path must be provided", nameof(socketPath));
        }

        if (OperatingSystem.IsWindows())
        {
            throw new NotSupportedException("Unix Domain Sockets only supported on Unix-like systems");
        }

        var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                try
                {
                    var endpoint = new UnixDomainSocketEndPoint(socketPath);
                    await socket.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw new HttpRequestException($"Failed to connect to socket '{socketPath}'");
                }
            },
            MaxConnectionsPerServer = maxConnections
        };

        var client = new System.Net.Http.HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/"),
        };

        if (timeout.HasValue)
        {
            client.Timeout = timeout.Value;
        }

        return client;
    }

    /// <summary>
    /// Creates a read-only <see cref="System.Net.Http.HttpClient"/>. Only GET requests should be performed using this instance.
    /// This method is functionally identical to <see cref="For(string, TimeSpan?, int)"/> but exists to document intent.
    /// </summary>
    public static System.Net.Http.HttpClient ForReadonly(string socketPath, TimeSpan? timeout = null)
        => For(socketPath, timeout);
}
