using LocalAgent.ApiService;
using Microsoft.Extensions.AI;
using LocalAgent.ApiService.Data;
using Microsoft.EntityFrameworkCore;
using LocalAgent.ApiService.Models;
using Swashbuckle.AspNetCore;
using OllamaSharp;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Register AppDbContext with SQLite connection string from Aspire
builder.AddSqliteDbContext<AppDbContext>(name: "sqlite");

builder.Services.AddSignalR();

builder.AddKeyedOllamaApiClient("llama32")
    .AddChatClient()
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: t => t.EnableSensitiveData = true);

// Configure extended timeouts for local LLM operations
// The HttpClient naming convention is "{connectionName}_httpClient"
builder.Services.AddHttpClient("llama32_httpClient")
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        // Configure connection pooling to keep connections alive longer for LLM requests
        PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5)
    })
    .ConfigureHttpClient(client =>
    {
        // Set to infinite to let the resilience pipeline handle timeouts
        client.Timeout = Timeout.InfiniteTimeSpan;
    })
    .AddStandardResilienceHandler(options =>
    {
        // Increase total request timeout to 5 minutes for local LLM inference
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
        // Increase attempt timeout to 2 minutes, leaving room for retry logic
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);
    });

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
