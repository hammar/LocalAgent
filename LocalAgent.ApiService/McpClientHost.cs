using Microsoft.Extensions.ServiceDiscovery;
using ModelContextProtocol.Client;

namespace LocalAgent.ApiService;

/// <summary>
/// Hosted service that manages the lifecycle of an MCP client.
/// </summary>
public class McpClientHost : IHostedService, IAsyncDisposable
{
    // Null-forgiving operator to suppress CS8618; because .NET startup guarantees 
    // that IHostedService.StartAsync is called before Client is accessed.
    private McpClient _client = null!;
    private readonly IClientTransport _transport;

    public McpClient Client => _client;

    public McpClientHost(IClientTransport transport)
    {
        _transport = transport;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client = await McpClient.CreateAsync(_transport, cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None);
    }
}