using CommunityToolkit.Aspire.Hosting;
using LocalAgent.ServiceDefaults;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Read AI configuration to determine if Ollama should be launched
var aiConfig = builder.Configuration.GetSection("AIConfig").Get<AIConfig>() ?? new AIConfig();
bool useLocalProvider = aiConfig.IsLocalProvider();

var sqlite = builder.AddSqlite("sqlite").WithSqliteWeb();

var mcpServer = builder.AddProject<Projects.LocalAgent_McpServer>("mcpserver")
    .WithHttpHealthCheck("/health");

var apiService = builder.AddProject<Projects.LocalAgent_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(sqlite)
    .WithEnvironment("ConnectionStrings__McpServer", mcpServer.GetEndpoint("http"))
    .WithEnvironment("AIConfig__Provider", aiConfig.Provider.ToString())
    .WithEnvironment("AIConfig__Azure__Endpoint", aiConfig.Azure.Endpoint)
    .WithEnvironment("AIConfig__Azure__ModelId", aiConfig.Azure.ModelId)
    .WithEnvironment("AIConfig__Azure__ApiKey", aiConfig.Azure.ApiKey);

// Only add Ollama resource if using Local provider
if (useLocalProvider)
{
    var llama32 = builder.AddOllama("ollama")
        .WithDataVolume()
        .AddModel("llama32", "llama3.2");
    
    apiService.WithReference(llama32);
}

builder.AddProject<Projects.LocalAgent_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
