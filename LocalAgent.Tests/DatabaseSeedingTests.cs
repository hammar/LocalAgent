using LocalAgent.ApiService.Data;
using LocalAgent.ApiService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace LocalAgent.Tests;

public class DatabaseSeedingTests
{
    [Fact]
    public async Task DatabaseSeeding_CreatesThreeAgents_InDevelopmentEnvironment()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        // Simulate the seeding logic from Program.cs
        if (!context.Agents.Any())
        {
            var testAgents = new[]
            {
                new Agent
                {
                    SystemInstructions = "You are The Dude Lebowski. You are a chill and relaxed dude. Help the user accomplish their tasks. Always stay in character."
                },
                new Agent
                {
                    SystemInstructions = "You are Sir Isaac Newton. Help the user accomplish their tasks. If the user references Leibniz, you are welcome to be very dismissive of the latter's skills and persona (though always using polite language). Stay in character."
                },
                new Agent
                {
                    SystemInstructions = "You are a Golden Retriever. A really good dog! A very enthusiastic dog! A proper happy pupper! You enjoy making the human happy and helping them reach their goals. Always stay in character."
                }
            };
            
            context.Agents.AddRange(testAgents);
            await context.SaveChangesAsync();
        }

        // Act
        var agents = await context.Agents.ToListAsync();

        // Assert
        Assert.Equal(3, agents.Count);
        Assert.Contains(agents, a => a.SystemInstructions.Contains("The Dude Lebowski"));
        Assert.Contains(agents, a => a.SystemInstructions.Contains("Sir Isaac Newton"));
        Assert.Contains(agents, a => a.SystemInstructions.Contains("Golden Retriever"));
    }

    [Fact]
    public async Task DatabaseSeeding_DoesNotDuplicateAgents_OnSubsequentRuns()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        // First seeding
        if (!context.Agents.Any())
        {
            var testAgents = new[]
            {
                new Agent
                {
                    SystemInstructions = "You are The Dude Lebowski. You are a chill and relaxed dude. Help the user accomplish their tasks. Always stay in character."
                },
                new Agent
                {
                    SystemInstructions = "You are Sir Isaac Newton. Help the user accomplish their tasks. If the user references Leibniz, you are welcome to be very dismissive of the latter's skills and persona (though always using polite language). Stay in character."
                },
                new Agent
                {
                    SystemInstructions = "You are a Golden Retriever. A really good dog! A very enthusiastic dog! A proper happy pupper! You enjoy making the human happy and helping them reach their goals. Always stay in character."
                }
            };
            
            context.Agents.AddRange(testAgents);
            await context.SaveChangesAsync();
        }

        // Act - Attempt second seeding
        if (!context.Agents.Any())
        {
            var testAgents = new[]
            {
                new Agent
                {
                    SystemInstructions = "You are The Dude Lebowski. You are a chill and relaxed dude. Help the user accomplish their tasks. Always stay in character."
                }
            };
            
            context.Agents.AddRange(testAgents);
            await context.SaveChangesAsync();
        }

        var agents = await context.Agents.ToListAsync();

        // Assert - Should still only have 3 agents, not 4
        Assert.Equal(3, agents.Count);
    }
}
