using LocalAgent.ApiService.Models;
using System.ComponentModel.DataAnnotations;

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

    [Fact]
    public void Agent_Should_Fail_Validation_With_Empty_Name()
    {
        // Arrange
        var agent = new Agent
        {
            Name = string.Empty,
            SystemInstructions = "Valid instructions"
        };
        var validationContext = new ValidationContext(agent);
        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(agent, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void Agent_Should_Fail_Validation_With_Empty_SystemInstructions()
    {
        // Arrange
        var agent = new Agent
        {
            Name = "Valid Name",
            SystemInstructions = string.Empty
        };
        var validationContext = new ValidationContext(agent);
        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(agent, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("SystemInstructions"));
    }

    [Fact]
    public void Agent_Should_Pass_Validation_With_Valid_Name_And_SystemInstructions()
    {
        // Arrange
        var agent = new Agent
        {
            Name = "Valid Name",
            SystemInstructions = "Valid instructions"
        };
        var validationContext = new ValidationContext(agent);
        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(agent, validationContext, validationResults, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(validationResults);
    }
}
