using Azure.Identity;
using LocalAgent.McpServer;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Microsoft Graph options
builder.Services.Configure<MicrosoftGraphOptions>(
    builder.Configuration.GetSection(MicrosoftGraphOptions.SectionName));

// Configure Microsoft Graph client with DefaultAzureCredential for local development
builder.Services.AddSingleton<GraphServiceClient>(sp =>
{
    var configuration = builder.Configuration;
    var graphOptions = configuration.GetSection(MicrosoftGraphOptions.SectionName).Get<MicrosoftGraphOptions>();
    
    // Use DefaultAzureCredential for local development and Azure environments
    // This will try multiple credential types in order:
    // 1. Environment variables (for CI/CD)
    // 2. Managed Identity (for Azure-hosted apps)
    // 3. Visual Studio/VS Code (for local development)
    // 4. Azure CLI (for local development)
    var credential = new DefaultAzureCredential();
    
    // Define the scopes needed for Microsoft Graph
    var scopes = graphOptions?.Scopes?.Any() == true 
        ? graphOptions.Scopes 
        : new[] { "https://graph.microsoft.com/.default" };
    
    var authProvider = new BaseBearerTokenAuthenticationProvider(
        new TokenCredentialAccessTokenProvider(credential, scopes));
    
    return new GraphServiceClient(authProvider);
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

// Helper class to adapt Azure.Identity TokenCredential to Kiota's IAccessTokenProvider
internal class TokenCredentialAccessTokenProvider : IAccessTokenProvider
{
    private readonly Azure.Core.TokenCredential _credential;
    private readonly string[] _scopes;

    public TokenCredentialAccessTokenProvider(Azure.Core.TokenCredential credential, string[] scopes)
    {
        _credential = credential;
        _scopes = scopes;
    }

    public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        return GetAccessTokenAsync(cancellationToken);
    }

    public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator();

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var tokenRequestContext = new Azure.Core.TokenRequestContext(_scopes);
        var token = await _credential.GetTokenAsync(tokenRequestContext, cancellationToken);
        return token.Token;
    }
}