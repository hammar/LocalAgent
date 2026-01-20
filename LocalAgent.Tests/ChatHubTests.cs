using LocalAgent.ApiService;
using LocalAgent.ApiService.Data;
using LocalAgent.ApiService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace LocalAgent.Tests;

public class ChatHubTests
{
    [Fact]
    public void GetSystemInstructionsWithDateTime_AppendsCurrentDateTime()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChatHub>>();
        var mockChatClient = new Mock<IChatClient>();
        var mockToolProviders = new List<IToolProvider>();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        
        using var context = new AppDbContext(options);
        
        var chatHub = new ChatHub(mockLogger.Object, mockChatClient.Object, context, mockToolProviders);
        
        // Act - Use reflection to call the private method
        var methodInfo = typeof(ChatHub).GetMethod("GetSystemInstructionsWithDateTime", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var result = methodInfo?.Invoke(chatHub, new object[] { "You are a helpful assistant." }) as string;
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("You are a helpful assistant.", result);
        Assert.Contains("Current date and time (UTC):", result);
        
        // Verify the date format (yyyy-MM-dd HH:mm:ss)
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", result);
    }

    [Fact]
    public void GetSystemInstructionsWithDateTime_PreservesOriginalInstructions()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChatHub>>();
        var mockChatClient = new Mock<IChatClient>();
        var mockToolProviders = new List<IToolProvider>();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        
        using var context = new AppDbContext(options);
        
        var chatHub = new ChatHub(mockLogger.Object, mockChatClient.Object, context, mockToolProviders);
        var originalInstructions = "You are The Dude Lebowski. You are a chill and relaxed dude.";
        
        // Act - Use reflection to call the private method
        var methodInfo = typeof(ChatHub).GetMethod("GetSystemInstructionsWithDateTime", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var result = methodInfo?.Invoke(chatHub, new object[] { originalInstructions }) as string;
        
        // Assert
        Assert.NotNull(result);
        Assert.StartsWith(originalInstructions, result);
        Assert.Contains("\n\nCurrent date and time (UTC):", result);
    }
}
