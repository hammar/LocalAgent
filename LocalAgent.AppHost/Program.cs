var builder = DistributedApplication.CreateBuilder(args);

var llama32 = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithOpenWebUI()
    .AddModel("llama32", "llama3.2");

var apiService = builder.AddProject<Projects.LocalAgent_ApiService>("apiservice")
    .WithHttpsHealthCheck("/health")
    .WithReference(llama32);

builder.AddProject<Projects.LocalAgent_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpsHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
