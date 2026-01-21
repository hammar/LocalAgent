using CommunityToolkit.Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var sqliteBuilder = builder.AddSqlite("sqlite");
var sqliteResource = sqliteBuilder.Resource;

var sqlite = sqliteBuilder.WithSqliteWeb(containerBuilder =>
{
    // Fix for Windows: sqlite-web expects the database file path as a command-line argument
    // The database directory is mounted to /data, so we need to pass /data/{filename}
    // Using reflection to access internal DatabaseFileName property
    var databaseFileNameProperty = sqliteResource.GetType().GetProperty("DatabaseFileName", 
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    var databaseFileName = databaseFileNameProperty?.GetValue(sqliteResource) as string;
    
    if (!string.IsNullOrEmpty(databaseFileName))
    {
        var dbPath = $"/data/{databaseFileName}";
        containerBuilder.WithArgs(dbPath);
    }
});

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
