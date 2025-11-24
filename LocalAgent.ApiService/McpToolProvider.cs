using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.VisualBasic;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace LocalAgent.ApiService;

public class McpToolProvider : IToolProvider
{
    private McpClient _client;

    public McpToolProvider(McpClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<AITool>> GetToolsAsync()
    {
        
        return await _client.ListToolsAsync();
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