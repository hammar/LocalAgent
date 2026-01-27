using CommunityToolkit.Aspire.Hosting;
using LocalAgent.ServiceDefaults;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Read AI configuration to determine if Ollama should be launched
AIConfig aiConfig = builder.Configuration.GetSection("AIConfig").Get<AIConfig>() 
    ?? throw new InvalidOperationException("AIConfig section is missing in configuration.");
bool useLocalProvider = aiConfig.IsLocalProvider();

var sqlite = builder.AddSqlite("sqlite").WithSqliteWeb();

var mcpServer = builder.AddProject<Projects.LocalAgent_McpServer>("mcpserver")
    .WithHttpHealthCheck("/health");

var apiService = builder.AddProject<Projects.LocalAgent_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(sqlite)
    .WithEnvironment("ConnectionStrings__McpServer", mcpServer.GetEndpoint("http"))
    .WithEnvironment("AIConfig__Provider", aiConfig.Provider.ToString())
    .WithEnvironment("AIConfig__ModelId", aiConfig.ModelId);

// Only add Ollama resource if using Local provider
if (useLocalProvider)
{
    var ollamaModel = builder.AddOllama("ollama")
        .WithDataVolume()
        .AddModel("ollamaModel", aiConfig.ModelId);
    apiService.WithReference(ollamaModel);
}
else
{
    var aiFoundry = builder.AddConnectionString("ai-foundry");
    apiService.WithReference(aiFoundry);
}

builder.AddProject<Projects.LocalAgent_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
