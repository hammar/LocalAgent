using System.Diagnostics;
using Microsoft.Extensions.AI;

namespace LocalAgent.ApiService;

public class McpToolProvider : IToolProvider
{
    private McpClientHost _mcpClientHost;

    private readonly ActivitySource _source;

    public McpToolProvider(IHostEnvironment env, McpClientHost mcpClientHost)
    {
        _mcpClientHost = mcpClientHost;
        _source = new ActivitySource(env.ApplicationName);
    }

    public async Task<IEnumerable<AITool>> GetToolsAsync()
    {
        using var activity = _source.StartActivity("get_tools");
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