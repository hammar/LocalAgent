using System;
using Microsoft.Extensions.AI;

namespace LocalAgent.ApiService;

public class DefaultToolProvider : IToolProvider
{
    private readonly ILogger<DefaultToolProvider> _logger;

    public DefaultToolProvider(ILogger<DefaultToolProvider> logger)
    {
        _logger = logger;
    }

    public IEnumerable<AITool> GetTools()
    {
        return [AIFunctionFactory.Create(LogDebugMessage, "LogDebugMessage", "Logs a standard debug message.")];
    }

    public Task<IEnumerable<AITool>> GetToolsAsync()
    {
        return Task.FromResult(GetTools());
    }

    private void LogDebugMessage()
    {
        _logger.LogDebug("{MethodName} was called", nameof(LogDebugMessage));
    }
}
