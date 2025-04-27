var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.LocalAgent_ApiService>("apiservice")
    .WithHttpsHealthCheck("/health");

builder.AddProject<Projects.LocalAgent_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpsHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
