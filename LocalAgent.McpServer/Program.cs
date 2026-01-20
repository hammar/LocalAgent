using Azure.Identity;
using LocalAgent.McpServer;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Microsoft Graph options
builder.Services.Configure<MicrosoftGraphOptions>(
    builder.Configuration.GetSection(MicrosoftGraphOptions.SectionName));

// Configure Microsoft Graph client with environment-aware authentication
builder.Services.AddSingleton<GraphServiceClient>(sp =>
{
    var graphOptions = sp.GetRequiredService<IOptions<MicrosoftGraphOptions>>().Value;
    var logger = sp.GetRequiredService<ILogger<Program>>();
    
    // Determine if we're in local development or production based on configuration
    var isLocalDevelopment = string.IsNullOrWhiteSpace(graphOptions.ClientId) || 
                            string.IsNullOrWhiteSpace(graphOptions.TenantId);
    
    Azure.Core.TokenCredential credential;
    string[] scopes = graphOptions.Scopes?.Any() == true 
        ? graphOptions.Scopes 
        : new[] { "https://graph.microsoft.com/.default" };
    
    if (isLocalDevelopment)
    {
        // Local development: Use DefaultAzureCredential
        // This will try multiple credential types in order:
        // 1. Environment variables (for CI/CD)
        // 2. Managed Identity (for Azure-hosted apps)
        // 3. Visual Studio/VS Code (for local development)
        // 4. Azure CLI (for local development)
        logger.LogInformation("Using DefaultAzureCredential for local development");
        credential = new DefaultAzureCredential();
    }
    else
    {
        // Production: Use ClientSecretCredential for app-only access or OBO flow
        logger.LogInformation("Using ClientSecretCredential for production with TenantId: {TenantId}, ClientId: {ClientId}", 
            graphOptions.TenantId, graphOptions.ClientId);
        
        if (string.IsNullOrWhiteSpace(graphOptions.ClientSecret))
        {
            throw new InvalidOperationException(
                "ClientSecret is required when TenantId and ClientId are configured. " +
                "Configure the secret using environment variables, Azure Key Vault, or user secrets.");
        }
        
        credential = new ClientSecretCredential(
            graphOptions.TenantId,
            graphOptions.ClientId,
            graphOptions.ClientSecret);
    }
    
    // Use Azure.Identity directly with Microsoft Graph
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