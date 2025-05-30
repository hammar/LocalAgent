using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;

namespace LocalAgent.ApiService;

public class ChatHub : Hub
{
    private ILogger<ChatHub> _logger;
    private readonly IChatClient _chatClient;
    private readonly IToolProvider _toolProvider;

    public ChatHub(ILogger<ChatHub> logger, IChatClient chatClient, IToolProvider toolProvider)
    {
        _logger = logger;
        _chatClient = chatClient;
        _toolProvider = toolProvider;
    }

    public async Task ProcessUserPrompt(List<ChatMessage> chatHistory)
    {
        string originatingConnectionId = Context.ConnectionId;
        var chatOptions = new ChatOptions
        {
            ToolMode = ChatToolMode.Auto,
            Tools = _toolProvider.GetTools().ToList(),
        };
        var responses = _chatClient.GetStreamingResponseAsync(chatHistory, chatOptions);
        await foreach (ChatResponseUpdate response in responses)
        {
            await Clients.Client(originatingConnectionId).SendAsync("ProcessAgentResponse", response);
        }
    }
}