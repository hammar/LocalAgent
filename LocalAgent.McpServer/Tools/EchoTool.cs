using System.ComponentModel;
using ModelContextProtocol.Server;

namespace LocalAgent.McpServer.Tools;

[McpServerToolType]
public class EchoTool
{
    private readonly ILogger<EchoTool> _logger;

    public EchoTool(ILogger<EchoTool> logger)
    {
        _logger = logger;
        _logger.LogInformation("EchoTool initialized.");
    }

    [McpServerTool, Description("Echoes the message back to the client.")]
    public string Echo(string message)
    {
        _logger.LogInformation("Echoing message: {Message}", message);
        return $"Echo {message}";
    }
}