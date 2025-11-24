using CommunityToolkit.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var sqlite = builder.AddSqlite("sqlite").WithSqliteWeb();

var llama32 = builder.AddOllama("ollama")
    .WithDataVolume()
    .AddModel("llama32", "llama3.2");

var mcpServer = builder.AddProject<Projects.LocalAgent_McpServer>("mcpserver")
    .WithHttpHealthCheck("/health");

var apiService = builder.AddProject<Projects.LocalAgent_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(llama32)
    .WithReference(sqlite)
    .WithEnvironment("ConnectionStrings__McpServer", mcpServer.GetEndpoint("http"));

builder.AddProject<Projects.LocalAgent_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
