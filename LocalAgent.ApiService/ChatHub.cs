using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;

namespace LocalAgent.ApiService;

public class ChatHub : Hub
{
    private ILogger<ChatHub> _logger;
    private readonly IChatClient _chatClient;
    private readonly ChatOptions _chatOptions;

    public ChatHub(ILogger<ChatHub> logger, IChatClient chatClient)
    {
        _logger = logger;
        _chatClient = chatClient;

        _chatOptions = new()
        {
            ToolMode = ChatToolMode.RequireSpecific("LogIWasRun"),
            Tools = [AIFunctionFactory.Create(LogIWasRun, "LogIWasRun", "Logs that I was run")]
        };
    }

    public async Task SendMessage(string message)
    {
        var response = await _chatClient.GetResponseAsync(message, _chatOptions);
        await Clients.All.SendAsync("ReceiveMessage", response.Text);
    }

    private void LogIWasRun() 
    {
        _logger.LogInformation("I was run");
    }
}