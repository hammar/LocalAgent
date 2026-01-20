using Azure.Identity;
using Microsoft.Graph;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Microsoft Graph client with InteractiveBrowserCredential.
// This brings up a login prompt when the MCP is called for the first time.
builder.Services.AddSingleton<GraphServiceClient>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();

    var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
    {
        TenantId = "common", // Support both personal and work/school accounts
        ClientId = "8a8525ed-8a70-4eeb-9aed-f04448b4764f", // The LocalAgent Azure AD app registration
        RedirectUri = new Uri("http://localhost")
    });

    // Tasks.Read and Tasks.ReadWrite provide the narrowest permissions for To-Do operations.
    // Any additional scopes required by other MCP tools can be added here as such tools are added.
    string[] scopes = ["Tasks.Read", "Tasks.ReadWrite"];
    
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