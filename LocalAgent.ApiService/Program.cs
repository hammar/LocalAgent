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

builder.AddKeyedOllamaSharpChatClient("llama32");
builder.Services.AddChatClient(sp => sp.GetRequiredKeyedService<IChatClient>("llama32"))
                .UseFunctionInvocation()
                .UseOpenTelemetry(configure: t => t.EnableSensitiveData = true)
                .UseLogging();


builder.Services.AddSingleton<IClientTransport>(sp =>
{
    var mcpServerEndpoint = builder.Configuration.GetConnectionString("McpServer")!;
    var transportOptions = new HttpClientTransportOptions { Endpoint = new Uri(mcpServerEndpoint) };
    return new HttpClientTransport(transportOptions);
});
builder.Services.AddSingleton<McpClientHost>();
builder.Services.AddHostedService<McpClientHost>();
builder.Services.AddSingleton<McpClient>(p => p.GetRequiredService<McpClientHost>().Client);

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
