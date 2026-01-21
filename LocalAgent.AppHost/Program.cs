using CommunityToolkit.Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var sqliteBuilder = builder.AddSqlite("sqlite");
var sqliteResource = sqliteBuilder.Resource;

var sqlite = sqliteBuilder.WithSqliteWeb(containerBuilder =>
{
    // Fix for Windows: sqlite-web expects the database file path as a command-line argument
    // The database directory is mounted to /data, so we need to pass /data/{filename}
    // 
    // Note: Using reflection to access internal DatabaseFileName property because it's not exposed publicly.
    // If this breaks after a library update, check if DatabaseFileName has been made public or renamed.
    try
    {
        var databaseFileNameProperty = sqliteResource.GetType().GetProperty("DatabaseFileName", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (databaseFileNameProperty == null)
        {
            throw new InvalidOperationException(
                "Unable to access DatabaseFileName property on SqliteResource. " +
                "The CommunityToolkit.Aspire.Hosting.SQLite library structure may have changed. " +
                "Please check the library version and update this code accordingly, or file an issue at " +
                "https://github.com/CommunityToolkit/Aspire/issues");
        }
        
        var databaseFileName = databaseFileNameProperty.GetValue(sqliteResource) as string;
        
        if (!string.IsNullOrEmpty(databaseFileName))
        {
            var dbPath = $"/data/{databaseFileName}";
            containerBuilder.WithArgs(dbPath);
        }
        else
        {
            throw new InvalidOperationException(
                "Database filename is null or empty. Ensure AddSqlite() is called with valid parameters.");
        }
    }
    catch (InvalidOperationException)
    {
        // Re-throw our specific exceptions
        throw;
    }
    catch (System.Reflection.TargetException ex)
    {
        throw new InvalidOperationException(
            $"Failed to get DatabaseFileName property value: {ex.Message}", ex);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException(
            $"Unexpected error while configuring SQLite Web container: {ex.Message}. " +
            $"This may indicate a compatibility issue with CommunityToolkit.Aspire.Hosting.SQLite.", ex);
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
