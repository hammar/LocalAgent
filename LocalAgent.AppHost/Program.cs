using CommunityToolkit.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var sqlite = builder.AddSqlite("sqlite", "../Data/", "dev.db").WithSqliteWeb();

var llama32 = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithOpenWebUI()
    .AddModel("llama32", "llama3.2");

var apiService = builder.AddProject<Projects.LocalAgent_ApiService>("apiservice")
    .WithHttpsHealthCheck("/health")
    .WithReference(llama32)
    .WithReference(sqlite);

builder.AddProject<Projects.LocalAgent_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpsHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
