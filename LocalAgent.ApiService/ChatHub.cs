using Microsoft.AspNetCore.SignalR;

namespace LocalAgent.ApiService;

public class ChatHub : Hub
{
    public async Task SendMessage(string message)
    {
        var response = $"I am the server. You sent '{message}'.";
        await Clients.All.SendAsync("ReceiveMessage", response);
    }
}