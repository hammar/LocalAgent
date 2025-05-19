using LocalAgent.ApiService;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddSignalR();

builder.AddKeyedOllamaSharpChatClient("llama32");
builder.Services.AddChatClient(sp => sp.GetRequiredKeyedService<IChatClient>("llama32"))
                .UseFunctionInvocation()
                .UseOpenTelemetry(configure: t => t.EnableSensitiveData = true)
                .UseLogging();

builder.Services.AddSingleton<IToolProvider, DefaultToolProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHub<ChatHub>("/chathub");

app.MapDefaultEndpoints();

app.Run();
