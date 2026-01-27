using LocalAgent.ApiService;
using Microsoft.Extensions.AI;
using LocalAgent.ApiService.Data;
using Microsoft.EntityFrameworkCore;
using LocalAgent.ServiceDefaults;
using Swashbuckle.AspNetCore;
using OllamaSharp;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Client;
using Azure;
using Azure.AI.Inference;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Register AppDbContext with SQLite connection string from Aspire
builder.AddSqliteDbContext<AppDbContext>(name: "sqlite");

builder.Services.AddSignalR();

// Configure AI based on provider setting
var aiConfig = builder.Configuration.GetSection("AIConfig").Get<AIConfig>() 
    ?? throw new InvalidOperationException("AIConfig section is missing in configuration.");

if (aiConfig.IsLocalProvider())
{
    // The connection name used for Ollama
    const string OllamaConnectionName = "ollamaModel";
    
    // Configure increased timeout for local Ollama LLM requests BEFORE creating the client
    // Local LLMs can be slower, especially on low-performance machines
    // The HttpClient name follows the pattern: {connectionName}_httpClient
    const int MinimumTimeoutSeconds = 1;
    var timeoutSeconds = Math.Max(MinimumTimeoutSeconds, aiConfig.TimeoutSeconds);
    
    builder.Services.AddHttpClient($"{OllamaConnectionName}_httpClient", client =>
    {
        // Set HttpClient timeout to infinite so the resilience handler's timeout is used
        client.Timeout = Timeout.InfiniteTimeSpan;
    })
    .AddStandardResilienceHandler(options =>
    {
        // Configure the resilience handler with the desired timeout
        // This overrides the global 10-second default for this specific client
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    });
    
    builder.AddOllamaApiClient(OllamaConnectionName)
        .AddChatClient()
        .UseFunctionInvocation()
        .UseOpenTelemetry(configure: t => t.EnableSensitiveData = true);
}
else
{
    builder.AddAzureChatCompletionsClient(connectionName: "ai-foundry")
        .AddChatClient(aiConfig.ModelId)
        .UseFunctionInvocation()
        .UseOpenTelemetry(configure: t => t.EnableSensitiveData = true);
}

builder.Services.AddSingleton<IClientTransport>(sp =>
{
    var mcpServerEndpoint = builder.Configuration.GetConnectionString("McpServer")!;
    var transportOptions = new HttpClientTransportOptions { Endpoint = new Uri(mcpServerEndpoint) };
    return new HttpClientTransport(transportOptions);
});
builder.Services.AddSingleton<McpClientHost>();

// The below is to ensure late initialization of the _client field in McpClientHost, 
// since we do not want McpClient.CreateAsync slowing down program start.
builder.Services.AddHostedService(sp => sp.GetRequiredService<McpClientHost>()); 

builder.Services.AddSingleton<IToolProvider, DefaultToolProvider>();
builder.Services.AddSingleton<IToolProvider, McpToolProvider>();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddSwaggerGen();   

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Seed development data
if (app.Environment.IsDevelopment())
{
    DbInitializer.Initialize(app);
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.MapHub<ChatHub>("/chathub");

app.Run();
