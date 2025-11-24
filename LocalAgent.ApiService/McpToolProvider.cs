using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.VisualBasic;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace LocalAgent.ApiService;

public class McpToolProvider : IToolProvider
{
    private McpClientHost _mcpClientHost;

    public McpToolProvider(McpClientHost mcpClientHost)
    {
        _mcpClientHost = mcpClientHost;
    }

    public async Task<IEnumerable<AITool>> GetToolsAsync()
    {
        return await _mcpClientHost.Client.ListToolsAsync();
    }

    /// <summary>
    /// Blocking version of GetToolsAsync. Do not use.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<AITool> GetTools()
    {
        return GetToolsAsync().GetAwaiter().GetResult();
    }
}