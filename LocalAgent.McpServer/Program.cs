using Azure.Identity;
using Microsoft.Graph;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Microsoft Graph client with DefaultAzureCredential for local development
// This will authenticate using Azure CLI, Azure PowerShell, Visual Studio, or VS Code
builder.Services.AddSingleton<GraphServiceClient>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Using DefaultAzureCredential for Microsoft Graph authentication");
    
    // Use DefaultAzureCredential which will try multiple authentication methods:
    // 1. Environment variables (for CI/CD)
    // 2. Managed Identity (if deployed to Azure)
    // 3. Visual Studio/VS Code (for local development)
    // 4. Azure CLI (for local development)
    // 5. Azure PowerShell (for local development)
    var credential = new DefaultAzureCredential();
    
    // Use specific scopes for Microsoft To-Do
    // Tasks.Read and Tasks.ReadWrite provide the narrowest permissions for To-Do operations
    string[] scopes = new[] { "Tasks.Read", "Tasks.ReadWrite" };
    
    return new GraphServiceClient(credential, scopes);
});

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Configure HttpClientFactory for weather.gov API
builder.Services.AddHttpClient("WeatherApi", client =>
{
    client.BaseAddress = new Uri("https://api.weather.gov");
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
});

var app = builder.Build();

app.MapMcp();

app.MapHealthChecks("/health");

app.Run();