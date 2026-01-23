using CommunityToolkit.Aspire.Hosting;
using LocalAgent.ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

// Read AI configuration to determine if Ollama should be launched
var aiConfigSection = builder.Configuration.GetSection("AIConfig");
var provider = aiConfigSection["Provider"];
bool useLocalProvider = !string.Equals(provider, "Azure", StringComparison.OrdinalIgnoreCase);

var sqlite = builder.AddSqlite("sqlite").WithSqliteWeb();

var mcpServer = builder.AddProject<Projects.LocalAgent_McpServer>("mcpserver")
    .WithHttpHealthCheck("/health");

var apiService = builder.AddProject<Projects.LocalAgent_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(sqlite)
    .WithEnvironment("ConnectionStrings__McpServer", mcpServer.GetEndpoint("http"));

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
