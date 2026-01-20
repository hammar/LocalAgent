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

    public async Task ProcessUserPrompt(Guid AgentId, List<ChatMessage> chatHistory)
    {
        // Return message to the originating client
        string originatingConnectionId = Context.ConnectionId;

        // Get system prompt and prepend
        var agent = _dbContext.Agents.Where(agent => agent.Id == AgentId).First();
        ChatMessage systemPromptMessage = new ChatMessage(ChatRole.System, agent.SystemInstructions);
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
        
        // Send a standard greeting message instead of calling the LLM
        string greetingText = $"Hello, my name is {agent.Name}, what can I do for you today?";
        
        // Create a single response update with the greeting
        var greetingResponse = new ChatResponseUpdate(ChatRole.Assistant, greetingText)
        {
            ResponseId = Guid.NewGuid().ToString(),
            FinishReason = ChatFinishReason.Stop
        };
        
        await Clients.Client(originatingConnectionId).SendAsync("ProcessAgentResponse", greetingResponse);
    }
}