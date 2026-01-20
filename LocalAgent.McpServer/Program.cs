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

// Add HttpContextAccessor to access the current HTTP context
builder.Services.AddHttpContextAccessor();

// Configure Microsoft Graph client as scoped (per-request) to support OBO flow
builder.Services.AddScoped<GraphServiceClient>(sp =>
{
    var graphOptions = sp.GetRequiredService<IOptions<MicrosoftGraphOptions>>().Value;
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    
    // Try to get user token from Authorization header for OBO flow
    var userToken = httpContextAccessor.HttpContext?.Request.Headers.Authorization
        .FirstOrDefault()?.Replace("Bearer ", "");
    
    Azure.Core.TokenCredential credential;
    string[] scopes = graphOptions.Scopes?.Any() == true 
        ? graphOptions.Scopes 
        : new[] { "https://graph.microsoft.com/.default" };
    
    // Determine authentication mode based on configuration and user context
    var hasEntraAppConfig = !string.IsNullOrWhiteSpace(graphOptions.ClientId) && 
                           !string.IsNullOrWhiteSpace(graphOptions.TenantId);
    
    if (hasEntraAppConfig && !string.IsNullOrWhiteSpace(userToken))
    {
        // Production with user context: Use On-Behalf-Of flow
        logger.LogInformation("Using OnBehalfOfCredential for OBO flow with TenantId: {TenantId}, ClientId: {ClientId}", 
            graphOptions.TenantId, graphOptions.ClientId);
        
        if (string.IsNullOrWhiteSpace(graphOptions.ClientSecret))
        {
            throw new InvalidOperationException(
                "ClientSecret is required for On-Behalf-Of flow. " +
                "Configure the secret using environment variables, Azure Key Vault, or user secrets.");
        }
        
        // Use OnBehalfOfCredential to exchange the user token for a Graph token
        credential = new OnBehalfOfCredential(
            graphOptions.TenantId,
            graphOptions.ClientId,
            graphOptions.ClientSecret,
            userToken);
    }
    else if (hasEntraAppConfig)
    {
        // Production without user context: This is a security issue
        // Using ClientSecretCredential would grant app-level access, potentially allowing
        // the calling user to access data they shouldn't have permissions for.
        // In production, we require user authentication via OBO flow.
        throw new InvalidOperationException(
            "Production mode requires user authentication. " +
            "The Authorization header with a valid bearer token must be present in the request. " +
            "Falling back to ClientSecretCredential (app-only access) would be a security risk, " +
            "as it could grant users access to data beyond their permissions.");
    }
    else
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