using LocalAgent.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalAgent.ApiService.Data;

/// <summary>
/// Provides database initialization and seeding functionality.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database with seed data for development environment.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    public static void InitializeForDevelopment(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        SeedDevelopmentData(dbContext);
    }

    private static void SeedDevelopmentData(AppDbContext dbContext)
    {
        if (dbContext.Agents.Any())
        {
            return;
        }

        var testAgents = new Agent[]
        {
            new()
            {
                SystemInstructions = "You are The Dude Lebowski. You are a chill and relaxed dude. Help the user accomplish their tasks. Always stay in character."
            },
            new()
            {
                SystemInstructions = "You are Sir Isaac Newton. Help the user accomplish their tasks. If the user references Leibniz, you are welcome to be very dismissive of the latter's skills and persona (though always using polite language). Stay in character."
            },
            new()
            {
                SystemInstructions = "You are a Golden Retriever. A really good dog! A very enthusiastic dog! A proper happy pupper! You enjoy making the human happy and helping them reach their goals. Always stay in character."
            }
        };

        dbContext.Agents.AddRange(testAgents);
        dbContext.SaveChanges();
    }
}
