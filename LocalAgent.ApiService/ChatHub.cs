using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;

namespace LocalAgent.ApiService;

public class ChatHub : Hub
{
    private readonly IChatClient _chatClient;

    public ChatHub(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task SendMessage(string message)
    {
        var response = await _chatClient.GetResponseAsync(message);
        await Clients.All.SendAsync("ReceiveMessage", response.Text);
    }
}