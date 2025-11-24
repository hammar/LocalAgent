using LocalAgent.ApiService.Models;

namespace LocalAgent.Tests;

public class AgentModelTests
{
    [Fact]
    public void Agent_Should_Have_Name_Property()
    {
        // Arrange
        var agent = new Agent();
        var expectedName = "Test Agent";

        // Act
        agent.Name = expectedName;

        // Assert
        Assert.Equal(expectedName, agent.Name);
    }

    [Fact]
    public void Agent_Name_Should_Default_To_Empty_String()
    {
        // Arrange & Act
        var agent = new Agent();

        // Assert
        Assert.Equal(string.Empty, agent.Name);
    }

    [Fact]
    public void Agent_Should_Have_All_Required_Properties()
    {
        // Arrange
        var agent = new Agent
        {
            Name = "Test Agent",
            SystemInstructions = "You are a helpful assistant."
        };

        // Assert
        Assert.NotEqual(Guid.Empty, agent.Id);
        Assert.Equal("Test Agent", agent.Name);
        Assert.Equal("You are a helpful assistant.", agent.SystemInstructions);
    }
}
