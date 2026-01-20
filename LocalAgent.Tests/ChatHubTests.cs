using LocalAgent.ApiService;
using LocalAgent.ApiService.Data;
using LocalAgent.ApiService.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Moq;

namespace LocalAgent.Tests;

public class ChatHubTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        var context = new AppDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        
        return context;
    }

    private (ChatHub chatHub, Mock<ISingleClientProxy> mockClientProxy, Mock<IChatClient> mockChatClient) SetupChatHub(AppDbContext context)
    {
        var mockLogger = new Mock<ILogger<ChatHub>>();
        var mockChatClient = new Mock<IChatClient>();
        var mockToolProviders = new List<IToolProvider>();
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<ISingleClientProxy>();
        var mockHubCallerContext = new Mock<HubCallerContext>();
        
        mockHubCallerContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        mockClients.Setup(c => c.Client(It.IsAny<string>())).Returns(mockClientProxy.Object);

        var chatHub = new ChatHub(mockLogger.Object, mockChatClient.Object, context, mockToolProviders)
        {
            Clients = mockClients.Object,
            Context = mockHubCallerContext.Object
        };

        return (chatHub, mockClientProxy, mockChatClient);
    }

    [Fact]
    public async Task StartAgent_SendsStandardGreeting_WithoutCallingLLM()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var testAgent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Test Agent",
            SystemInstructions = "You are a test agent."
        };
        context.Agents.Add(testAgent);
        await context.SaveChangesAsync();

        var (chatHub, mockClientProxy, mockChatClient) = SetupChatHub(context);

        ChatResponseUpdate? capturedResponse = null;
        mockClientProxy
            .Setup(c => c.SendCoreAsync(
                It.Is<string>(s => s == "ProcessAgentResponse"),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, object[], CancellationToken>((method, args, token) =>
            {
                capturedResponse = args[0] as ChatResponseUpdate;
            })
            .Returns(Task.CompletedTask);

        // Act
        await chatHub.StartAgent(testAgent.Id);

        // Assert
        Assert.NotNull(capturedResponse);
        Assert.Equal($"Hello, my name is {testAgent.Name}, what can I do for you today?", capturedResponse!.Text);
        Assert.Equal(ChatRole.Assistant, capturedResponse.Role);
        Assert.Equal(ChatFinishReason.Stop, capturedResponse.FinishReason);
        Assert.NotNull(capturedResponse.ResponseId);

        // Verify that the LLM was never called
        mockChatClient.Verify(
            c => c.GetStreamingResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StartAgent_IncludesAgentName_InGreeting()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var testAgent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Golden Retriever",
            SystemInstructions = "You are a good dog."
        };
        context.Agents.Add(testAgent);
        await context.SaveChangesAsync();

        var (chatHub, mockClientProxy, _) = SetupChatHub(context);

        ChatResponseUpdate? capturedResponse = null;
        mockClientProxy
            .Setup(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, object[], CancellationToken>((method, args, token) =>
            {
                capturedResponse = args[0] as ChatResponseUpdate;
            })
            .Returns(Task.CompletedTask);

        // Act
        await chatHub.StartAgent(testAgent.Id);

        // Assert
        Assert.NotNull(capturedResponse);
        Assert.Contains("Golden Retriever", capturedResponse!.Text);
    }
}

