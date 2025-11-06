using System.Reflection.Metadata.Ecma335;
using LocalAgent.ApiService.Data;
using LocalAgent.ApiService.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;

namespace LocalAgent.ApiService;

public class ChatHub : Hub
{
    private ILogger<ChatHub> _logger;
    private readonly IChatClient _chatClient;
    private readonly AppDbContext _dbContext;
    private readonly IToolProvider _toolProvider;

    public ChatHub(ILogger<ChatHub> logger, IChatClient chatClient, AppDbContext dbContext, IToolProvider toolProvider)
    {
        _logger = logger;
        _chatClient = chatClient;
        _dbContext = dbContext;
        _toolProvider = toolProvider;
    }

    public async Task ProcessUserPrompt(Guid AgentId, List<ChatMessage> chatHistory)
    {
        // Return message to the originating client
        string originatingConnectionId = Context.ConnectionId;

        // Get system prompt and prepend
        var agent = _dbContext.Agents.Where(agent => agent.Id == AgentId).First();
        ChatMessage systemPromptMessage = new ChatMessage(ChatRole.System, agent.SystemInstructions);
        chatHistory.Insert(0, systemPromptMessage);
        
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

    public async Task StartAgent(Guid AgentId)
    {
        string originatingConnectionId = Context.ConnectionId;
        var agent = _dbContext.Agents.Where(agent => agent.Id == AgentId).First();
        ChatMessage systemInstructionsMessage = new ChatMessage(ChatRole.System, agent.SystemInstructions);
        ChatMessage startupMessage = new ChatMessage(ChatRole.User, "You are now starting to run. Please respond with your first greeting message.");
        List<ChatMessage> chatHistory = [systemInstructionsMessage, startupMessage];
        var responses = _chatClient.GetStreamingResponseAsync(chatHistory);
        await foreach (ChatResponseUpdate response in responses)
        {
            await Clients.Client(originatingConnectionId).SendAsync("ProcessAgentResponse", response);
        }
    }
}