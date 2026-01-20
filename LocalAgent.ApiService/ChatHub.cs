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
    private readonly IEnumerable<IToolProvider> _toolProviders;

    public ChatHub(ILogger<ChatHub> logger, IChatClient chatClient, AppDbContext dbContext, IEnumerable<IToolProvider> toolProviders)
    {
        _logger = logger;
        _chatClient = chatClient;
        _dbContext = dbContext;
        _toolProviders = toolProviders;
    }

    private string GetSystemInstructionsWithDateTime(string baseInstructions)
    {
        var currentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        return $"{baseInstructions}\n\nCurrent date and time (UTC): {currentDateTime}";
    }

    public async Task ProcessUserPrompt(Guid AgentId, List<ChatMessage> chatHistory)
    {
        // Return message to the originating client
        string originatingConnectionId = Context.ConnectionId;

        // Get system prompt and prepend
        var agent = _dbContext.Agents.Where(agent => agent.Id == AgentId).First();
        var systemInstructionsWithDateTime = GetSystemInstructionsWithDateTime(agent.SystemInstructions);
        ChatMessage systemPromptMessage = new ChatMessage(ChatRole.System, systemInstructionsWithDateTime);
        chatHistory.Insert(0, systemPromptMessage);
        
        // Get all available tools in parallel and flatten
        var toolRetrievalTasks = _toolProviders
            .Select(provider => provider.GetToolsAsync())
            .ToList();
        var allTools = (await Task.WhenAll(toolRetrievalTasks)).SelectMany(tools => tools).ToList();

        var chatOptions = new ChatOptions
        {
            ToolMode = ChatToolMode.Auto,
            Tools = allTools,
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
        var systemInstructionsWithDateTime = GetSystemInstructionsWithDateTime(agent.SystemInstructions);
        ChatMessage systemInstructionsMessage = new ChatMessage(ChatRole.System, systemInstructionsWithDateTime);
        ChatMessage startupMessage = new ChatMessage(ChatRole.User, "You are now starting to run. Please respond with your first greeting message.");
        List<ChatMessage> chatHistory = [systemInstructionsMessage, startupMessage];
        var responses = _chatClient.GetStreamingResponseAsync(chatHistory);
        await foreach (ChatResponseUpdate response in responses)
        {
            await Clients.Client(originatingConnectionId).SendAsync("ProcessAgentResponse", response);
        }
    }
}